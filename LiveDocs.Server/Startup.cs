using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AJP.MediatrEndpoints;
using AJP.MediatrEndpoints.EndpointRegistration;
using LiveDocs.Server.config;
using LiveDocs.Server.Hubs;
using LiveDocs.Server.Replacers;
using LiveDocs.Server.RequestHandlers;
using LiveDocs.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;

namespace LiveDocs.Server
{
    public class Startup
    {
        private const string WellKnownAzureAdEndpointUri = "https://login.microsoftonline.com";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<StronglyTypedConfig.LiveDocs>(Configuration.GetSection(StronglyTypedConfig.LiveDocs.ConfigKey));
            services.Configure<StronglyTypedConfig.AzureAd>(Configuration.GetSection(StronglyTypedConfig.AzureAd.ConfigKey));

            services.AddSingleton<IAzureIAMTokenFetcher, AzureIAMTokenFetcher>();
            services.AddSingleton<IAzureRMApiClient, AzureRMApiClient>();

            services.AddSingleton<IBackgroundTaskQueue>(ctx => new BackgroundTaskQueue(50));
            services.AddSingleton<IReplacementCache, InMemoryReplacementCache>();
            services.AddHostedService(sp => sp.GetService<IReplacementCache>() as InMemoryReplacementCache);
            
            services.AddSingleton<ISvcBusMessageInfoReplacer, SvcBusMessageInfoReplacer>();
            services.AddSingleton<ISqlStoredProcInfoReplacer, SqlStoredProcInfoReplacer>();
            services.AddSingleton<IStd18InfoReplacer, Std18InfoReplacer>();

            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            services.AddSingleton<IFileContentDownloader, FileContentDownloader>();
            services.AddSingleton<IHubGroupTracker, HubGroupTracker>();
            
            services.AddMediatrEndpoints(typeof(Startup));

            services.AddSingleton<IMarkdownReplacementAggregatorBackgroundService, MarkdownReplacementAggregatorBackgroundService>();
            services.AddHostedService(sp => sp.GetService<IMarkdownReplacementAggregatorBackgroundService>() as MarkdownReplacementAggregatorBackgroundService);

            services.AddHttpClient("AzureDevOpsClient", c =>
            {
                var PAT = Configuration["LiveDocs:azureDevOpsPat"];
                var authToken = Encoding.ASCII.GetBytes($"anything:{PAT}");
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(authToken));
            });

            services.AddHttpClient("AzureIAMClient", c =>
            {
                c.BaseAddress = new Uri(WellKnownAzureAdEndpointUri);
            });
            
            services.AddHttpClient("AzureRMClient", c =>
            {
                c.BaseAddress = new Uri(Configuration["LiveDocs:azureResourceManagementApiBaseUri"]);
            });

            services.AddHttpClient("PublicUrlClient", c =>
            {
            });

            
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");

                endpoints.MapHub<LatestMarkdownHub>("/latestmarkdownhub");
                
                endpoints.MapGroupOfEndpointsForAPath("/api/v1/resources", "LiveDocsResources")
                    .WithGet<GetResourceDocumentationsRequest, GetResourceDocumentationsResponse>("/")
                    .WithPost<ReloadResourceDocumentationFilesRequest, ReloadResourceDocumentationFilesResponse>(
                        "/reload", "Reload the Resource Documentation files i.e. to reflect recent changes.");
            });
        }
    }
}

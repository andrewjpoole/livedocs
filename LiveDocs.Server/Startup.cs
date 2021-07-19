using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AJP.MediatrEndpoints;
using AJP.MediatrEndpoints.EndpointRegistration;
using LiveDocs.Server.config;
using LiveDocs.Server.Replacers;
using LiveDocs.Server.RequestHandlers;
using LiveDocs.Server.Services;
using Microsoft.Extensions.Configuration;

namespace LiveDocs.Server
{
    public class Startup
    {
        private const string AllowSpecificOriginsPolicyName = "allowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<StronglyTypedConfig.LiveDocs>(Configuration.GetSection(StronglyTypedConfig.LiveDocs.ConfigKey));
            services.Configure<StronglyTypedConfig.AzureAd>(Configuration.GetSection(StronglyTypedConfig.AzureAd.ConfigKey));

            services.AddSingleton<IAzureIAMTokenFetcher, AzureIAMTokenFetcher>();
            services.AddSingleton<IAzureRMApiClient, AzureRMApiClient>();
            services.AddSingleton<ISvcBusMessageInfoReplacer, SvcBusMessageInfoReplacer>();
            services.AddSingleton<ISqlStoredProcInfoReplacer, SqlStoredProcInfoReplacer>();

            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddMediatrEndpoints(typeof(Startup));

            services.AddSingleton<IAggregatorBackgroundService, AggregatorBackgroundService>();
            services.AddHostedService(sp => (AggregatorBackgroundService)sp.GetService<IAggregatorBackgroundService>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
                
                endpoints.MapGroupOfEndpointsForAPath("/api/v1/livedocs", "LiveDocs")
                    .WithGet<GetLiveDocsRequest, GetLiveDocsResponse>("/{resourceName}");
            });
        }
    }
}

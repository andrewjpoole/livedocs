using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using AJP.MediatrEndpoints;
using AJP.MediatrEndpoints.EndpointRegistration;
using LiveDocs.Server.config;
using LiveDocs.Server.Hubs;
using LiveDocs.Server.Replacers;
using LiveDocs.Server.RequestHandlers;
using LiveDocs.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Authentication.Cookies;

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
                      
            // services.AddAuthentication(options =>
            // {
            //     options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            //     options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            // })
            //.AddCookie()
            //.AddOpenIdConnect(options =>
            //{
            //    options.ClientId = Configuration["AzureAd:ClientId"];
            //    options.Authority = Configuration["AzureAd:Instance"] + Configuration["AzureAd:TenantId"];
            //})
            //.AddJwtBearer(options =>
            //{
            //    options.Authority = Configuration["AzureAd:Instance"] + Configuration["AzureAd:TenantId"];
            //    options.Audience = Configuration["AzureAd:Audience"];
            //    options.Events = new JwtBearerEvents();
            //    options.Events.OnMessageReceived = context =>
            //    {
            //        if (context.Request.Path.Value.StartsWith("/latestmarkdownhub") && context.Request.Query.TryGetValue("token", out var token))
            //        {
            //            context.Token = token;
            //        }

            //        return Task.CompletedTask;
            //    };
            //});

            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));

            //services.AddSingleton<IUserIdProvider, EmailBasedUserIdProvider>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(Configuration.GetSection("AzureAd"));

            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters.NameClaimType = "name";

                // (from https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz?view=aspnetcore-5.0)
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        // If the request is for our hub...
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) &&
                        (path.StartsWithSegments("/latestmarkdownhub")))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var ex = context.Exception;
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var x = context.Principal?.Identity?.Name;
                        return Task.CompletedTask;
                    },OnForbidden = context => 
                    {                        
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddSignalR();
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddResponseCompression(opts =>
            {
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" });
            });
            services.AddMediatrEndpoints(typeof(Startup));

            services.AddSingleton<IAzureIAMTokenFetcher, AzureIAMTokenFetcher>();
            services.AddSingleton<IAzureRMApiClient, AzureRMApiClient>();

            services.AddSingleton<IBackgroundTaskQueue>(ctx => new BackgroundTaskQueue(50));
            services.AddSingleton<InMemoryReplacementCache>();
            services.AddSingleton<IReplacementCache>(sp => sp.GetRequiredService<InMemoryReplacementCache>());
            services.AddSingleton<IInMemoryReplacementCacheBackgroundTaskQueueStats>(sp => sp.GetRequiredService<InMemoryReplacementCache>());
            services.AddHostedService(sp => sp.GetService<IReplacementCache>() as InMemoryReplacementCache);

            services.AddSingleton<ISvcBusMessageInfoReplacer, SvcBusMessageInfoReplacer>();
            services.AddSingleton<ISqlStoredProcInfoReplacer, SqlStoredProcInfoReplacer>();
            services.AddSingleton<IStd18InfoReplacer, Std18InfoReplacer>();
            services.AddSingleton<IFileContentDownloader, FileContentDownloader>();

            services.AddSingleton<HubGroupTracker>();
            services.AddSingleton<IHubGroupTracker>(sp => sp.GetRequiredService<HubGroupTracker>());
            services.AddSingleton<IConnectedClientStats>(sp => sp.GetRequiredService<HubGroupTracker>());
            
            services.AddSingleton<MarkdownReplacementAggregatorBackgroundService>();
            services.AddSingleton<IMarkdownReplacementAggregatorBackgroundService>(sp => sp.GetRequiredService<MarkdownReplacementAggregatorBackgroundService>());
            services.AddSingleton<IResourceDocumentationStats>(sp => sp.GetRequiredService<MarkdownReplacementAggregatorBackgroundService>());
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

            app.CopyTokenFromQueryToHeaderForSignalrHub("/latestmarkdownhub");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");

                endpoints.MapGet("/hello", context => context.Response.WriteAsync("helloworld")).RequireAuthorization();

                endpoints.MapHub<LatestMarkdownHub>("/latestmarkdownhub");

                endpoints.MapGroupOfEndpointsForAPath("/api/v1/resources", "LiveDocsResources")
                    .WithGet<GetResourceDocumentationsRequest, GetResourceDocumentationsResponse>("/", configureEndpoint: endpoint => endpoint.RequireAuthorization())
                    .WithPost<ReloadResourceDocumentationFilesRequest, ReloadResourceDocumentationFilesResponse>(
                        "/reload", "Reload the Resource Documentation files i.e. to reflect recent changes.", configureEndpoint: endpoint => endpoint.RequireAuthorization());

                endpoints.MapGroupOfEndpointsForAPath("/api/v1/stats", "Stats")
                    .WithGet<GetStatsRequest, GetStatsResponse>("/");
            });
        }
    }

    public class EmailBasedUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirst(ClaimTypes.Email)?.Value;
        }
    }

    public static class AuthExtensions 
    {
        public static void CopyTokenFromQueryToHeaderForSignalrHub(this IApplicationBuilder app, string hubPrefix)
        {
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value ?? string.Empty;
                if (path.StartsWith(hubPrefix))
                {
                    if (context.Request.Query.TryGetValue("access_token", out var token))
                    {
                        context.Request.Headers.Add("Authorization", new string[] { "bearer " + token });
                    }
                }
                await next();
            });

        }
    }
}

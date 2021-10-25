using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Modulight.Modules.Hosting;
using StardustDL.RazorComponents.Markdown;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace LiveDocs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("LiveDocs.ServerAPI", client =>
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
                .CreateClient("LiveDocs.ServerAPI"));


            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("http://livedocs/user_impersonation");
            });
            
            builder.Services.AddModules(builder =>
            {
                builder.UseRazorComponentClientModules().AddMarkdownModule();
            });

            await builder.Build().RunAsyncWithModules();
        }
    }
}

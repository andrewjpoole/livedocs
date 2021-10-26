using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using LiveDocs.Extensions;

namespace LiveDocs.Pages
{
    public class FragmentNavigationBase : ComponentBase, IAsyncDisposable
    {
        [Inject] public NavigationManager NavManager { get; set; }
        [Inject] public IJSRuntime JsRuntime { get; set; }
        public IJSObjectReference ScrollInterface;

        protected override void OnInitialized()
        {
            NavManager.LocationChanged += TryFragmentNavigation;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ScrollInterface = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./scroll-interface.js");
            }
            await NavManager.NavigateToFragmentAsync(ScrollInterface);
        }

        private async void TryFragmentNavigation(object sender, LocationChangedEventArgs args)
        {
            await NavManager.NavigateToFragmentAsync(ScrollInterface);
        }
        
        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            NavManager.LocationChanged -= TryFragmentNavigation;
            if (ScrollInterface is not null)
            {
                await ScrollInterface.DisposeAsync();
            }
        }
    }
}

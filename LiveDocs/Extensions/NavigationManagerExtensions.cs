using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace LiveDocs.Extensions
{
    public static class Extensions
    {
        public static ValueTask NavigateToFragmentAsync(this NavigationManager navigationManager, IJSObjectReference scrollInterface)
        {
            var uri = navigationManager.ToAbsoluteUri(navigationManager.Uri);

            if (uri.Fragment.Length == 0)
                return default;

            return scrollInterface.InvokeVoidAsync("scrollToElement", uri.Fragment.Substring(1));
        }
    }
}

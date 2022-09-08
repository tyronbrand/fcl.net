using Microsoft.JSInterop;

namespace Fcl.Net.Blazor.Models
{
    public class JsInstance
    {
        private readonly IJSObjectReference jSInstance;

        public JsInstance(IJSObjectReference jSInstance)
        {
            this.jSInstance = jSInstance;
        }
        public async Task Close() => await jSInstance.InvokeVoidAsync("close");

    }
}

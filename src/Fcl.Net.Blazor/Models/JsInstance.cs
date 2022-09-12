using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Fcl.Net.Blazor.Models
{
    public class JsInstance
    {
        private readonly IJSObjectReference _jsInstance;

        public JsInstance(IJSObjectReference jsInstance)
        {
            this._jsInstance = jsInstance;
        }
        public async Task Close() => await _jsInstance.InvokeVoidAsync("close");

    }
}

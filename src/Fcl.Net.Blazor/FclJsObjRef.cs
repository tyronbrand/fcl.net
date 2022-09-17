using Fcl.Net.Blazor.Models;
using Fcl.Net.Core;
using Fcl.Net.Core.Models;
using Microsoft.JSInterop;

namespace Fcl.Net.Blazor
{
    public class FclJsObjRef : IAsyncDisposable
    {
        private readonly Lazy<Task<IJSObjectReference>> _moduleTask;
        private JsInstance? _jsInstance;

        public FclJsObjRef(IJSRuntime jsRuntime)
        {
            _moduleTask = new Lazy<Task<IJSObjectReference>>(() => jsRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./_content/Fcl.Net.Blazor/fclDotNet.js").AsTask());
        }

        public async ValueTask<string> OpenLocalViewAwaitResponse(FclService service, FclServiceConfig config, object? data = null)
        {
            var jsMethod = service.Method.ToString().ToLower().Contains("pop") ? "execPop" : "execIFrame";
            var module = await _moduleTask.Value;

            return await module.InvokeAsync<string>(jsMethod, service.ToDictionary<string, object>(), data, config).ConfigureAwait(false);
        }

        public async ValueTask OpenLocalView(Uri uri, FclServiceMethod fclServiceMethod)
        {
            var jsMethod = fclServiceMethod == FclServiceMethod.ViewPop ? "renderPop" : "renderFrame";
            var module = await _moduleTask.Value.ConfigureAwait(false);

            var objRef = await module.InvokeAsync<IJSObjectReference>(jsMethod, uri, true).ConfigureAwait(false);
            _jsInstance = new JsInstance(objRef);
        }

        public async ValueTask CloseLocalView()
        {
            if (_jsInstance != null)
                await _jsInstance.Close().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value;
                await module.DisposeAsync();
            }
        }
    }
}

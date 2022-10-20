using Fcl.Net.Blazor.Models;
using Fcl.Net.Core;
using Fcl.Net.Core.Models;
using Microsoft.JSInterop;
using Newtonsoft.Json;

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
            var jsMethod = GetExecJsMethod(service.Method);
            var module = await _moduleTask.Value.ConfigureAwait(false);

            return await module.InvokeAsync<string>(jsMethod, service.ToDictionary<string, object>(), JsonConvert.SerializeObject(data), JsonConvert.SerializeObject(config)).ConfigureAwait(false);
        }

        private string GetExecJsMethod(FclServiceMethod fclServiceMethod)
        {
            switch(fclServiceMethod)
            {
                case FclServiceMethod.IFrameRPC:
                    return "execIFrame";
                case FclServiceMethod.ExtRpc:
                    return "execExt";
                case FclServiceMethod.PopRpc:
                    return "execPop";
                default:
                    return "execIFrame";
            }
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

        public async ValueTask<string> InvokeMethod(string identifier)
        {
            var module = await _moduleTask.Value.ConfigureAwait(false);
            return await module.InvokeAsync<string>(identifier).ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (_moduleTask.IsValueCreated)
            {
                var module = await _moduleTask.Value.ConfigureAwait(false);
                await module.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}

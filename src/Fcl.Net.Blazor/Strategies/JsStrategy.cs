using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using Newtonsoft.Json;

namespace Fcl.Net.Blazor.Strategies
{
    public class JsStrategy : IStrategy
    {
        private readonly FclJsObjRef _fclJsObjRef;

        public JsStrategy(FclJsObjRef fclJsObjRef)
        {
            _fclJsObjRef = fclJsObjRef;
        }

        public async Task<T?> ExecuteAsync<T>(FclService service, FclServiceConfig? config = null, object? data = null, HttpMethod? httpMethod = null)
            where T : class
        {
            try
            {
                var response = await _fclJsObjRef.OpenLocalViewAwaitResponse(service, config, data).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (Exception)
            {
                await _fclJsObjRef.CloseLocalView();
                return default;
            }
        }
    }
}

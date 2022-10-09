using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service.Strategies;
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

        public async Task<FclAuthResponse?> ExecuteAsync(FclService service, FclServiceConfig? config = null, object? data = null, HttpMethod? httpMethod = null)
        {
            try
            {
                var response = await _fclJsObjRef.OpenLocalViewAwaitResponse(service, config, data).ConfigureAwait(false);
                return JsonConvert.DeserializeObject<FclAuthResponse>(response);
            }
            catch (Exception)
            {
                await _fclJsObjRef.CloseLocalView();
                return null;
            }
        }
    }
}

using Fcl.Net.Core.Models;
using Fcl.Net.Core.Platform;
using Flow.Net.Sdk.Client.Http;
using Newtonsoft.Json;

namespace Fcl.Net.Blazor.Platform
{
    public class BlazorPlatform : IPlatform
    {
        private readonly FclJsObjRef _fclJsObjRef;

        public BlazorPlatform(FclJsObjRef fclJsObjRef)
        {
            _fclJsObjRef = fclJsObjRef;
        }

        public async Task<ICollection<FclService>?> GetClientServices()
        {
            var result = await _fclJsObjRef.InvokeMethod("getFclExtensions").ConfigureAwait(false);

            if(result != null)
                return JsonConvert.DeserializeObject<ICollection<FclService>>(result);

            return null;
        }

        public string Location()
        {
            return "";
        }
    }
}

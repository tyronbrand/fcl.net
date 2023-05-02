using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Service.Strategies
{
    public abstract class BaseStrategy : IStrategy
    {
        protected readonly FetchService FetchService;

        public BaseStrategy(FetchService fetchService)
        {
            FetchService = fetchService;
        }

        public virtual async Task<T> ExecuteAsync<T>(FclService service, FclServiceConfig config = null, object data = null, HttpMethod httpMethod = null)
            where T : class
        {
            if (httpMethod == null)
                httpMethod = HttpMethod.Post;

            var requestData = new FclStrategyRequest
            {
                Config = config,
                Service = new FclStrategyRequestService
                {
                    Params = service.Params.Any() ? service.Params : null,
                    Data = service.Data.Any() ? service.Data : null,
                    Type = service.Type
                }
            }.ToDictionary<string, object>();

            if (data != null)
            {
                foreach (var item in data.ToDictionary<string, object>())
                    requestData.Add(item.Key, item.Value);
            }

            return await FetchService.FetchAndReadResponseAsync<T>(service, requestData, httpMethod).ConfigureAwait(false);
        }
    }
}

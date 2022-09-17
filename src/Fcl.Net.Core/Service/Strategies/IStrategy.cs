using Fcl.Net.Core.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Service.Strategies
{
    public interface IStrategy
    {
        Task<FclAuthResponse> ExecuteAsync(FclService service, FclServiceConfig config = null, object data = null, HttpMethod httpMethod = null);
    }
}

using Fcl.Net.Core.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Interfaces
{
    public interface IStrategy
    {
        Task<T> ExecuteAsync<T>(FclService service, FclServiceConfig config = null, object data = null, HttpMethod httpMethod = null);
    }
}

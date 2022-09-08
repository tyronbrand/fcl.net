using Fcl.Net.Core.Models;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Resolve
{
    public interface IResolver
    {
        Task Resolve(FclInteraction fclInteraction);
    }
}

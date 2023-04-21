using Fcl.Net.Core.Models;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Interfaces
{
    public interface IResolver
    {
        Task ResolveAsync(FclInteraction fclInteraction);
    }
}

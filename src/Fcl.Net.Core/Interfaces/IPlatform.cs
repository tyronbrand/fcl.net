using Fcl.Net.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Interfaces
{
    public interface IPlatform
    {
        Task<ICollection<FclService>> GetClientServices();
    }
}

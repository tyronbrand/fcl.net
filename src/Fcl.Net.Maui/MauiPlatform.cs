using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;

namespace Fcl.Net.Maui
{
    public class MauiPlatform : IPlatform
    {
        public async Task<ICollection<FclService>> GetClientServices() => null;
    }
}

using Fcl.Net.Core.Models;
using Fcl.Net.Core.Platform;

namespace Fcl.Net.Maui
{
    public class MauiPlatform : IPlatform
    {
        public async Task<ICollection<FclService>> GetClientServices() => null;
        public string Location() => "";
    }
}

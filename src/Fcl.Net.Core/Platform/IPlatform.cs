using Fcl.Net.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Platform
{
    public interface IPlatform
    {
        string Location();
        Task<ICollection<FclService>> GetClientServices();

        //TODO - storage/sessions
        //Task StorageSet<T>( Dictionary<string, T> items);
        //Task<T> StorageGet<T>(string key);
        //Task StorageRemove<T>(string key);
    }
}

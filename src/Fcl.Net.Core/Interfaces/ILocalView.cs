using Fcl.Net.Core.Models;
using System;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Interfaces
{
    public interface ILocalView
    {
        bool IsLocalDefault();
        Task OpenLocalView(Uri uri);
        Task CloseLocalView();
    }
}

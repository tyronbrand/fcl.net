using Fcl.Net.Core.Models;
using System;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Service.Strategy
{
    public interface ILocalView
    {
        Task OpenLocalView(Uri uri);
        Task CloseLocalView();
    }
}

using Fcl.Net.Core.Models;
using System;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Service.Strategies.LocalViews
{
    public interface ILocalView
    {
        bool IsDefault();
        Task OpenLocalView(Uri uri);
        Task CloseLocalView();
    }
}

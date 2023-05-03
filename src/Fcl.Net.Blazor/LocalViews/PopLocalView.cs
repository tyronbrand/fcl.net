using System;
using System.Threading.Tasks;
using Fcl.Net.Core;
using Fcl.Net.Core.Interfaces;

namespace Fcl.Net.Blazor.LocalViews
{
    public class PopLocalView : ILocalView
    {
        private readonly FclJsObjRef _fclJsObjRef;

        public PopLocalView(FclJsObjRef fclJsObjRef)
        {
            _fclJsObjRef = fclJsObjRef;
        }

        public async Task CloseLocalView() => await _fclJsObjRef.CloseLocalView().ConfigureAwait(false);

        public async Task OpenLocalView(Uri uri) => await _fclJsObjRef.OpenLocalView(uri, FclServiceMethod.ViewPop).ConfigureAwait(false);

        public bool IsLocalDefault() => false;
    }
}

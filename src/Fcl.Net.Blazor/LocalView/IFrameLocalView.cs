using System;
using System.Threading.Tasks;
using Fcl.Net.Core;
using Fcl.Net.Core.Service.Strategy;

namespace Fcl.Net.Blazor.LocalView
{
    public class IFrameLocalView : ILocalView
    {
        private readonly FclJsObjRef _fclJsObjRef; 
        
        public IFrameLocalView(FclJsObjRef fclJsObjRef)
        {
            _fclJsObjRef = fclJsObjRef;
        }

        public async Task CloseLocalView() => await _fclJsObjRef.CloseLocalView().ConfigureAwait(false);

        public async Task OpenLocalView(Uri uri) => await _fclJsObjRef.OpenLocalView(uri, FclServiceMethod.ViewIFrame).ConfigureAwait(false);
    }
}

﻿using Fcl.Net.Core;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service.Strategy;

namespace Fcl.Net.Blazor.LocalView
{
    public class IFrameLocalView : ILocalView
    {
        private readonly FclJsObjRef _fclJSObjRef; 
        
        public IFrameLocalView(FclJsObjRef fclJSObjRef)
        {
            _fclJSObjRef = fclJSObjRef;
        }

        public async Task CloseLocalView() => await _fclJSObjRef.CloseLocalView().ConfigureAwait(false);

        public async Task OpenLocalView(Uri uri) => await _fclJSObjRef.OpenLocalView(uri, FclServiceMethod.ViewIFrame).ConfigureAwait(false);
    }
}
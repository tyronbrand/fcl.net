using System.Threading.Tasks;
using System;

namespace Fcl.Net.Maui.FlowAuthenticator
{
    partial class FlowWebAuthenticatorImplementation : IFlowWebAuthenticator
    {
        public Task AuthenticateAsync(Uri url, Uri callbackUrl)
        {
            throw new NotImplementedException();
        }

        public Task CloseBrowserAsync()
        {
            throw new NotImplementedException();
        }
    }
}
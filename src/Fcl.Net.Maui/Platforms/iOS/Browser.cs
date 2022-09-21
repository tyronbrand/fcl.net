using Fcl.Net.Core.Models;
using Fcl.Net.Maui.WebBrowser;

namespace Fcl.Net.Maui
{
    public class Browser : IWebBrowser, IPlatformWebAuthenticatorCallback
    {
        public bool OpenUrlCallback(Uri uri)
        {
            throw new NotImplementedException();
        }

        public Task<FclAuthResponse> OpenWebBrowser(Uri url, Uri callbackUrl)
        {
            throw new NotImplementedException();
        }
    }
}

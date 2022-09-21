using Fcl.Net.Core.Service.Strategies.LocalViews;
using Fcl.Net.Maui.WebBrowser;

namespace Fcl.Net.Maui.LocalViews
{
    public class BrowserView : ILocalView
    {
        private readonly IWebBrowser _webBrowser;
        public BrowserView(IWebBrowser webBrowser)
        {
            _webBrowser = webBrowser;
        }

        public Task CloseLocalView()
        {
            throw new NotImplementedException();
        }

        public bool IsDefault() => true;

        public Task OpenLocalView(Uri uri) => _webBrowser.OpenWebBrowser(uri, new Uri("xamarinessentials://"));
    }
}

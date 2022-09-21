using Fcl.Net.Core.Models;

namespace Fcl.Net.Maui.WebBrowser
{
    public interface IWebBrowser
    {
        Task<FclAuthResponse> OpenWebBrowser(Uri url, Uri callbackUrl);
    }

    public interface IPlatformBrowserCallback
    {
#if IOS || MACCATALYST || MACOS
		bool OpenUrlCallback(Uri uri);
#elif ANDROID
		bool OnResumeCallback(Android.Content.Intent intent);
#endif
    }
}

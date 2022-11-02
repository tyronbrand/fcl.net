using AuthenticationServices;
using Foundation;
using SafariServices;
using System.Diagnostics;
using UIKit;
using WebKit;

namespace Fcl.Net.Maui.FlowAuthenticator
{
    partial class FlowWebAuthenticatorImplementation : IFlowAuthenticator
    {

#if __IOS__
        const int asWebAuthenticationSessionErrorCodeCanceledLogin = 1;
        const string asWebAuthenticationSessionErrorDomain = "com.apple.AuthenticationServices.WebAuthenticationSession";

        const int sfAuthenticationErrorCanceledLogin = 1;
        const string sfAuthenticationErrorDomain = "com.apple.SafariServices.Authentication";
#endif

        TaskCompletionSource<bool> tcsResponse;
        UIViewController currentViewController;
        Uri redirectUri;

#if __IOS__
        ASWebAuthenticationSession was;
        SFAuthenticationSession sf;
#endif

        public async Task AuthenticateAsync(Uri url, Uri callbackUrl)
        {
            await OpenBrowserAsync(url, callbackUrl);

            was = null;
            sf = null;

            await WindowStateManager.Default.GetCurrentUIWindow().RootViewController.DismissViewControllerAsync(true);
        }

        public async Task<bool> OpenBrowserAsync(Uri url, Uri callbackUrl)
        {
            var prefersEphemeralWebBrowserSession = false;

            if (!VerifyHasUrlSchemeOrDoesntRequire(callbackUrl.Scheme))
                throw new InvalidOperationException("You must register your URL Scheme handler in your app's Info.plist.");

            // Cancel any previous task that's still pending
            if (tcsResponse?.Task != null && !tcsResponse.Task.IsCompleted)
                tcsResponse.TrySetCanceled();

            tcsResponse = new TaskCompletionSource<bool>();
            redirectUri = callbackUrl;
            var scheme = redirectUri.Scheme;

#if __IOS__
            void AuthSessionCallback(NSUrl cbUrl, NSError error)
            {
                if (error == null)
                {
                    CallbackSuccess();
                }
                else if (error.Domain == asWebAuthenticationSessionErrorDomain && error.Code == asWebAuthenticationSessionErrorCodeCanceledLogin)
                    tcsResponse.TrySetCanceled();
                else if (error.Domain == sfAuthenticationErrorDomain && error.Code == sfAuthenticationErrorCanceledLogin)
                    tcsResponse.TrySetCanceled();
                else
                    tcsResponse.TrySetException(new NSErrorException(error));
            }

            if (OperatingSystem.IsIOSVersionAtLeast(12))
            {
                was = new ASWebAuthenticationSession(GetNativeUrl(url), scheme, AuthSessionCallback);

                if (OperatingSystem.IsIOSVersionAtLeast(13))
                {
                    var ctx = new ContextProvider(WindowStateManager.Default.GetCurrentUIWindow());
                    was.PresentationContextProvider = ctx;
                    was.PrefersEphemeralWebBrowserSession = prefersEphemeralWebBrowserSession;
                }
                else if (prefersEphemeralWebBrowserSession)
                {
                    ClearCookies();
                }

                using (was)
                {
                    was.Start();
                    return await tcsResponse.Task;
                }
            }

            if (prefersEphemeralWebBrowserSession)
                ClearCookies();

            if (OperatingSystem.IsIOSVersionAtLeast(11))
            {
                sf = new SFAuthenticationSession(GetNativeUrl(url), scheme, AuthSessionCallback);
                using (sf)
                {
                    sf.Start();
                    return await tcsResponse.Task;
                }
            }

            // iOS9+
            var controller = new SFSafariViewController(GetNativeUrl(url), false)
            {
                Delegate = new NativeSFSafariViewControllerDelegate
                {
                    DidFinishHandler = (svc) =>
                    {
                        // Cancel our task if it wasn't already marked as completed
                        if (!(tcsResponse?.Task?.IsCompleted ?? true))
                            tcsResponse.TrySetCanceled();
                    }
                },
            };

            currentViewController = controller;
            await WindowStateManager.Default.GetCurrentUIViewController().PresentViewControllerAsync(controller, true);
#else
			var opened = UIApplication.SharedApplication.OpenUrl(url);
			if (!opened)
				tcsResponse.TrySetException(new Exception("Error opening Safari"));
#endif

            return await tcsResponse.Task;
        }

        void ClearCookies()
        {
            NSUrlCache.SharedCache.RemoveAllCachedResponses();

#if __IOS__
            if (OperatingSystem.IsIOSVersionAtLeast(11))
            {
                WKWebsiteDataStore.DefaultDataStore.HttpCookieStore.GetAllCookies((cookies) =>
                {
                    foreach (var cookie in cookies)
                    {
                        WKWebsiteDataStore.DefaultDataStore.HttpCookieStore.DeleteCookie(cookie, null);
                    }
                });
            }
#endif
        }

        private void CallbackSuccess()
        {
            currentViewController?.DismissViewControllerAsync(true);
            currentViewController = null;

            tcsResponse.TrySetResult(true);
        }

        static bool VerifyHasUrlSchemeOrDoesntRequire(string scheme)
        {
            // iOS11+ uses sfAuthenticationSession which handles its own url routing
            if (OperatingSystem.IsIOSVersionAtLeast(11, 0) || OperatingSystem.IsTvOSVersionAtLeast(11, 0))
                return true;

            return VerifyHasUrlScheme(scheme);
        }

#if __IOS__
        class NativeSFSafariViewControllerDelegate : SFSafariViewControllerDelegate
        {
            public Action<SFSafariViewController> DidFinishHandler { get; set; }

            public override void DidFinish(SFSafariViewController controller) =>
                DidFinishHandler?.Invoke(controller);
        }

        class ContextProvider : NSObject, IASWebAuthenticationPresentationContextProviding
        {
            public ContextProvider(UIWindow window) =>
                Window = window;

            public readonly UIWindow Window;

            [Export("presentationAnchorForWebAuthenticationSession:")]
            public UIWindow GetPresentationAnchor(ASWebAuthenticationSession session)
                => Window;
        }
#endif

        static bool VerifyHasUrlScheme(string scheme)
        {
            var cleansed = scheme.Replace("://", string.Empty, StringComparison.Ordinal);
            var schemes = GetCFBundleURLSchemes().ToList();
            return schemes.Any(x => x != null && x.Equals(cleansed, StringComparison.OrdinalIgnoreCase));
        }

        static IEnumerable<string> GetCFBundleURLSchemes()
        {
            var schemes = new List<string>();

            NSObject nsobj = null;
            if (!NSBundle.MainBundle.InfoDictionary.TryGetValue((NSString)"CFBundleURLTypes", out nsobj))
                return schemes;

            var array = nsobj as NSArray;

            if (array == null)
                return schemes;

            for (nuint i = 0; i < array.Count; i++)
            {
                var d = array.GetItem<NSDictionary>(i);
                if (d == null || !d.Any())
                    continue;

                if (!d.TryGetValue((NSString)"CFBundleURLSchemes", out nsobj))
                    continue;

                var a = nsobj as NSArray;
                var urls = ConvertToIEnumerable<NSString>(a).Select(x => x.ToString()).ToArray();
                foreach (var url in urls)
                    schemes.Add(url);
            }

            return schemes;
        }

        static IEnumerable<T> ConvertToIEnumerable<T>(NSArray array)
            where T : class, ObjCRuntime.INativeObject
        {
            for (nuint i = 0; i < array.Count; i++)
                yield return array.GetItem<T>(i);
        }

        static Foundation.NSUrl GetNativeUrl(Uri uri)
        {
            try
            {
                return new Foundation.NSUrl(uri.OriginalString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to create NSUrl from Original string, trying Absolute URI: {ex.Message}");
                return new Foundation.NSUrl(uri.AbsoluteUri);
            }
        }

        public Task CloseBrowserAsync()
        {
            CallbackSuccess();
            return Task.CompletedTask;
        }
    }
}
using Fcl.Net.Core;
using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategies;
using Fcl.Net.Maui.FlowAuthenticator;
#if ANDROID
using Android.Content;
using Application = Android.App.Application;
#endif


namespace Fcl.Net.Maui.Strategies
{
    public class MauiHttpPostStrategy : HttpPostStrategy
    {
        private readonly Uri _redirectUri;

        public MauiHttpPostStrategy(Uri redirectUri, FetchService fetchService, Dictionary<FclServiceMethod, ILocalView> localViews) : base(fetchService, localViews)
        {
            _redirectUri = redirectUri;
        }

        public override async Task<T> PollAsync<T>(T response)
        {
            if(response is FclAuthResponse fclAuthResponse)
            {

                if (fclAuthResponse.Local == null)
                    throw new FclException("Local was null.");

                var url = FetchService.BuildUrl(fclAuthResponse.Local);

#if IOS
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Task.WhenAll(FlowAuthenticator.WebAuthenticator.Default.AuthenticateAsync(url, _redirectUri), Poller(fclAuthResponse)).ConfigureAwait(false);
                });
#else
            await Task.WhenAny(Microsoft.Maui.Authentication.WebAuthenticator.Default.AuthenticateAsync(url, _redirectUri), Poller(fclAuthResponse)).ConfigureAwait(false);
#endif

                return await FetchService.FetchAndReadResponseAsync<T>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);
            }

            return response;
        }

        private async Task<bool> Poller(FclAuthResponse fclAuthResponse)
        {
            var delayMs = 1000;
            var timeoutMs = 300000;
            var startTime = DateTime.UtcNow;

            while (true)
            {
                var pollingResponse = await FetchService.FetchAndReadResponseAsync<FclAuthResponse>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);

                if (pollingResponse.Status == ResponseStatus.Approved || pollingResponse.Status == ResponseStatus.Declined)
                {
                    await RedirectToApp();
                    return true;
                }

                if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > timeoutMs)
                    throw new FclException("Timed out polling.");

                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }

        private async Task RedirectToApp()
        {
#if ANDROID

            var packageName = Application.Context.PackageName;
            var intent = new Intent(Intent.ActionView);
            intent.AddCategory(Intent.CategoryBrowsable);
            intent.AddCategory(Intent.CategoryDefault);
            intent.SetPackage(packageName);
            intent.SetData(global::Android.Net.Uri.Parse(_redirectUri.OriginalString));                      

            Platform.CurrentActivity.StartActivity(intent);
#elif IOS
            await FlowAuthenticator.WebAuthenticator.Default.CloseBrowserAsync().ConfigureAwait(false);
#endif
        }
    }
}

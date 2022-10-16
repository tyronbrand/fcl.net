﻿using Fcl.Net.Core;
using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategies;
using Fcl.Net.Core.Service.Strategies.LocalViews;
#if ANDROID
using Android.Content;
using Application = Android.App.Application;
#endif


namespace Fcl.Net.Maui.Strategies
{
    public class MauiHttpPostStrategy : HttpPostStrategy
    {
        private readonly IWebAuthenticator _authenticator;
        private readonly Uri _redirectUri;

        public MauiHttpPostStrategy(IWebAuthenticator authenticator, Uri redirectUri, FetchService fetchService, Dictionary<FclServiceMethod, ILocalView> localViews) : base(fetchService, localViews)
        {
            _authenticator = authenticator;
            _redirectUri = redirectUri;
        }

        public override async Task<FclAuthResponse> PollAsync(FclAuthResponse fclAuthResponse)
        {
            if (fclAuthResponse.Local == null)
                throw new FclException("Local was null.");

            var url = _fetchService.BuildUrl(fclAuthResponse.Local);
            await Task.WhenAll(_authenticator.AuthenticateAsync(url, _redirectUri), Poller(fclAuthResponse)).ConfigureAwait(false);

            return await _fetchService.FetchAndReadResponseAsync<FclAuthResponse>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);
        }

        private async Task<bool> Poller(FclAuthResponse fclAuthResponse)
        {
            var delayMs = 1000;
            var timeoutMs = 300000;
            var startTime = DateTime.UtcNow;

            while (true)
            {
                var pollingResponse = await _fetchService.FetchAndReadResponseAsync<FclAuthResponse>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);

                if (pollingResponse.Status == ResponseStatus.Approved || pollingResponse.Status == ResponseStatus.Declined)
                {
                    RedirectToApp();
                    return true;
                }

                if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > timeoutMs)
                    throw new FclException("Timed out polling.");

                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }

        private void RedirectToApp()
        {
#if ANDROID

            var packageName = Application.Context.PackageName;
            var intent = new Intent(Intent.ActionView);
            intent.AddCategory(Intent.CategoryBrowsable);
            intent.AddCategory(Intent.CategoryDefault);
            intent.SetPackage(packageName);
            intent.SetData(global::Android.Net.Uri.Parse(_redirectUri.OriginalString));                      

            Platform.CurrentActivity.StartActivity(intent);
#endif
        }
    }
}

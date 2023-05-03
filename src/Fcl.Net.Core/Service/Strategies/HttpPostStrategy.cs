using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Service.Strategies
{
    public class HttpPostStrategy : BaseStrategy
    {
        protected readonly Dictionary<FclServiceMethod, ILocalView> _localViews;

        public HttpPostStrategy(FetchService fetchService, Dictionary<FclServiceMethod, ILocalView> localViews) : base(fetchService)
        {
            _localViews = localViews;
        }

        public override async Task<T> ExecuteAsync<T>(FclService service, FclServiceConfig config = null, object data = null, HttpMethod httpMethod = null)
        {
            var response = await base.ExecuteAsync<T>(service, config, data, httpMethod).ConfigureAwait(false);

            return await HandleResponse(response);
        }

        private async Task<T> HandleResponse<T>(T response)
        {
            if(response is FclAuthResponse authResponse)
            {
                switch (authResponse.Status)
                {
                    case ResponseStatus.Approved:
                        return response;
                    case ResponseStatus.Redirect:
                        return response;
                    case ResponseStatus.Declined:
                        throw new FclException(string.IsNullOrEmpty(authResponse.Reason) ? $"Declined: {authResponse.Reason}." : "No reason supplied.");
                    case ResponseStatus.Pending:
                        return await PollAsync(response).ConfigureAwait(false);
                    default:
                        throw new FclException("Auto Decline: Invalid Response.");
                }
            }

            return response;
        }

        public virtual async Task<T> PollAsync<T>(T response)
        {
            if (response is FclAuthResponse fclAuthResponse)
            {
                if (fclAuthResponse.Local == null)
                    throw new FclException("Local was null.");

                ILocalView localView = null;
                if (_localViews.ContainsKey(fclAuthResponse.Local.Method))
                {
                    localView = _localViews[fclAuthResponse.Local.Method];
                }
                else
                {
                    foreach (var view in _localViews)
                    {
                        if (view.Value.IsLocalDefault())
                        {
                            localView = view.Value;
                            break;
                        }
                    }
                }

                if (localView == null)
                    throw new FclException($"Failed to find strategy for {fclAuthResponse.Local.Method}");

                var url = FetchService.BuildUrl(fclAuthResponse.Local);
                await localView.OpenLocalView(url).ConfigureAwait(false);

                var delayMs = 1000;
                var timeoutMs = 300000;
                var startTime = DateTime.UtcNow;

                while (true)
                {
                    var pollingResponse = await FetchService.FetchAndReadResponseAsync<T>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);

                    if (pollingResponse is FclAuthResponse fclPollingResponse)
                    {
                        if (fclPollingResponse.Status == ResponseStatus.Approved || fclPollingResponse.Status == ResponseStatus.Declined)
                        {
                            await localView.CloseLocalView().ConfigureAwait(false);
                            return pollingResponse;
                        }

                        if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > timeoutMs)
                        {
                            await localView.CloseLocalView().ConfigureAwait(false);
                            throw new FclException("Timed out polling.");
                        }
                    }

                    await Task.Delay(delayMs).ConfigureAwait(false);
                }
            }

            return response;
        }
    }
}

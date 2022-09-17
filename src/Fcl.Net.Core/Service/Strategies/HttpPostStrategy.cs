﻿using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service.Strategies.LocalViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Service.Strategies
{
    public class HttpPostStrategy : IStrategy
    {
        private readonly FetchService _fetchService;
        private readonly Dictionary<FclServiceMethod, ILocalView> _localViews;

        public HttpPostStrategy(FetchService fetchService, Dictionary<FclServiceMethod, ILocalView> localViews)
        {
            _fetchService = fetchService;
            _localViews = localViews;
        }

        public async Task<FclAuthResponse> ExecuteAsync(FclService service, FclServiceConfig config = null, object data = null, HttpMethod httpMethod = null)
        {
            if (httpMethod == null)
                httpMethod = HttpMethod.Post;

            var requestData = new FclStrategyRequest
            {
                Config = config,
                Service = new FclStrategyRequestService
                {
                    Params = service.Params.Any() ? service.Params : null,
                    Data = service.Data.Any() ? service.Data : null,
                    Type = service.Type
                }
            }.ToDictionary<string, object>();

            if (data != null)
            {
                foreach (var item in data.ToDictionary<string, object>())
                    requestData.Add(item.Key, item.Value);
            }

            var response = await _fetchService.FetchAndReadResponseAsync<FclAuthResponse>(service, requestData, httpMethod).ConfigureAwait(false);

            switch (response.Status)
            {
                case ResponseStatus.Approved:
                    return response;
                case ResponseStatus.Redirect:
                    return response;
                case ResponseStatus.Declined:
                    throw new FclException(string.IsNullOrEmpty(response.Reason) ? $"Declined: {response.Reason}." : "No reason supplied.");
                case ResponseStatus.Pending:
                    return await PollAsync(response).ConfigureAwait(false);
                default:
                    throw new FclException("Auto Decline: Invalid Response.");
            }
        }

        private async Task<FclAuthResponse> PollAsync(FclAuthResponse fclAuthResponse)
        {
            if (fclAuthResponse.Local == null)
                throw new FclException("Local was null.");

            ILocalView localView = null;
            if (_localViews.ContainsKey(fclAuthResponse.Local.Method))
            {
                localView = _localViews[fclAuthResponse.Local.Method];                
            }
            else if (fclAuthResponse.Local.Method == FclServiceMethod.BrowserIframe)
            {
                foreach (var view in _localViews)
                {
                    if (view.Value.IsDefault())
                    {
                        localView = view.Value;
                        break;
                    }
                }
            }

            if(localView != null)
            {
                var url = _fetchService.BuildUrl(fclAuthResponse.Local);
                await localView.OpenLocalView(url).ConfigureAwait(false);
            }
            else
            {
                throw new FclException($"Failed to find strategy for {fclAuthResponse.Local.Method}");
            }

            var delayMs = 1000;
            var timeoutMs = 300000;
            var startTime = DateTime.UtcNow;

            while (true)
            {
                var pollingResponse = await _fetchService.FetchAndReadResponseAsync<FclAuthResponse>(fclAuthResponse.Updates ?? fclAuthResponse.AuthorizationUpdates, httpMethod: HttpMethod.Get).ConfigureAwait(false);

                if (pollingResponse.Status == ResponseStatus.Approved || pollingResponse.Status == ResponseStatus.Declined)
                {
                    await localView.CloseLocalView().ConfigureAwait(false);
                    return pollingResponse;
                }

                if (DateTime.UtcNow.Subtract(startTime).TotalMilliseconds > timeoutMs)
                {
                    await localView.CloseLocalView().ConfigureAwait(false);
                    throw new FclException("Timed out polling.");
                }

                await Task.Delay(delayMs).ConfigureAwait(false);
            }
        }
    }
}
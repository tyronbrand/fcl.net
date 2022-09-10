using Fcl.Net.Core.Config;
using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Resolve;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategy;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Exceptions;
using Flow.Net.Sdk.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fcl.Net.Core
{
    public class Fcl
    {
        public FclUser User;
        public IFlowClient Sdk;

        private readonly ExecService _execService;
        private readonly FclConfig _fclConfig;

        public Fcl(IFlowClient sdkClient, FclConfig fclConfig, Dictionary<FclServiceMethod, ILocalView> localViews, FetchService fetchService = null, Dictionary<FclServiceMethod, IStrategy> strategies = null)
        {
            Sdk = sdkClient;

            if (fetchService == null)
            {
                fetchService = new FetchService(
                    new HttpClient(),
                    new FetchServiceConfig
                    {
                        Location = "" //TODO
                    });
            }

            var strategyItems = new Dictionary<FclServiceMethod, IStrategy>
            {
                { FclServiceMethod.HttpPost, new HttpPostStrategy(fetchService, localViews) }
            };

            if (strategies != null && strategies.Any())
            {
                foreach (var strategy in strategies)
                    strategyItems.Add(strategy.Key, strategy.Value);
            }

            _execService = new ExecService(strategyItems);
            _fclConfig = fclConfig;
        }    

        public async Task AuthenticateAsync()
        {
            if (User != null && User.LoggedIn)
            {
                //TODO
                //authn-refresh
            }
            else
            {
                var response = await _execService.ExecuteAsync(GetDiscoveryService(), GetServiceConfig()).ConfigureAwait(false);

                if (response.Status == ResponseStatus.Approved)
                    SetCurrentUser(response);
            }
        }

        private FclServiceConfig GetServiceConfig()
        {
            return new FclServiceConfig
            {
                Services = _fclConfig.Services,
                App = _fclConfig.AppInfo,
                Client = new FclClientInfo
                {
                    Hostname = _fclConfig.Location
                }
            };
        }

        private FclService GetDiscoveryService()
        {
            return new FclService
            {
                FclType = "Service",
                FclVsn = "1.0.0",
                Type = FclServiceType.Authn,
                Endpoint = _fclConfig.WalletDiscovery.Wallet,
                Method = _fclConfig.WalletDiscovery.WalletMethod                
            };
        }

        private void SetCurrentUser(FclAuthResponse fclAuthResponse)
        {
            User = new FclUser
            {
                Address = fclAuthResponse.Data.Address,
                LoggedIn = true,
                Services = fclAuthResponse.Data.Services,
            };
        }

        public async Task<FclSignature> SignUserMessage(string message)
        {
            var userSignatureService = User.Services.FirstOrDefault(f => f.Type == FclServiceType.UserSignature);

            if (userSignatureService == null)
                throw new FlowException("Current user must have authorized a signing service.");

            var data = new FclSignableMessage
            {
                Message = message.StringToHex()
            };

            var response = await _execService.ExecuteAsync(userSignatureService, msg: data).ConfigureAwait(false);

            if (response.Data == null || string.IsNullOrEmpty(response.Data.Signature) || string.IsNullOrEmpty(response.Data.Address) || response.Data.KeyId == null)
                throw new FclException("Failed to sign message.");

            return new FclSignature
            {
                Address= response.Data.Address,
                KeyId = (uint)response.Data.KeyId,
                Signature = response.Data.Signature
            };
        }

        public async Task<string> MutateAsync(FclMutation fclMutation)
        {
            try
            {
                if (!User.LoggedIn)
                    throw new FclException("User unauthenticated.");

                var interactionBuilder = new FclInteractionBuilder(_execService, GetServiceConfig(), Sdk);
                var interaction = await interactionBuilder.Build(fclMutation, User).ConfigureAwait(false);
                var transaction = await Sdk.SendTransactionAsync(interaction.ToFlowTransaction()).ConfigureAwait(false);

                return transaction.Id;
            }
            catch (Exception ex)
            {
                throw new FclException("Mutation failed", ex);
            }
        }

        public async Task<ICadence> QueryAsync(FlowScript flowScript) => await Sdk.ExecuteScriptAtLatestBlockAsync(flowScript).ConfigureAwait(false);
    }
}

using Fcl.Net.Core.Config;
using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Resolve;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategy;
using Fcl.Net.Core.Utilities;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Exceptions;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fcl.Net.Core
{
    public class Fcl
    {        
        public FclUser User
        {
            get => _user;
            private set => _user = value;
        }
        
        public IFlowClient Sdk
        {
            get => _sdk;
            private set => _sdk = value;
        }

        private FclUser _user;
        private IFlowClient _sdk;
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
                var response = await _execService.ExecuteAsync(GetDiscoveryService(), GetServiceConfig(), _fclConfig.AccountProof ?? null).ConfigureAwait(false);

                if (response.Status == ResponseStatus.Approved)
                    SetCurrentUser(response);
            }
        }

        public void Unauthenticate()
        {
            _user = null;
        }

        public async Task<bool> VerifyAccountProof(bool includeDomainTag = false)
        {
            if (User == null || !User.LoggedIn)
                throw new FclException("User unauthenticated.");

            if (_fclConfig.AccountProof == null)
                throw new FclException("Config does not contain account proof.");

            var accountProofService = User.Services.FirstOrDefault(f => f.Type == FclServiceType.AccountProof);
            var address = accountProofService.Data["address"];
            var signatures = ((JArray)accountProofService.Data["signatures"]).ToObject<IEnumerable<FclCompositeSignature>>();

            var message = WalletUtilities.EncodeAccountProof(address.ToString(), _fclConfig.AccountProof.Nonce, _fclConfig.AccountProof.AppId, includeDomainTag);

            //TODO - use testnet "0x74daa6f9c7ef24b1" / mainnet "0xb4b82a1c9d21d284"
            var script = @"
import FCLCrypto from 0x74daa6f9c7ef24b1
pub fun main(
    address: Address,
    message: String,
    keyIndices: [Int],
    signatures: [String]
): Bool {
    return FCLCrypto.verifyAccountProofSignatures(address: address, message: message, keyIndices: keyIndices, signatures: signatures)
}";

            return await Verify(address.ToString(), message.BytesToHex(), script, signatures);
        }        

        public async Task<bool> VerifyUserSignature(string message, IEnumerable<FclCompositeSignature> fclCompositeSignatures)
        {
            //TODO - use testnet "0x74daa6f9c7ef24b1" / mainnet "0xb4b82a1c9d21d284"
            var script = @"
import FCLCrypto from 0x74daa6f9c7ef24b1
pub fun main(
    address: Address,
    message: String,
    keyIndices: [Int],
    signatures: [String]
): Bool {
    return FCLCrypto.verifyUserSignatures(address: address, message: message, keyIndices: keyIndices, signatures: signatures)
}";

            var address = fclCompositeSignatures.Select(s => s.Address).FirstOrDefault();

            return await Verify(address, message.StringToHex(), script, fclCompositeSignatures);
        }

        private async Task<bool> Verify(string address, string message, string script, IEnumerable<FclCompositeSignature> fclCompositeSignatures)
        {
            var signatures = new List<ICadence>();
            var signatureIndexes = new List<ICadence>();

            foreach (var signature in fclCompositeSignatures)
            {
                signatures.Add(new CadenceString(!string.IsNullOrEmpty(signature.Signature) ? signature.Signature : ""));
                signatureIndexes.Add(new CadenceNumber(CadenceNumberType.Int, signature.KeyId > 0 ? signature.KeyId.ToString() : "-1"));
            }

            try
            {
                var response = await Sdk.ExecuteScriptAtLatestBlockAsync(
                    new FlowScript
                    {
                        Script = script,
                        Arguments = new List<ICadence>()
                        {
                            new CadenceAddress(address.RemoveHexPrefix()),
                            new CadenceString(message),
                            new CadenceArray(signatureIndexes),
                            new CadenceArray(signatures)
                        }
                    }).ConfigureAwait(false);

                return response.As<CadenceBool>().Value;
            }
            catch (Exception ex)
            {
                throw new FclException(ex.Message, ex);
            }
        }

        public async Task<FclCompositeSignature> SignUserMessage(string message)
        {
            if (User == null || !User.LoggedIn)
                throw new FclException("User unauthenticated.");

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

            return new FclCompositeSignature
            {
                Address = response.Data.Address,
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
            _user = new FclUser
            {
                Address = fclAuthResponse.Data.Address,
                LoggedIn = true,
                Services = fclAuthResponse.Data.Services,
            };
        }
    }
}

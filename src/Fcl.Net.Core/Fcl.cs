using Fcl.Net.Core.Config;
using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Platform;
using Fcl.Net.Core.Resolve;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategies;
using Fcl.Net.Core.Utilities;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Fcl.Net.Core
{
    public class Fcl : INotifyPropertyChanged
    {        
        public FclUser User
        {
            get => _user;
            private set => SetProperty(ref _user, value);
        }
        
        public IFlowClient Sdk
        {
            get => _sdk;
            private set => _sdk = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private FclUser _user;
        private IFlowClient _sdk;
        private readonly ExecService _execService;
        private readonly FclConfig _fclConfig;
        private readonly IPlatform _platform;        

        public Fcl(FclConfig fclConfig, IFlowClient sdkClient, IPlatform platform, Dictionary<FclServiceMethod, IStrategy> strategies)
        {
            _platform = platform;
            _execService = new ExecService(strategies);
            _fclConfig = fclConfig;
            _sdk = sdkClient;
        }

        public async Task AuthenticateAsync()
        {
            var service = (User != null && User.LoggedIn) ? User.Services.FirstOrDefault(f => f.Type == FclServiceType.AuthnRefresh) : GetDiscoveryService();
            if(service !=null)
            {
                var response = await _execService.ExecuteAsync(service, await GetServiceConfigAsync(), _fclConfig.AccountProof).ConfigureAwait(false);

                if (response != null && response.Status == ResponseStatus.Approved)
                    SetCurrentUser(response);
            }            
        }

        public void Unauthenticate()
        {
            User = null;
        }

        public async Task<bool> VerifyAccountProofAsync(bool includeDomainTag = false)
        {
            if (User == null || !User.LoggedIn)
                throw new FclException("User unauthenticated.");

            if (_fclConfig.AccountProof == null)
                throw new FclException("Config does not contain account proof.");

            var accountProofService = User.Services.FirstOrDefault(f => f.Type == FclServiceType.AccountProof);

            if (accountProofService == null)
                throw new FclException("User does not container account proof service.");

            var address = accountProofService.Data["address"];
            var signatures = ((JArray)accountProofService.Data["signatures"]).ToObject<IEnumerable<FclCompositeSignature>>();

            var message = WalletUtilities.EncodeAccountProof(address.ToString(), _fclConfig.AccountProof.Nonce, _fclConfig.AccountProof.AppId, includeDomainTag);

            var script = $@"
import FCLCrypto from {(_fclConfig.Environment == ChainId.Mainnet ? "0xb4b82a1c9d21d284" : "0x74daa6f9c7ef24b1")}
pub fun main(
    address: Address,
    message: String,
    keyIndices: [Int],
    signatures: [String]
): Bool {{
    return FCLCrypto.verifyAccountProofSignatures(address: address, message: message, keyIndices: keyIndices, signatures: signatures)
}}";

            return await VerifyAsync(address.ToString(), message.BytesToHex(), script, signatures).ConfigureAwait(false);
        }        

        public async Task<bool> VerifyUserSignatureAsync(string message, IEnumerable<FclCompositeSignature> fclCompositeSignatures)
        {
            var script = $@"
import FCLCrypto from {(_fclConfig.Environment == ChainId.Mainnet ? "0xb4b82a1c9d21d284" : "0x74daa6f9c7ef24b1")}
pub fun main(
    address: Address,
    message: String,
    keyIndices: [Int],
    signatures: [String]
): Bool {{
    return FCLCrypto.verifyUserSignatures(address: address, message: message, keyIndices: keyIndices, signatures: signatures)
}}";

            var address = fclCompositeSignatures.Select(s => s.Address).FirstOrDefault();

            return await VerifyAsync(address, message.StringToHex(), script, fclCompositeSignatures).ConfigureAwait(false);
        }

        private async Task<bool> VerifyAsync(string address, string message, string script, IEnumerable<FclCompositeSignature> fclCompositeSignatures)
        {
            var signatures = new List<ICadence>();
            var signatureIndexes = new List<ICadence>();

            foreach (var signature in fclCompositeSignatures)
            {
                signatures.Add(new CadenceString(!string.IsNullOrEmpty(signature.Signature) ? signature.Signature : ""));
                signatureIndexes.Add(new CadenceNumber(CadenceNumberType.Int, signature.KeyId.ToString()));
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

        public async Task<FclCompositeSignature> SignUserMessageAsync(string message)
        {
            if (User == null || !User.LoggedIn)
                throw new FclException("User unauthenticated.");

            var userSignatureService = User.Services.FirstOrDefault(f => f.Type == FclServiceType.UserSignature);

            if (userSignatureService == null)
                throw new FclException("Current user must have authorized a signing service.");

            var data = new FclSignableMessage
            {
                Message = message.StringToHex()
            };

            var response = await _execService.ExecuteAsync(userSignatureService, await GetServiceConfigAsync(), data).ConfigureAwait(false);

            if (response == null || response.Data == null || string.IsNullOrEmpty(response.Data.Signature) || string.IsNullOrEmpty(response.Data.Address) || response.Data.KeyId == null)
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

                var interactionBuilder = new FclInteractionBuilder(_execService, await GetServiceConfigAsync(), Sdk);
                var interaction = await interactionBuilder.BuildAsync(fclMutation, User).ConfigureAwait(false);
                var transaction = await Sdk.SendTransactionAsync(interaction.ToFlowTransaction()).ConfigureAwait(false);

                return transaction.Id;
            }
            catch (Exception ex)
            {
                throw new FclException("Mutation failed", ex);
            }
        }

        public async Task<ICadence> QueryAsync(FlowScript flowScript) => await Sdk.ExecuteScriptAtLatestBlockAsync(flowScript).ConfigureAwait(false);

        public async Task<FlowAccount> GetAccountAsync(string address) => await _sdk.GetAccountAtLatestBlockAsync(address).ConfigureAwait(false);

        public async Task<FlowBlock> GetBlockAsync(string blockId) => await _sdk.GetBlockByIdAsync(blockId).ConfigureAwait(false);

        public async Task<FlowBlock> GetLastestBlock(bool isSealed = true) => await _sdk.GetLatestBlockAsync(isSealed).ConfigureAwait(false);

        public async Task<FlowBlockHeader> GetBlockHeader(string blockId) => await _sdk.GetBlockHeaderByIdAsync(blockId).ConfigureAwait(false);

        public async Task<FlowTransactionResult> GetTransactionStatus(string transactionId) => await _sdk.GetTransactionResultAsync(transactionId).ConfigureAwait(false);

        public async Task<FlowTransactionResponse> GetTransaction(string transactionId) => await _sdk.GetTransactionAsync(transactionId).ConfigureAwait(false);

        public void SetWalletProvider(FclWalletDiscovery fclWalletDiscovery)
        {
            _fclConfig.WalletDiscovery = fclWalletDiscovery;
        }

        private async Task<FclServiceConfig> GetServiceConfigAsync()
        {
            return new FclServiceConfig
            {
                Services = _fclConfig.Services,
                App = _fclConfig.AppInfo,
                Client = new FclClientInfo
                {
                    Hostname = _platform.Location(),
                    ClientServices = await _platform.GetClientServices()
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
    }
}

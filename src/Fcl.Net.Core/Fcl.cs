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
        /// <summary>
        /// Current user
        /// </summary>
        public FclUser User
        {
            get => _user;
            private set => SetProperty(ref _user, value);
        }
        
        /// <summary>
        /// SDK Client
        /// </summary>
        public IFlowClient Sdk
        {
            get => _sdk;
            private set => _sdk = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private FclUser _user;
        private IFlowClient _sdk;
        private readonly ExecService _execService;
        private readonly FclConfig _fclConfig;
        private readonly IPlatform _platform;
        private readonly Dictionary<FclServiceMethod, IStrategy> _strategies;

        /// <summary>
        /// Flow Client Library
        /// </summary>
        /// <param name="fclConfig"></param>
        /// <param name="sdkClient"></param>
        /// <param name="platform"></param>
        /// <param name="strategies"></param>
        public Fcl(FclConfig fclConfig, IFlowClient sdkClient, IPlatform platform, Dictionary<FclServiceMethod, IStrategy> strategies)
        {
            _platform = platform;
            _execService = new ExecService(strategies);
            _fclConfig = fclConfig;
            _sdk = sdkClient;
            _strategies = strategies;
        }

        /// <summary>
        /// Authenticate user.
        /// </summary>
        /// <exception cref="FclException"></exception>
        public async Task AuthenticateAsync()
        {
            try
            {
                var service = (User != null && User.LoggedIn) ? User.Services.FirstOrDefault(f => f.Type == FclServiceType.AuthnRefresh) : GetDiscoveryService();
                if (service != null)
                {
                    var response = await _execService.ExecuteAsync(service, await GetServiceConfigAsync(), _fclConfig.AccountProof).ConfigureAwait(false);

                    if (response != null && response.Status == ResponseStatus.Approved)
                        SetCurrentUser(response);
                }
            }
            catch(Exception ex)
            {
                throw new FclException("Authentication error", ex);
            }                      
        }

        /// <summary>
        /// Unauthenticate user.
        /// </summary>
        public void Unauthenticate()
        {
            User = null;
        }

        /// <summary>
        /// Verifies account proof.
        /// </summary>
        /// <param name="includeDomainTag"></param>
        /// <returns><see langword="true"/> if verified</returns>
        /// <exception cref="FclException"></exception>
        public async Task<bool> VerifyAccountProofAsync(bool includeDomainTag = false)
        {
            try
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
            catch (Exception ex)
            {
                throw new FclException("Verify account proof error", ex);
            }
        }

        /// <summary>
        /// Verifies user signature.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fclCompositeSignatures"></param>
        /// <returns><see langword="true"/> if verified</returns>
        /// <exception cref="FclException"></exception>
        public async Task<bool> VerifyUserSignatureAsync(string message, IEnumerable<FclCompositeSignature> fclCompositeSignatures)
        {
            try
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
            catch (Exception ex)
            {
                throw new FclException("Verify user signature error", ex);
            }
        }

        private async Task<bool> VerifyAsync(string address, string message, string script, IEnumerable<FclCompositeSignature> fclCompositeSignatures)
        {
            try
            {
                var signatures = new List<ICadence>();
                var signatureIndexes = new List<ICadence>();

                foreach (var signature in fclCompositeSignatures)
                {
                    signatures.Add(new CadenceString(!string.IsNullOrEmpty(signature.Signature) ? signature.Signature : ""));
                    signatureIndexes.Add(new CadenceNumber(CadenceNumberType.Int, signature.KeyId.ToString()));
                }
                
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

        /// <summary>
        /// Signs a message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns><see cref="FclCompositeSignature"/></returns>
        /// <exception cref="FclException"></exception>
        public async Task<FclCompositeSignature> SignUserMessageAsync(string message)
        {
            try
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
            catch (Exception ex)
            {
                throw new FclException("Sign user message error", ex);
            }
        }

        /// <summary>
        /// Submits a transaction to the network.
        /// </summary>
        /// <param name="fclMutation"></param>
        /// <returns>Transaction Id</returns>
        /// <exception cref="FclException"></exception>
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
                throw new FclException("Mutation error", ex);
            }
        }

        /// <summary>
        /// Executes a read-only Cadence script against the latest sealed execution state.
        /// </summary>
        /// <param name="flowScript"></param>
        /// <returns><see cref="ICadence"/></returns>
        /// <exception cref="FclException"></exception>
        public async Task<ICadence> QueryAsync(FlowScript flowScript)
        {
            try
            {
                return await Sdk.ExecuteScriptAtLatestBlockAsync(flowScript).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FclException("Query error", ex);
            }
        }

        /// <summary>
        /// Gets an account by address at the latest sealed block.
        /// </summary>
        /// <param name="address"></param>
        /// <returns><see cref="FlowAccount"/></returns>
        /// <exception cref="FclException"></exception>
        public async Task<FlowAccount> GetAccountAsync(string address)
        {
            try
            {
                return await _sdk.GetAccountAtLatestBlockAsync(address).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FclException("Get account error", ex);
            }
        }

        /// <summary>
        /// Gets a full block by Id.
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns><see cref="FlowBlock"/></returns>
        /// <exception cref="FclException"></exception>
        public async Task<FlowBlock> GetBlockAsync(string blockId)
        {
            try
            {
                return await _sdk.GetBlockByIdAsync(blockId).ConfigureAwait(false);
            }
            catch (Exception ex) 
            {
                throw new FclException("Get block by id error", ex);
            }
        }

        /// <summary>
        /// Gets the full payload of the latest sealed or unsealed block.
        /// </summary>
        /// <param name="isSealed"></param>
        /// <returns><see cref="FlowBlock"/></returns>
        /// <exception cref="FclException"></exception>
        public async Task<FlowBlock> GetLastestBlock(bool isSealed = true) 
        {
            try
            {
                return await _sdk.GetLatestBlockAsync(isSealed).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FclException("Get latest block error", ex);
            }
        }

        /// <summary>
        /// Gets a block header by Id.
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns><see cref="FlowBlockHeader"/></returns>
        /// <exception cref="FclException"></exception>
        public async Task<FlowBlockHeader> GetBlockHeader(string blockId) 
        {
            try
            {
                return await _sdk.GetBlockHeaderByIdAsync(blockId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FclException("Get block header error", ex);
            }
        }

        /// <summary>
        /// Gets the result of a transaction.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns><see cref="FlowTransactionResult"/></returns>
        /// <exception cref="FclException"></exception>
        public async Task<FlowTransactionResult> GetTransactionStatus(string transactionId) 
        {
            try
            {
                return await _sdk.GetTransactionResultAsync(transactionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FclException("Get transaction status error", ex);
            }
        }

        /// <summary>
        /// Gets a transaction by Id.
        /// </summary>
        /// <param name="transactionId"></param>
        /// <returns><see cref="FlowTransactionResponse"/></returns>
        /// <exception cref="FclException"></exception>
        public async Task<FlowTransactionResponse> GetTransaction(string transactionId) 
        {
            try
            {
                return await _sdk.GetTransactionAsync(transactionId).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new FclException("Get transaction error", ex);
            }
        }

        /// <summary>
        /// Override wallet provider.
        /// </summary>
        /// <param name="fclWalletDiscovery"></param>
        public void SetWalletProvider(FclWalletDiscovery fclWalletDiscovery)
        {
            _fclConfig.WalletDiscovery = fclWalletDiscovery;
        }

        private async Task<FclServiceConfig> GetServiceConfigAsync()
        {
            try
            {
                var serviceConfig = new FclServiceConfig
                {
                    Services = _fclConfig.Services,
                    App = _fclConfig.AppInfo,
                    Client = new FclClientInfo
                    {
                        Hostname = _fclConfig.Location
                    }
                };

                var clientServices = await _platform.GetClientServices();
                if (clientServices != null)
                    serviceConfig.Client.ClientServices = clientServices;

                if (_strategies.ContainsKey(FclServiceMethod.WcRpc))
                    serviceConfig.Client.ClientServices.Add(GetWallectConnectService());

                foreach (var strategy in _strategies)
                    serviceConfig.Client.SupportedStrategies.Add(strategy.Key);

                return serviceConfig;
            }
            catch (Exception ex)
            {
                throw new FclException("Get transaction error", ex);
            }            
        }

        private FclService GetWallectConnectService()
        {
            return new FclService
            {
                FclType = "Service",
                FclVsn = "1.0.0",
                Type = FclServiceType.Authn,
                Method = FclServiceMethod.WcRpc,
                Uid = "https://walletconnect.com",
                Endpoint = "flow_authn",
                OptIn = false,
                Provider = new FclServiceProvider
                {
                    Name = "WalletConnect",
                    Icon = "https://avatars.githubusercontent.com/u/37784886",
                    Description = "Wallet Connect",
                    Website = new Uri("https://walletconnect.com")
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
                Endpoint = _fclConfig.WalletDiscovery.Wallet.ToString(),
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

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

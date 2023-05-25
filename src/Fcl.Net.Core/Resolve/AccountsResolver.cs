using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Interfaces;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
using Flow.Net.Sdk.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Resolve
{
    public class AccountsResolver : IResolver
    {
        private readonly FclUser _fclUser;
        private readonly ExecService _execService;
        private readonly FclServiceConfig _fclServiceConfig;

        public AccountsResolver(FclUser fclUser, ExecService execService, FclServiceConfig fclServiceConfig)
        {
            _fclUser = fclUser;
            _execService = execService;
            _fclServiceConfig = fclServiceConfig;
        }

        public async Task ResolveAsync(FclInteraction fclInteraction)
        {
            var preAuthz = _fclUser.Services.FirstOrDefault(f => f.Type == FclServiceType.PreAuthz);

            FclAuthResponse response = null;
            if (preAuthz != null)
            {
                var preSigned = fclInteraction.BuildPreSignable(new FclRole { Proposer = true, Authorizer = true, Payer = true });
                response = await _execService.ExecuteAsync<FclAuthResponse>(preAuthz, _fclServiceConfig, preSigned).ConfigureAwait(false);
            }            
            else 
            {
                var authz = _fclUser.Services.FirstOrDefault(f => f.Type == FclServiceType.Authz) ?? throw new FclException("Authz service not found");

                var proposerService = SignableUserToService(GetProposer(fclInteraction), authz.Method, authz.Endpoint);
                var payerServices = GetPayers(fclInteraction).Select(payer => SignableUserToService(payer, authz.Method, authz.Endpoint)).ToList();
                var authorizationServices = GetAuthorizers(fclInteraction).Select(authorization => SignableUserToService(authorization, authz.Method, authz.Endpoint)).ToList();

                response = new FclAuthResponse
                {
                    Data = new FclAuthData
                    {
                        Address = authz.Identity.Address,
                        Proposer = proposerService,
                        Payer = payerServices,
                        Authorization = authorizationServices
                    }
                };
            }
                
            var signableUsers = GetSignableUsersAsync(response);

            var accounts = new Dictionary<string, FclSignableUser>();
            fclInteraction.Authorizations = new List<string>();

            foreach (var user in signableUsers)
            {
                var tempId = $"{user.Address}-{user.KeyId}";
                user.TempId = tempId;

                if (accounts.ContainsKey(tempId))
                    user.Role.Merge(accounts[tempId].Role);

                accounts[tempId] = user;

                if (user.Role.Proposer)
                    fclInteraction.Proposer = tempId;

                if (user.Role.Payer && !fclInteraction.Payer.Contains(tempId))
                    fclInteraction.Payer.Add(tempId);

                if (user.Role.Authorizer && !fclInteraction.Authorizations.Contains(tempId))
                    fclInteraction.Authorizations.Add(tempId);
            }

            fclInteraction.Accounts = accounts;
        }

        private FclSignableUser GetProposer(FclInteraction fclInteraction)
        {
            foreach (var account in fclInteraction.Accounts)
            {
                if (account.Value.Role.Proposer)
                    return account.Value;
            }

            return GetAuthz().FirstOrDefault();
        }

        private ICollection<FclSignableUser> GetAuthorizers(FclInteraction fclInteraction)
        {
            return FindSignableUsers(fclInteraction, account => account.Role.Authorizer);
        }

        private ICollection<FclSignableUser> GetPayers(FclInteraction fclInteraction)
        {
            return FindSignableUsers(fclInteraction, account => account.Role.Payer);
        }

        private ICollection<FclSignableUser> FindSignableUsers(FclInteraction fclInteraction, Func<FclSignableUser, bool> role)
        {
            var signableUsers = fclInteraction.Accounts.Values.Where(role).ToList();

            if (!signableUsers.Any())
                return GetAuthz();

            return signableUsers;
        }

        private FclService SignableUserToService(FclSignableUser fclSignableUser, FclServiceMethod fclServiceMethod, string endpoint)
        {
            if (string.IsNullOrEmpty(fclSignableUser.Address))
                return null;

            return new FclService
            {
                FclType = "Service",
                FclVsn = "1.0.0",
                Method = fclServiceMethod,
                Endpoint = endpoint,
                Identity = new FclServiceIdentity
                {
                    Address = fclSignableUser.Address,
                    KeyId = fclSignableUser.KeyId,
                }                
            };
        }

        private ICollection<FclSignableUser> GetAuthz()
        {
            var signableUsers = new List<FclSignableUser>();

            var authzServices = _fclUser.Services.Where(w => w.Type == FclServiceType.Authz);

            foreach (var service in authzServices)
            {
                var address = service.Identity?.Address;
                var keyId = service.Identity?.KeyId;

                if (address != null && keyId != null)
                {
                    var signableUser = new FclSignableUser
                    {
                        Address = address,
                        KeyId = (uint)keyId,
                        TempId = $"{address}|{keyId}",
                        Role = new FclRole
                        {
                            Proposer = false,
                            Authorizer = false,
                            Payer = true
                        },
                        SigningService = service
                    };

                    signableUsers.Add(signableUser);
                }
            }

            return signableUsers;
        }

        private ICollection<FclSignableUser> GetSignableUsersAsync(FclAuthResponse fclAuthResponse)
        {
            var axs = new List<FclSignableUserItem>();

            if (fclAuthResponse.Data?.Proposer != null)
                axs.Add(new FclSignableUserItem { Key = "PROPOSER", Service = fclAuthResponse.Data.Proposer });
            
            if(fclAuthResponse.Data?.Payer != null)
            {
                foreach(var payer in fclAuthResponse.Data.Payer)
                    axs.Add(new FclSignableUserItem { Key = "PAYER", Service = payer });
            }

            if (fclAuthResponse.Data?.Authorization != null)
            {
                foreach (var authorizer in fclAuthResponse.Data.Authorization)
                    axs.Add(new FclSignableUserItem { Key = "AUTHORIZER", Service = authorizer });
            }

            var signableUsers = new List<FclSignableUser>();

            foreach(var item in axs)
            {
                var address = item.Service.Identity?.Address;
                var keyId = item.Service.Identity?.KeyId;

                if(address != null && keyId != null)
                {
                    var signableUser = new FclSignableUser
                    {
                        Address = address,
                        KeyId = (uint)keyId,
                        TempId = $"{address}|{keyId}",
                        Role = new FclRole
                        {
                            Proposer = item.Key == "PROPOSER",
                            Authorizer = item.Key == "AUTHORIZER",
                            Payer = item.Key == "PAYER"
                        },
                        SigningService = item.Service
                    };

                    signableUsers.Add(signableUser);
                }
            }

            return signableUsers;
        }
    }
}

using Fcl.Net.Core.Exceptions;
using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
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

            if (preAuthz == null)
                throw new FclException($"User missing preAuthz.");

            var preSigned = fclInteraction.BuildPreSignable(new FclRole { Proposer = true, Authorizer = true, Payer = true });
            var response = await _execService.ExecuteAsync(preAuthz, _fclServiceConfig, preSigned).ConfigureAwait(false);
            var signableUsers = GetSignableUsersAsync(response);

            var accounts = new Dictionary<string, FclSignableUser>();
            fclInteraction.Authorizations = new List<string>();

            foreach(var user in signableUsers)
            {
                var tempId = $"{user.Address}-{user.KeyId}";
                user.TempId = tempId;

                if (accounts.ContainsKey(tempId))
                    user.Role.Merge(accounts[tempId].Role);
                
                accounts[tempId] = user;

                if(user.Role.Proposer)
                    fclInteraction.Proposer = tempId;
                
                if(user.Role.Payer)
                    fclInteraction.Payer.Add(tempId);

                if (user.Role.Authorizer)
                    fclInteraction.Authorizations.Add(tempId);
            }

            fclInteraction.Accounts = accounts;
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

            var singableUsers = new List<FclSignableUser>();

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

                    singableUsers.Add(signableUser);
                }
            }

            return singableUsers;
        }
    }
}

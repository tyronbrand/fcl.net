using Fcl.Net.Core.Models;
using Fcl.Net.Core.Service;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Resolve
{
    public class SignatureResolver : IResolver
    {
        private readonly ExecService _execService;
        private readonly FclServiceConfig _fclServiceConfig;

        public SignatureResolver(ExecService execService, FclServiceConfig fclServiceConfig)
        {
            _execService = execService;
            _fclServiceConfig = fclServiceConfig;
        }

        public async Task ResolveAsync(FclInteraction fclInteraction)
        {
            if (fclInteraction.Tag != FclInteractionTag.Transaction)
                return;

            var insideSigners = fclInteraction.FindInsideSigners();
            await FetchSignaturesAsync(fclInteraction, insideSigners, true).ConfigureAwait(false);

            var outsideSigners = fclInteraction.FindOutsideSigners();
            await FetchSignaturesAsync(fclInteraction, outsideSigners).ConfigureAwait(false);
        }

        private async Task FetchSignaturesAsync(FclInteraction fclInteraction, ICollection<string> signerIds, bool isPayload = false)
        {
            foreach (var id in signerIds)
            {
                var tx = fclInteraction.ToFlowTransaction();
                var payload = DomainTag.AddTransactionDomainTag(isPayload ? Rlp.EncodedCanonicalPayload(tx) : Rlp.EncodedCanonicalAuthorizationEnvelope(tx)).BytesToHex();

                if (!fclInteraction.Accounts.ContainsKey(id))
                    throw new FlowException("Can't find account by id.");

                var acct = fclInteraction.Accounts[id];
                var signable = CreateSignable(fclInteraction, payload, acct);
                var response = await _execService.ExecuteAsync(acct.SigningService, _fclServiceConfig, signable).ConfigureAwait(false);
                fclInteraction.Accounts[id].Signature = response.Data?.Signature ?? response.CompositeSignature?.Signature;
            }
        }

        public FclSignable CreateSignable(FclInteraction fclInteraction, string payload, FclSignableUser fclSignableUser)
        {
            return new FclSignable
            {
                Message = payload,
                KeyId = fclSignableUser.KeyId,
                Address = fclSignableUser.Address,
                Roles = fclSignableUser.Role,
                Cadence = fclInteraction.Message.Cadence,
                Args = fclInteraction.Arguments.Select(s => s.Value.AsArgument).ToList(),
                Interaction = fclInteraction,
                Voucher = fclInteraction.CreateVoucher()
            };
        }
    }
}

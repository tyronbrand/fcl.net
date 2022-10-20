using Fcl.Net.Core.Models;
using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Client;
using Flow.Net.Sdk.Core.Exceptions;
using System.Linq;
using System.Threading.Tasks;

namespace Fcl.Net.Core.Resolve
{
    public class SequenceNumberResolver : IResolver
    {
        private readonly IFlowClient _sdkClient;
        public SequenceNumberResolver(IFlowClient sdkClient)
        {
            _sdkClient = sdkClient;
        }

        public async Task ResolveAsync(FclInteraction fclInteraction)
        {
            var proposer = fclInteraction.Proposer;
            var account = proposer != null && fclInteraction.Accounts.ContainsKey(proposer) ? fclInteraction.Accounts[proposer] : null;
            var address = account?.Address;
            var keyId = account?.KeyId;

            if (proposer == null || account == null || address == null || keyId == null)
                throw new FlowException("Failed to resolve sequence number.");                       

            var flowAccount = await _sdkClient.GetAccountAtLatestBlockAsync(address).ConfigureAwait(false);
            var flowAccountKey = flowAccount.Keys.FirstOrDefault(s => s.Index == keyId);

            fclInteraction.Accounts[proposer].SequenceNumber = flowAccountKey.SequenceNumber;
        }
    }
}

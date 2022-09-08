using Flow.Net.Sdk.Core;
using Flow.Net.Sdk.Core.Cadence;
using Flow.Net.Sdk.Core.Exceptions;
using Flow.Net.Sdk.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;

namespace Fcl.Net.Core.Models
{
    public class FclInteraction
    {
        public FclInteraction()
        {
            Assigns = new Dictionary<string, string>();
            Accounts = new Dictionary<string, FclSignableUser>();
            Params = new Dictionary<string, string>();
            Arguments = new Dictionary<string, FclArgument>();
            Authorizations = new List<string>();
            Payer = new List<string>();
        }

        [JsonProperty("tag")]
        [JsonConverter(typeof(StringEnumConverter))]
        public FclInteractionTag Tag { get; set; }

        [JsonProperty("assigns")]
        public Dictionary<string, string> Assigns { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("accounts")]
        public Dictionary<string, FclSignableUser> Accounts { get; set; }

        [JsonProperty("params")]
        public Dictionary<string, string> Params { get; set; }

        [JsonProperty("arguments")]
        public Dictionary<string, FclArgument> Arguments { get; set; }

        [JsonProperty("message")]
        public FclMessage Message { get; set; }

        [JsonProperty("proposer")]
        public string Proposer { get; set; }

        [JsonProperty("authorizations")]
        public ICollection<string> Authorizations { get; set; }

        [JsonProperty("payer")]
        public ICollection<string> Payer { get; set; }

        [JsonProperty("events")]
        public FclEvents Events { get; set; }

        [JsonProperty("transaction")]
        public FclId Transaction { get; set; }

        [JsonProperty("block")]
        public FclBlock Block { get; set; }

        [JsonProperty("account")]
        public FclAccount Account { get; set; }

        [JsonProperty("collection")]
        public FclId Collection { get; set; }

        public FclPreSignable BuildPreSignable(FclRole fclRole)
        {
            var preSignable = new FclPreSignable
            {
                Roles = fclRole,
                Cadence = Message.Cadence,
                Interaction = this,
                Voucher = CreateVoucher()
            };

            if (Arguments.Any())
                preSignable.Args = Arguments?.Select(s => s.Value.AsArgument).ToList();

            return preSignable;
        }

        public FclVoucher CreateVoucher()
        {
            var voucher = new FclVoucher
            {
                Cadence = Message.Cadence,
                RefrenceBlock = Message.RefrenceBlock,
                ComputeLimit = Message.ComputeLimit,
                ProposalKey = CreateProposalKey(),
                PayloadSignatures = CreateSignatures(FindInsideSigners()),
                EnvelopeSignatures = CreateSignatures(FindOutsideSigners())
            };

            if (Arguments.Any())
                voucher.Arguments = Arguments.Select(s => s.Value.AsArgument).ToList();

            if (Payer.Any())
            {
                var payer = Payer.FirstOrDefault();

                if (!string.IsNullOrEmpty(payer))
                {
                    if (Accounts.ContainsKey(payer))
                    {
                        voucher.Payer = Accounts[payer].Address.AddHexPrefix();
                    }
                }
            }

            if (Authorizations.Any())
                voucher.Authorizers = Authorizations.Select(authorizer => Accounts[authorizer].Address.AddHexPrefix()).Distinct().ToList();

            return voucher;
        }

        public FclProposalKey CreateProposalKey()
        {
            if (string.IsNullOrEmpty(Proposer))
                return new FclProposalKey();

            Accounts.TryGetValue(Proposer, out var account);

            if (account == null)
                return new FclProposalKey();

            return new FclProposalKey
            {
                Address = account.Address.AddHexPrefix(),
                KeyId = account.KeyId,
                SequenceNum = account.SequenceNumber
            };
        }

        public List<FclSignature> CreateSignatures(ICollection<string> accounts)
        {
            var signers = new List<FclSignature>();
            foreach (var signer in accounts)
            {
                Accounts.TryGetValue(signer, out var account);

                if (account != null)
                {
                    signers.Add(new FclSignature
                    {
                        Address = account.Address.AddHexPrefix(),
                        KeyId = account.KeyId,
                        Signature = account.Signature,
                    });
                }
            }

            return signers;
        }

        public ICollection<string> FindInsideSigners()
        {
            var inside = Authorizations.ToList();

            if (!string.IsNullOrEmpty(Proposer) && !inside.Contains(Proposer))
                inside.Add(Proposer);

            if (Payer.Any())
            {
                var payer = Payer.FirstOrDefault();

                if (!string.IsNullOrEmpty(payer) && inside.Contains(payer))
                    inside.Remove(payer);
            }

            return inside;
        }

        public ICollection<string> FindOutsideSigners()
        {
            var outside = new List<string>();

            if (Payer.Any())
            {
                var payer = Payer.FirstOrDefault();

                if (!string.IsNullOrEmpty(payer))
                    outside.Add(payer);
            }

            return outside;
        }

        public FlowTransaction ToFlowTransaction()
        {
            var propKey = CreateProposalKey();
            var payer = Accounts[Payer.FirstOrDefault()]?.Address;

            if (string.IsNullOrEmpty(payer))
                throw new FlowException("Payer is missing.");

            var arguments = Arguments.Select(arg => JsonConvert.SerializeObject(arg.Value.AsArgument)).Select(cadenceStr => cadenceStr.Decode()).ToList();
            var payloadSignatures = CreateFlowSignatures(FindInsideSigners());
            var envelopeSignatures = CreateFlowSignatures(FindOutsideSigners());

            var transaction = new FlowTransaction
            {
                Script = Message.Cadence,
                Arguments = arguments,
                ReferenceBlockId = Message.RefrenceBlock,
                GasLimit = Message.ComputeLimit,
                ProposalKey = new FlowProposalKey
                {
                    Address = new FlowAddress(propKey.Address),
                    KeyId = (uint)propKey.KeyId,
                    SequenceNumber = (ulong)propKey.SequenceNum
                },
                Payer = new FlowAddress(payer),
                Authorizers = Authorizations.Select(s => new FlowAddress(Accounts[s].Address)).ToList(),
                PayloadSignatures = payloadSignatures,
                EnvelopeSignatures = envelopeSignatures
            };

            return transaction;
        }

        private List<FlowSignature> CreateFlowSignatures(ICollection<string> signers)
        {
            var flowSignatures = new List<FlowSignature>();

            foreach (var address in signers)
            {
                var account = Accounts.ContainsKey(address) ? Accounts[address] : Accounts.Select(s => s.Value).FirstOrDefault(s => s.Address == address);

                if (account != null && account.Signature != null)
                {
                    flowSignatures.Add(new FlowSignature
                    {
                        Address = new FlowAddress(account.Address),
                        KeyId = account.KeyId,
                        Signature = account.Signature?.HexToBytes(),
                    });
                }
            }

            return flowSignatures;
        }
    }

    public class FclAccount
    {
        [JsonProperty("addr")]
        public string Addr { get; set; }
    }

    public class FclBlock
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("height")]
        public ulong? Height { get; set; }

        [JsonProperty("isSealed")]
        public bool? IsSealed { get; set; }
    }

    public class FclEvents
    {
        public FclEvents()
        {
            BlockIds = new List<string>();
        }

        [JsonProperty("blockIds")]
        public ICollection<string> BlockIds { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("start")]
        public string Start { get; set; }

        [JsonProperty("end")]
        public string End { get; set; }
    }

    public class FclId
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class FclMessage
    {
        public FclMessage()
        {
            Arguments = new List<string>();
            Authorizations = new List<string>();
            Params = new List<string>();
        }

        [JsonProperty("arguments")]
        public ICollection<string> Arguments { get; set; }

        [JsonProperty("authorizations")]
        public ICollection<string> Authorizations { get; set; }

        [JsonProperty("cadence")]
        public string Cadence { get; set; }

        [JsonProperty("computeLimit")]
        public ulong ComputeLimit { get; set; }

        [JsonProperty("params")]
        public ICollection<string> Params { get; set; }

        [JsonProperty("refBlock")]
        public string RefrenceBlock { get; set; }
    }
}

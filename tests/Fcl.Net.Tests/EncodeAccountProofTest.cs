using Fcl.Net.Core.Utilities;
using Flow.Net.Sdk.Core;
using Xunit;

namespace Fcl.Net.Tests
{
    public class EncodeAccountProofTest
    {
        const string Address = "0xabc123def456";
        const string AppIdentifier = "AWESOME-APP-ID";
        const string Nonce = "3037366134636339643564623330316636626239323161663465346131393662";
        const string EncodedWithDomainTag = "46434c2d4143434f554e542d50524f4f462d56302e3000000000000000000000f8398e415745534f4d452d4150502d4944880000abc123def456a03037366134636339643564623330316636626239323161663465346131393662";
        const string EncodedWithoutDomainTag = "f8398e415745534f4d452d4150502d4944880000abc123def456a03037366134636339643564623330316636626239323161663465346131393662";

        [Fact]
        public void EncodeAccountProofWithoutDomainTag()
        {
            var message = WalletUtilities.EncodeAccountProof(Address, Nonce, AppIdentifier, false);
            Assert.Equal(EncodedWithoutDomainTag, message.BytesToHex());
        }

        [Fact]
        public void EncodeAccountProofWithDomainTag()
        {
            var message = WalletUtilities.EncodeAccountProof(Address, Nonce, AppIdentifier, true);
            Assert.Equal(EncodedWithDomainTag, message.BytesToHex());
        }
    }
}
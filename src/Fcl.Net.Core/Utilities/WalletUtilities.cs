using Flow.Net.Sdk.Core;
using System.Collections.Generic;
using System.Text;

namespace Fcl.Net.Core.Utilities
{
    public static class WalletUtilities
    {
        public static byte[] EncodeAccountProof(string address, string nonce, string appIdentifier, bool includeDomainTag)
        {
            var signatureArray = new List<byte[]>
            {
                Rlp.EncodeElement(Encoding.UTF8.GetBytes(appIdentifier)),
                Rlp.EncodeElement(Flow.Net.Sdk.Core.Utilities.Pad(address.HexToBytes(), 8)),
                Rlp.EncodeElement(nonce.HexToBytes()),
            };

            var encoded = Rlp.EncodeList(signatureArray.ToArray());

            if(includeDomainTag)
            {
                encoded = Flow.Net.Sdk.Core.Utilities.CombineByteArrays(new[]
                {
                    Flow.Net.Sdk.Core.Utilities.Pad("FCL-ACCOUNT-PROOF-V0.0", 32, false),
                    encoded
                });
            }

            return encoded;
        }
    }
}

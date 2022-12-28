using Fcl.Net.Core;

namespace Fcl.Net.Maui.WalletProviders
{
    public class FclWalletProvider
    {
        public string Name { get; set; }
        public Uri Logo { get; set; }
        public FclServiceMethod Method { get; set; }
        public string Endpoint { get; set; }
    }
}

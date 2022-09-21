using Fcl.Net.Core.Models;
using System.Collections.Generic;

namespace Fcl.Net.Core.Config
{
    public class FclConfig
    {
        public FclConfig(FclWalletDiscovery walletDiscovery, FclAppInfo appInfo, string location)
        {
            WalletDiscovery = walletDiscovery;
            AppInfo = appInfo;
            Location = location;
        }

        public FclWalletDiscovery WalletDiscovery { get; set; }
        public FclAppInfo AppInfo { get; }
        public string Location { get; }
        public Dictionary<string, string> Services { get; set; }
        public FclAccountProofData AccountProof { get; set; }
    }
}

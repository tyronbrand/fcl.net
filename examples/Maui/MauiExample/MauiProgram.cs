using Fcl.Net.Core.Config;
using Fcl.Net.Core.Models;
using Fcl.Net.Core;
using Fcl.Net.Maui;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;

namespace MauiExample
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
#if TIZEN
				.UseMauiCompatibility()
#endif
                .ConfigureEssentials(essentials =>
                {
                    essentials.UseVersionTracking();
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // sdk 
            var sdkOptions = new FlowClientOptions
            {
                ServerUrl = ServerUrl.TestnetHost
            };

            // fcl config
            var appInfo = new FclAppInfo
            {
                Icon = new Uri("https://avatars.githubusercontent.com/u/62387156?s=200&v=4"),
                Title = "Blazor Example"
            };

            var fclConfig = new FclConfig(appInfo, ChainId.Testnet)
            {
                AccountProof = new FclAccountProofData("AWESOME-BLAZOR-APP-ID", "3037366134636339643564623330316636626239323161663465346131393662")
            };

            // maui redirect URI
            var redirectUri = new Uri("fclmaui://");

            builder.Services.AddFclServices(sdkOptions, fclConfig, redirectUri);

            return builder.Build();
        }
    }
}
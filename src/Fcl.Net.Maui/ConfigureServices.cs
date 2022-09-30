using Fcl.Net.Core;
using Fcl.Net.Core.Config;
using Fcl.Net.Core.Platform;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategies;
using Fcl.Net.Maui.Strategies;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;

namespace Fcl.Net.Maui
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddFclServices(this IServiceCollection services, FlowClientOptions sdkClientOptions, FclConfig fclConfig, Uri redirectUri)
        {
            // platform
            services.AddSingleton<IPlatform, MauiPlatform>();

            // sdk client
            services.AddSingleton(f => sdkClientOptions);
            services.AddHttpClient<IFlowClient, FlowHttpClient>();

            // fetch service
            services.AddSingleton(f =>
            {
                var fetchServiceConfig = new FetchServiceConfig
                {
                    Location = fclConfig.Location
                };

                return fetchServiceConfig;
            });
            services.AddHttpClient<FetchService>();

            // browsers
            services.AddSingleton(b => WebAuthenticator.Default);

            // strategies
            services.AddSingleton(f =>
            {
                return new MauiHttpPostStrategy(f.GetRequiredService<IWebAuthenticator>(), redirectUri, f.GetRequiredService<FetchService>(), null);
            });

            // fcl
            services.AddSingleton(f => fclConfig);
            services.AddSingleton(f =>
            {
                // strategies
                var strategies = new Dictionary<FclServiceMethod, IStrategy>
                {
                    { FclServiceMethod.HttpPost, f.GetRequiredService<MauiHttpPostStrategy>() }
                };

                return new Core.Fcl(
                    f.GetRequiredService<FclConfig>(),
                    f.GetRequiredService<IFlowClient>(),
                    f.GetRequiredService<IPlatform>(),
                    strategies);
            });

            return services;
        }
    }
}

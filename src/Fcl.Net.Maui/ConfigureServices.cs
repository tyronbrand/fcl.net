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
    /// <summary>
    /// Adds MAUI services
    /// </summary>
    public static class ConfigureServices
    {
        private static readonly HttpClient _httpClient = new();

        public static IServiceCollection AddFclServices(this IServiceCollection services, FlowClientOptions sdkClientOptions, FclConfig fclConfig, Uri redirectUri)
        {
            // platform
            services.AddSingleton<IPlatform, MauiPlatform>();

            // sdk client
            services.AddSingleton(f => sdkClientOptions);
            services.AddSingleton<IFlowClient>(f =>
            {
                return new FlowHttpClient(_httpClient, f.GetRequiredService<FlowClientOptions>());
            });

            // fetch service
            services.AddSingleton(f =>
            {
                var fetchServiceConfig = new FetchServiceConfig
                {
                    Location = fclConfig.Location
                };

                return new FetchService(_httpClient, fetchServiceConfig);
            });

            // strategies
            services.AddSingleton(f =>
            {
                return new MauiHttpPostStrategy(redirectUri, f.GetRequiredService<FetchService>(), null);
            });

            // fcl
            services.AddSingleton(f =>
            {
                // strategies
                var strategies = new Dictionary<FclServiceMethod, IStrategy>
                {
                    { FclServiceMethod.HttpPost, f.GetRequiredService<MauiHttpPostStrategy>() }
                };

                return new Core.Fcl(
                    fclConfig,
                    f.GetRequiredService<IFlowClient>(),
                    f.GetRequiredService<IPlatform>(),
                    strategies);
            });

            return services;
        }
    }
}

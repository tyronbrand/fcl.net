using Fcl.Net.Core;
using Fcl.Net.Core.Config;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategy;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;

namespace Fcl.Net.Maui
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddFclServices(this IServiceCollection services, FlowClientOptions sdkClientOptions, FclConfig fclConfig)
        {
            // sdk client
            services.AddSingleton(f =>
            {
                return sdkClientOptions;
            });
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

            // local views

            // strategies

            // fcl
            services.AddSingleton(f =>
            {
                return fclConfig;
            });

            services.AddSingleton(f =>
            {
                // local views
                var localViews = new Dictionary<FclServiceMethod, ILocalView>
                {                    
                };

                // strategies
                var strategies = new Dictionary<FclServiceMethod, IStrategy>
                {                    
                };

                return new Core.Fcl(
                    f.GetRequiredService<IFlowClient>(),
                    f.GetRequiredService<FclConfig>(),
                    localViews,
                    f.GetRequiredService<FetchService>(),
                    strategies);
            });

            return services;
        }
    }
}

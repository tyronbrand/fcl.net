using Fcl.Net.Core;
using Fcl.Net.Core.Config;
using Fcl.Net.Core.Platform;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategies;
using Fcl.Net.Core.Service.Strategies.LocalViews;
using Fcl.Net.Maui.LocalViews;
using Fcl.Net.Maui.WebBrowser;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;

namespace Fcl.Net.Maui
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddFclServices(this IServiceCollection services, FlowClientOptions sdkClientOptions, FclConfig fclConfig)
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
            services.AddSingleton<IWebBrowser, Browser>();

            // local views
            services.AddSingleton<BrowserView>();

            // strategies
            services.AddSingleton(f =>
            {
                // local views
                var localViews = new Dictionary<FclServiceMethod, ILocalView>
                {
                    { FclServiceMethod.HttpPost, f.GetRequiredService<BrowserView>() }
                };

                return new HttpPostStrategy(f.GetRequiredService<FetchService>(), localViews);
            });

            // fcl
            services.AddSingleton(f => fclConfig);
            services.AddSingleton(f =>
            {
                // strategies
                var strategies = new Dictionary<FclServiceMethod, IStrategy>
                {
                    { FclServiceMethod.HttpPost, f.GetRequiredService<HttpPostStrategy>() }
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

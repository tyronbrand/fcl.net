using System.Collections.Generic;
using Fcl.Net.Blazor.LocalView;
using Fcl.Net.Blazor.Strategy;
using Fcl.Net.Core;
using Fcl.Net.Core.Config;
using Fcl.Net.Core.Service;
using Fcl.Net.Core.Service.Strategy;
using Flow.Net.Sdk.Client.Http;
using Flow.Net.Sdk.Core.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Fcl.Net.Blazor
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

            // JS interop
            services.AddSingleton<FclJsObjRef>();

            // local views
            services.AddSingleton<IFrameLocalView>();
            services.AddSingleton<PopLocalView>();

            // strategies
            services.AddSingleton<JsStrategy>();

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
                    { FclServiceMethod.ViewIFrame, f.GetRequiredService<IFrameLocalView>() },
                    { FclServiceMethod.ViewPop, f.GetRequiredService<PopLocalView>() },
                    { FclServiceMethod.BrowserIframe, fclConfig.WalletDiscovery.WalletMethod == FclServiceMethod.IFrameRPC ? f.GetRequiredService<IFrameLocalView>() : f.GetRequiredService<PopLocalView>() }
                };

                // strategies
                var strategies = new Dictionary<FclServiceMethod, IStrategy>
                {
                    { FclServiceMethod.IFrameRPC, f.GetRequiredService<JsStrategy>() },
                    { FclServiceMethod.PopRpc, f.GetRequiredService<JsStrategy>() }
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

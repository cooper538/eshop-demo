using EShop.AnalyticsService.Configuration;
using EShop.AnalyticsService.Consumers;
using EShop.Common.Infrastructure.Extensions;

namespace EShop.AnalyticsService;

public static class DependencyInjection
{
    public static IHostApplicationBuilder AddAnalyticsServices(this IHostApplicationBuilder builder)
    {
        builder
            .Services.AddOptions<AnalyticsSettings>()
            .BindConfiguration(AnalyticsSettings.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddMessaging(
            builder.Configuration,
            endpointPrefix: "analytics",
            configureConsumers: x => x.AddConsumer<OrderConfirmedConsumer>()
        );

        return builder;
    }
}

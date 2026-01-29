using MassTransit;

namespace EShop.Common.Infrastructure.Correlation.MassTransit;

public static class MassTransitCorrelationExtensions
{
    public static void UseCorrelationIdFilters(
        this IBusFactoryConfigurator configurator,
        IRegistrationContext context
    )
    {
        configurator.UsePublishFilter(typeof(CorrelationIdPublishFilter<>), context);
        configurator.UseSendFilter(typeof(CorrelationIdSendFilter<>), context);
        configurator.UseConsumeFilter(typeof(CorrelationIdConsumeFilter<>), context);
    }
}

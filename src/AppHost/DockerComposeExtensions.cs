namespace EShop.AppHost;

public record AppResources(
    IResourceBuilder<PostgresServerResource> Postgres,
    IResourceBuilder<RabbitMQServerResource> RabbitMq,
    IResourceBuilder<ProjectResource> MigrationService,
    IResourceBuilder<ProjectResource> ProductService,
    IResourceBuilder<ProjectResource> OrderService,
    IResourceBuilder<ProjectResource> NotificationService,
    IResourceBuilder<ProjectResource> AnalyticsService,
    IResourceBuilder<ProjectResource> Gateway
);

public static class DockerComposeExtensions
{
    public static void ConfigureDockerComposePublishing(
        this IDistributedApplicationBuilder builder,
        AppResources resources
    )
    {
        if (!builder.ExecutionContext.IsPublishMode)
        {
            return;
        }

        builder.AddDockerComposeEnvironment("docker-compose");

        // Infrastructure - always running
        resources.Postgres.PublishAsDockerComposeService((_, s) => s.Restart = "unless-stopped");
        resources.RabbitMq.PublishAsDockerComposeService((_, s) => s.Restart = "unless-stopped");

        // Migration - one-time job
        resources.MigrationService.PublishAsDockerComposeService((_, s) => s.Restart = "no");

        // Application services - restart on failure
        resources.ProductService.PublishAsDockerComposeService((_, s) => s.Restart = "on-failure");
        resources.OrderService.PublishAsDockerComposeService((_, s) => s.Restart = "on-failure");
        resources.NotificationService.PublishAsDockerComposeService(
            (_, s) => s.Restart = "on-failure"
        );
        resources.AnalyticsService.PublishAsDockerComposeService(
            (_, s) => s.Restart = "on-failure"
        );
        resources.Gateway.PublishAsDockerComposeService((_, s) => s.Restart = "on-failure");
    }
}

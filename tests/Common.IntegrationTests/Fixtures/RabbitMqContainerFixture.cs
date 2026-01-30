using Testcontainers.RabbitMq;

namespace EShop.Common.IntegrationTests.Fixtures;

public sealed class RabbitMqContainerFixture : IAsyncLifetime
{
    private readonly RabbitMqContainer _container = new RabbitMqBuilder()
        .WithImage("rabbitmq:3-management-alpine")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public string Hostname => _container.Hostname;

    public int AmqpPort => _container.GetMappedPublicPort(5672);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

// ReSharper disable once CheckNamespace
#pragma warning disable IDE0130 // Namespace intentionally differs - extension methods for Microsoft.Extensions.Hosting
namespace Microsoft.Extensions.Hosting;

public static class Extensions
{
    /// <summary>
    /// Adds common Aspire service defaults including OpenTelemetry, health checks,
    /// service discovery, and resilience policies.
    /// </summary>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddServiceDiscovery();
        });

        return builder;
    }

    /// <summary>
    /// Configures OpenTelemetry for logging, metrics, and tracing.
    /// </summary>
    public static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder
    )
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource("EShop.*")
                    .AddAspNetCoreInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(
        this IHostApplicationBuilder builder
    )
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
        );

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    /// <summary>
    /// Adds default health checks for liveness and readiness probes.
    /// </summary>
    public static IHostApplicationBuilder AddDefaultHealthChecks(
        this IHostApplicationBuilder builder
    )
    {
        builder
            .Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps the default health check endpoints (/health, /alive).
    /// </summary>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health");

        app.MapHealthChecks(
            "/alive",
            new HealthCheckOptions { Predicate = r => r.Tags.Contains("live") }
        );

        return app;
    }

    /// <summary>
    /// Configures Serilog with console (compact JSON) and file sinks.
    /// </summary>
    public static IHostApplicationBuilder AddSerilog(
        this IHostApplicationBuilder builder,
        string logDirectory = "logs"
    )
    {
        var serviceName =
            builder.Configuration["OTEL_SERVICE_NAME"] ?? builder.Environment.ApplicationName;

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ServiceName", serviceName)
            .WriteTo.Console(new CompactJsonFormatter())
            .WriteTo.File(
                new CompactJsonFormatter(),
                Path.Combine(logDirectory, $"{serviceName}-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7
            )
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Services.AddSerilog();

        return builder;
    }
}

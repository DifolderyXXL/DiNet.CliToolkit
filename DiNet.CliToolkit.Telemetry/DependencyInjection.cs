using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Services;
using DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;
using DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;
using DiNet.HashSimilarityTK.Cli.Services.Telemetry;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Runtime.Serialization;

namespace DiNet.HashSimilarityTK.Cli;

public static class DependencyInjection
{
    public static void ConfigureObservability(this HostApplicationBuilder builder, LogLevel logLevel)
    {
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(logLevel);

        if (logLevel != LogLevel.None)
        {
            builder.Logging.AddConsole();
            builder.Logging.AddFilter("Microsoft.Hosting", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.Warning);
        }

        builder.Services.AddSingleton(Diagnostics.Source);
        builder.Services.AddSingleton<ITelemetry, Telemetry>();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(Diagnostics.ServiceName, Diagnostics.ServiceVersion))
            .WithTracing(tracing => tracing
                .AddSource(Diagnostics.Source.Name)
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri("http://localhost:4318/v1/traces");
                    o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    o.ExportProcessorType = ExportProcessorType.Simple;
                }))
            .WithMetrics(metrics => metrics
                .AddMeter(Diagnostics.Meter.Name)
                .AddOtlpExporter(o =>
                {
                    o.Endpoint = new Uri("http://localhost:4318/v1/metrics");
                    o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    o.ExportProcessorType = ExportProcessorType.Simple;
                }));

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.AddOtlpExporter(o =>
            {
                o.Endpoint = new Uri("http://localhost:4318/v1/logs");
                o.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });
    }

    public static void ConfigureBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentMatchService, DocumentMatchService>();
        services.AddSingleton(new LineFilterBuilder()
            .IgnoreWhitespace()
            .MinLength(3)
            .IgnoreStructuralSymbols()
            .IgnoreComments()
            .IgnoreImports()
            .Build());

        services.AddSingleton<DocumentReader>();
        services.AddSingleton<IDocumentReader>(sp => sp.GetRequiredService<DocumentReader>());
        services.Decorate<IDocumentReader, FilteredDocumentReader>();
    }

    public static void ConfigureCli(this IServiceCollection services)
    {
        services.AddCliToolkit(cli =>
        {
            cli.UseKebabCaseFormatter();
            cli.Handlers
                .IntroduceHelpCommand("help")
                .Register<GetTopFileSimilarityHandler>("top")
                .Register<GetLongestSequenceSimilarityHandler>("seq")
                .Register<IndexingStatisticsHandler>("index");
        });

        services.AddSingleton<IQueryHandlerResolver>(sp =>
            new TelemetryQueryHandlerResolver(
                new DefaultQueryHandlerResolver(sp),
                sp.GetRequiredService<ITelemetry>()));
    }
}

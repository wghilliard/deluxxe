using System.Diagnostics;
using Deluxxe.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Xunit.Abstractions;

namespace Deluxxe.Tests;

public class BaseTest : IDisposable
{
    private readonly TracerProvider? _tracerProvider;

    protected ActivitySource activitySource { get; private set; }

    protected ILoggerFactory loggerFactory { get; private set; }

    protected BaseTest(ITestOutputHelper testOutputHelper)
    {
        activitySource = new(GetType().Name);
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("Deluxxe.Tests"))
            .AddSource(GetType().Name)
            .AddConsoleExporter(opts => opts.Targets = ConsoleExporterOutputTargets.Debug)
            .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:4317"); })
            .Build();

        loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(logging =>
            {
                // logging.AddConsoleExporter();
                logging.AddOtlpExporter();
            });
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, XUnitLoggerProvider>());
            builder.Services.AddSingleton(testOutputHelper);
        });

        const bool shouldLogTraces = true;
        if (shouldLogTraces)
        {
            Trace.Listeners.Add(new LoggerTraceListener(loggerFactory));
        }
    }

    public void Dispose()
    {
        _tracerProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
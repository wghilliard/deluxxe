using System.Diagnostics;
using Deluxxe.Extensions;
using Microsoft.Extensions.Hosting;
using DeluxxeCli;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogging(opts => opts.AddConsole());
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("DeluxxeCli"))
    .WithTracing(tracing => tracing
        .AddSource("DeluxxeCli")
        .AddConsoleExporter(opts => opts.Targets = ConsoleExporterOutputTargets.Debug)
        .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:4317"); }));

builder.Services.AddSingleton(new ActivitySource("DeluxxeCli"));
builder.Services.AddHostedService<CliWorker>();
builder.Services.AddDeluxxe();
builder.Services.AddHttpClient();

var completionTokenSource = new CancellationTokenSource();
builder.Services.AddSingleton(new CompletionToken(completionTokenSource));

var host = builder.Build();

await host.RunAsync(completionTokenSource.Token);
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using Deluxxe.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DeluxxeCli;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand();
        var raffleCommand = new Command("raffle");
        var configFileOption = new Option<FileInfo>(name: "--config-file")
        {
            IsRequired = true
        };
        configFileOption.AddAlias("-c");
        raffleCommand.Add(configFileOption);
        raffleCommand.SetHandler(async configFile => { await HandleRaffleCommand(configFile); },
            configFileOption);

        rootCommand.Add(raffleCommand);


        await rootCommand.InvokeAsync(args);
    }

    private static async Task HandleRaffleCommand(FileInfo configFile)
    {
        await using var reader = configFile.OpenRead();
        var raffleRunConfig = JsonSerializer.Deserialize<RaffleRunConfiguration>(reader);
        reader.Close();

        if (raffleRunConfig is null)
        {
            await Console.Error.WriteLineAsync("No raffle run configuration found.");
        }

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton(raffleRunConfig!.serializerOptions);
        builder.Services.AddLogging(opts => opts.AddConsole());

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("DeluxxeCli"))
            .WithTracing(tracing => tracing
                .AddSource("DeluxxeCli")
                .AddConsoleExporter(opts => opts.Targets = ConsoleExporterOutputTargets.Debug)
                .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:4317"); }));

        builder.Services.AddSingleton(new ActivitySource("DeluxxeCli"));
        builder.Services.AddHostedService<RaffleCliWorker>();
        builder.Services.AddDeluxxe();
        builder.Services.AddDeluxxeJson();
        builder.Services.AddHttpClient();

        var completionTokenSource = new CancellationTokenSource();
        builder.Services.AddSingleton(new CompletionToken(completionTokenSource));
        builder.Services.AddSingleton(raffleRunConfig);
        builder.Services.AddSingleton(raffleRunConfig.raffleConfiguration);
        var host = builder.Build();

        await host.RunAsync(completionTokenSource.Token);
    }
}
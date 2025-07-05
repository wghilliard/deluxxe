using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using Deluxxe.Extensions;
using Deluxxe.IO;
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
        // common options
        var outputDirOption = new Option<DirectoryInfo>("--output-dir") { IsRequired = true };
        outputDirOption.AddAlias("-o");
        outputDirOption.SetDefaultValue(new DirectoryInfo("./output"));
        var eventNameOption = new Option<string>("--event-name") { IsRequired = true };
        eventNameOption.AddAlias("-e");

        // commands
        var raffleCommand = new Command("raffle")
        {
            eventNameOption,
            outputDirOption
        };
        raffleCommand.SetHandler(HandleRaffleCommand, outputDirOption, eventNameOption);

        var validateDriversCommand = new Command("validate-drivers")
        {
            eventNameOption,
            outputDirOption
        };
        validateDriversCommand.SetHandler(HandleValidateDriversCommand, outputDirOption, eventNameOption);

        var downloadStickerMapCommand = new Command("download-sticker-map")
        {
            outputDirOption
        };
        downloadStickerMapCommand.SetHandler(HandleDownloadStickerMapCommand, outputDirOption);

        var dateOption = new Option<string>("--date") { IsRequired = true };
        var mylapsAccountOption = new Option<string>("--mylaps-account-id");
        var eventIdOption = new Option<int?>("--event-id");
        var createEventCommand = new Command("create-event")
        {
            eventNameOption,
            dateOption,
            outputDirOption,
            mylapsAccountOption,
            eventIdOption
        };

        createEventCommand.SetHandler(HandleCreateEventCommand, eventNameOption, dateOption, outputDirOption, mylapsAccountOption, eventIdOption);

        var renderEmailsCommand = new Command("render-emails")
        {
            eventNameOption,
            outputDirOption
        };
        renderEmailsCommand.SetHandler(HandleRenderEmailsCommand, outputDirOption, eventNameOption);

        // entrypoint
        var rootCommand = new RootCommand
        {
            validateDriversCommand,
            raffleCommand,
            createEventCommand,
            renderEmailsCommand,
            downloadStickerMapCommand
        };

        await rootCommand.InvokeAsync(args);
    }

    private static async Task HandleDownloadStickerMapCommand(DirectoryInfo outputDir)
    {
        var (builder, completionTokenSource) = HostApplicationBuilder(new RuntimeContext
        {
            outputDir = outputDir
        });

        builder.Services.AddHostedService<DownloadStickerMapCliWorker>();
        var host = builder.Build();
        await host.RunAsync(completionTokenSource.Token);
    }

    private static async Task HandleCreateEventCommand(string eventName, string date, DirectoryInfo outputDir, string? mylapsAccountId, int? eventId)
    {
        var (builder, completionTokenSource) = HostApplicationBuilder(new RuntimeContext
        {
            outputDir = outputDir,
            uniqueEventName = eventName
        });
        builder.Services.AddSingleton(new CreateEventOptions
        {
            EventName = eventName,
            Date = date,
            OutputDir = outputDir.FullName,
            MylapsAccountId = mylapsAccountId,
            EventId = eventId
        });
        builder.Services.AddSingleton(new RaffleSerializerOptions
        {
            shouldOverwrite = true,
            writeIntermediates = true
        });

        builder.Services.AddHostedService<CreateEventCliWorker>();
        var host = builder.Build();

        await host.RunAsync(completionTokenSource.Token);
    }

    private static async Task HandleRaffleCommand(DirectoryInfo outputDir, string eventName)
    {
        var ctx = new RuntimeContext
        {
            outputDir = outputDir,
            uniqueEventName = eventName
        };
        var directoryManager = new FileSystemDirectoryManager(ctx);
        await using var reader = directoryManager.deluxxeConfigFile.OpenRead();
        var raffleRunConfig = JsonSerializer.Deserialize<RaffleRunConfiguration>(reader);
        reader.Close();

        if (raffleRunConfig is null)
        {
            await Console.Error.WriteLineAsync("No raffle run configuration found.");
            return;
        }

        var (builder, completionTokenSource) = HostApplicationBuilder(new RuntimeContext
        {
            outputDir = outputDir,
            uniqueEventName = eventName
        });
        builder.Services.AddSingleton(raffleRunConfig);
        builder.Services.AddSingleton(raffleRunConfig.raffleConfiguration);
        builder.Services.AddSingleton(raffleRunConfig.serializerOptions);

        builder.Services.AddHostedService<RaffleCliWorker>();
        var host = builder.Build();

        await host.RunAsync(completionTokenSource.Token);
    }

    private static async Task HandleValidateDriversCommand(DirectoryInfo outputDir, string eventName)
    {
        var ctx = new RuntimeContext
        {
            outputDir = outputDir,
            uniqueEventName = eventName
        };
        var directoryManager = new FileSystemDirectoryManager(ctx);
        await using var reader = directoryManager.deluxxeConfigFile.OpenRead();
        var raffleRunConfig = JsonSerializer.Deserialize<RaffleRunConfiguration>(reader);
        reader.Close();

        if (raffleRunConfig is null)
        {
            await Console.Error.WriteLineAsync("No raffle run configuration found.");
            return;
        }


        var (builder, completionTokenSource) = HostApplicationBuilder(new RuntimeContext
        {
            outputDir = outputDir,
            uniqueEventName = eventName
        });

        builder.Services.AddSingleton(raffleRunConfig);
        builder.Services.AddSingleton(raffleRunConfig.raffleConfiguration);
        builder.Services.AddSingleton(raffleRunConfig.serializerOptions);

        builder.Services.AddHostedService<ValidateDriversCliWorker>();
        var host = builder.Build();

        await host.RunAsync(completionTokenSource.Token);
    }

    private static async Task HandleRenderEmailsCommand(DirectoryInfo outputDir, string eventName)
    {
        var ctx = new RuntimeContext
        {
            outputDir = outputDir,
            uniqueEventName = eventName
        };
        var directoryManager = new FileSystemDirectoryManager(ctx);
        await using var reader = directoryManager.deluxxeConfigFile.OpenRead();
        var raffleRunConfig = JsonSerializer.Deserialize<RaffleRunConfiguration>(reader);
        reader.Close();

        if (raffleRunConfig is null)
        {
            await Console.Error.WriteLineAsync("No raffle run configuration found.");
            return;
        }

        var (builder, completionTokenSource) = HostApplicationBuilder(ctx);

        builder.Services.AddSingleton(raffleRunConfig);
        builder.Services.AddSingleton(raffleRunConfig.raffleConfiguration);
        builder.Services.AddSingleton(raffleRunConfig.serializerOptions);

        builder.Services.AddHostedService<RenderEmailsCliWorker>();
        var host = builder.Build();

        await host.RunAsync(completionTokenSource.Token);
    }

    private static (HostApplicationBuilder builder, CancellationTokenSource completionTokenSource) HostApplicationBuilder(RuntimeContext ctx)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddLogging(opts => opts.AddSimpleConsole(opts => { opts.SingleLine = true; }));
        builder.Services.AddSingleton(ctx);
        builder.Services.AddSingleton<IDirectoryManager, FileSystemDirectoryManager>();

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("DeluxxeCli"))
            .WithTracing(tracing => tracing
                .AddSource("DeluxxeCli")
                .AddHttpClientInstrumentation()
                .AddConsoleExporter(opts => opts.Targets = ConsoleExporterOutputTargets.Debug)
                .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://localhost:4317"); }));

        builder.Services.AddSingleton(new ActivitySource("DeluxxeCli"));
        builder.Services.AddDeluxxe();
        builder.Services.AddDeluxxeJson();
        builder.Services.AddHttpClient();

        var completionTokenSource = new CancellationTokenSource();
        builder.Services.AddSingleton(new CompletionToken(completionTokenSource));
        return (builder, completionTokenSource);
    }
}
// See https://aka.ms/new-console-template for more information

/*
 * Date: 2025-09-24
 * Description: A simple console app that watches a folder for new files and publishes events to RabbitMQ using Rebus.
 *
 * Written by: ChatGPT-4.0 Copilot
 * Reviewed by: Ekin BULUT
 */

using Domain.Interfaces;
using Infrastructure.Database.Extensions;
using Infrastructure.Integration.Services;
using Infrastructure.Integration.Services.JellyFin;
using Infrastructure.Messaging.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Events;
using Watcher.Console.App.Factories;
using Watcher.Console.App.Handlers;
using Watcher.Console.App.Models;
using Watcher.Console.App.Services;

string title = "Hive Folder Watcher";
Console.Title = title;

//write the app version and name
var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

// --- Host/DI bootstrap -------------------------------------------------------
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o =>
        {
            o.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] ";
            o.SingleLine = true;
        });
    })
    .ConfigureServices((ctx, services) =>
    {
        // RabbitMQ connection
        // Prefer appsettings.json: ConnectionStrings:RabbitMq
        // Fallback to env var RABBITMQ__CONNECTION, then default localhost
        var rabbitConn =
            ctx.Configuration.GetConnectionString("RabbitMq")
            ?? Environment.GetEnvironmentVariable("RABBITMQ__CONNECTION")
            ?? "amqp://guest:guest@localhost:5672";
        
        var jellyfinUrl = ctx.Configuration["JellyFin:BaseUrl"]
                          ?? Environment.GetEnvironmentVariable("JELLYFIN_BASE_URL")
                          ?? throw new ArgumentNullException("JellyFin:BaseUrl not configured");
        
        var jellyfinApiKey = ctx.Configuration["JellyFin:ApiKey"]
                              ?? Environment.GetEnvironmentVariable("JELLYFIN_ACCESS_TOKEN")
                              ?? throw new ArgumentNullException("JellyFin:ApiKey not configured");

        var inputQueue = "hive-watcher";
        services.AutoRegisterHandlersFromAssemblyOf<MessageHandler>();

        services.AddMessaging(rabbitConn, inputQueue, 1);
        
        services.AddDbContext(ctx.Configuration);

        services.AddHttpClient();
        services.AddTransient<ITmdbApiService, TmdbApiService>();
        services.AddTransient<IJellyFinService, JellyFinService>(sp =>
            new JellyFinService(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<ILogger<JellyFinService>>(),
                jellyfinApiKey,
                jellyfinUrl));
        // Register watcher services with proper buffer management
        services.AddTransient<IFileSystemWatcherFactory, FileSystemWatcherFactory>();
        services.AddTransient<IFileSystemService, FileSystemService>();
        services.AddTransient<IConsoleLogger, ConsoleLogger>();
        services.AddSingleton<IWatcherManager, WatcherManager>();
    });

using var host = builder.Build();
await host.StartAsync();

// Create watcher with proper DI services to enable buffer management
var watcherFactory = host.Services.GetRequiredService<IFileSystemWatcherFactory>();
var fileSystemService = host.Services.GetRequiredService<IFileSystemService>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation($"{title} v{appVersion}");

WatcherArguments watcherArgs;
try
{
    watcherArgs = new WatcherArguments(Environment.GetCommandLineArgs());
}
catch (Exception ex)
{
    logger.LogError(ex.Message);
    return;
}

if (watcherArgs.ShowHelp)
{
    //write possible arguments
    logger.LogInformation($"{title} - A simple file system watcher.");
    logger.LogInformation("Options:");
    logger.LogInformation("  --watch or -w <folder_to_watch>   Specifies the folder to watch.");
    logger.LogInformation("  --help                      Displays this help message.");
    logger.LogInformation("");
    logger.LogInformation("Usage: Watcher.Console.App --watch <folder_to_watch>");
    return;
}

logger.LogInformation("");

logger.LogInformation($"Watching folder: {watcherArgs.FolderToWatch}");
logger.LogInformation("Press 'q' to quit or Ctrl+C to exit.");
logger.LogInformation("");

// Resolve Rebus bus through DI
var bus = host.Services.GetRequiredService<Rebus.Bus.IBus>();
await bus.Subscribe<FileFoundEvent>();

// Subscribe to WatchPathChangedEvent from a different queue (e.g., from API)
await bus.Advanced.Topics.Subscribe("hive-api.path-changed");

// --- Define a lightweight publish DTO that implements your abstraction -------
var defaultCorrelationId = Guid.NewGuid().ToString("N");



var watcher = new Watcher.Console.App.Services.Watcher(
    path: watcherArgs.FolderToWatch,
    watcherFactory,
    fileSystemService,
    logger: logger,                       
    filter: "*.*",
    includeSubdirectories: true,
    allowedExtensions: new[] { ".mkv", ".mov", ".mp4" } // optional
);

//
// watcher.Changed += (s, e) => Console.WriteLine($"[Changed] {e.FullPath}");
// watcher.Renamed += (s, e) => Console.WriteLine($"[Renamed] {e.OldFullPath} -> {e.FullPath}");
// watcher.Error   += (s, e) => Console.WriteLine($"[Error] {e.GetException().Message}");

watcher.FileContentDiscovered += (s, e) =>
{
    
    logger.LogInformation("File Discovered: {Path} (Size: {Size} bytes)", e.Path, e.Size);

    // THIS IS OVER ENGINEERING !!
    var fileEvent = new FileFoundEvent
    {
        FilePaths = new List<string> { e.Path },
        MetaData = new MetaData(e.Name, e.Size, e.Extension),
        CausationId = defaultCorrelationId
    };

    bus.Publish(fileEvent);
};
   
//watcher.Start();
// watcher.PerformInitialScan();

var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    logger.LogWarning("\nShutting down watcher...");
    cancellationTokenSource.Cancel();
};

// Check if running in interactive mode
if (Environment.UserInteractive && !Console.IsInputRedirected)
{
    // logger.LogInformation("Press 'q' to quit or Ctrl+C to exit.");
    
    // Run the interactive loop in a background task
    _ = Task.Run(async () =>
    {
        try
        {
            while (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                var key = Console.ReadKey(true);
                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    logger.LogWarning("\nShutting down watcher...");
                    cancellationTokenSource.Cancel();
                    break;
                }
            }
        }
        catch (InvalidOperationException)
        {
            // Console input not available, ignore
        }
    });
}
else
{
    logger.LogInformation("Running in non-interactive mode. Use Ctrl+C to exit.");
}

// Wait for cancellation
try
{
    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
}
catch (TaskCanceledException)
{
    // Expected when cancellation is requested
}

logger.LogWarning("Stopping watcher...");
watcher.Stop();

// Stop Rebus/Host

await host.StopAsync();

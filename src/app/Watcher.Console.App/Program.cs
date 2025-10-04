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
using Infrastructure.Messaging.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Watcher.Console.App.Events;
using Watcher.Console.App.Handlers;
using Watcher.Console.App.Models;
using Watcher.Console.App.Services;

string title = "Hive Folder Watcher";
Console.Title = title;

//write the app version and name
var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
Console.WriteLine($"{title} v{appVersion}");

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

        var inputQueue = "hive-watcher";
        services.AutoRegisterHandlersFromAssemblyOf<MessageHandler>();

        services.AddMessaging(rabbitConn, inputQueue, 1);
        
        services.AddDbContext(ctx.Configuration);

        services.AddHttpClient();
        services.AddTransient<ITmdbApiService, TmdbApiService>();
    });

using var host = builder.Build();
await host.StartAsync();

WatcherArguments watcherArgs;
try
{
    watcherArgs = new WatcherArguments(Environment.GetCommandLineArgs());
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return;
}

if (watcherArgs.ShowHelp)
{
    //write possible arguments
    Console.WriteLine($"{title} - A simple file system watcher.");
    Console.WriteLine("Options:");
    Console.WriteLine("  --watch or -w <folder_to_watch>   Specifies the folder to watch.");
    Console.WriteLine("  --help                      Displays this help message.");
    Console.WriteLine();
    Console.WriteLine("Usage: Watcher.Console.App --watch <folder_to_watch>");
    return;
}

Console.WriteLine();

Console.WriteLine($"Watching folder: {watcherArgs.FolderToWatch}");
Console.WriteLine("Press 'q' to quit or Ctrl+C to exit.");
Console.WriteLine();

// Resolve Rebus bus through DI
var bus = host.Services.GetRequiredService<Rebus.Bus.IBus>();
await bus.Subscribe<FileFoundEvent>();

// --- Define a lightweight publish DTO that implements your abstraction -------
var defaultCorrelationId = Guid.NewGuid().ToString("N");

using var watcher = new Watcher.Console.App.Services.Watcher(watcherArgs.FolderToWatch);
watcher.Changed += (s, e) =>
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {e.ChangeType}: {e.FullPath}");
watcher.Renamed += (s, e) =>
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Renamed: {e.OldFullPath} -> {e.FullPath}");
watcher.Error += (s, e) =>
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error: {e.GetException().Message}");
watcher.FileContentDiscovered += (s, e) =>
{
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] File Discovered: {e.Path} (Size: {e.Size} bytes)");

    // THIS IS OVER ENGINEERING !!
    var fileEvent = new FileFoundEvent
    {
        FilePaths = new List<string> { e.Path },
        MetaData = new MetaData(e.Name, e.Size, e.Extension),
        CausationId = defaultCorrelationId
    };

    bus.Publish(fileEvent);
};
   

watcher.PerformInitialScan();

watcher.Start();

var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nShutting down watcher...");
    cancellationTokenSource.Cancel();
};

// Check if running in interactive mode
if (Environment.UserInteractive && !Console.IsInputRedirected)
{
    Console.WriteLine("Press 'q' to quit or Ctrl+C to exit.");
    
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
                    Console.WriteLine("\nShutting down watcher...");
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
    Console.WriteLine("Running in non-interactive mode. Use Ctrl+C to exit.");
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

Console.WriteLine("Stopping watcher...");
watcher.Stop();

// Stop Rebus/Host
await host.StopAsync();


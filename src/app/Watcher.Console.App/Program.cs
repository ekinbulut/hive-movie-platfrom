// See https://aka.ms/new-console-template for more information

using System.Text.Json.Serialization.Metadata;
using Infrastructure.Messaging.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Watcher.Console.App.Events;
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
        services.AddMessaging(rabbitConn, inputQueue, 0);

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

    
    var fileEvent = new FileFoundEvent
    {
        FilePaths = new List<string> { e.Path },
        Meta = JsonConvert.SerializeObject(new { Size = e.Size, Name = e.Name, Extension = e.Extension  }),
        CausationId = defaultCorrelationId
    };

    bus.Publish(fileEvent);
};
   

watcher.PerformInitialScan();

watcher.Start();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nShutting down watcher...");
    watcher.Stop();
    Environment.Exit(0);
};

while (true)
{
    var key = Console.ReadKey(true);
    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
    {
        Console.WriteLine("\nShutting down watcher...");
        watcher.Stop();
        break;
    }
}

// Stop Rebus/Host when loop exits
await host.StopAsync();


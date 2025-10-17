// See https://aka.ms/new-console-template for more information

/*
 * Date: 2025-09-24
 * Description: A simple console app that watches a folder for new files and publishes events to RabbitMQ using Rebus.
 *
 * Written by: ChatGPT-4.0 Copilot
 * Reviewed by: Ekin BULUT
 */

using Domain.Events;
using Domain.Interfaces;
using Infrastructure.Database.Extensions;
using Infrastructure.Integration.Services;
using Infrastructure.Integration.Services.JellyFin;
using Infrastructure.Messaging.Extensions;
using Infrastructure.Messaging.Handlers;
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
        
        var inputQueue = "hive-watcher";
        services.AutoRegisterHandlersFromAssemblyOf<MessageHandler>();
        services.AutoRegisterHandlersFromAssemblyOf<WatchPathChangedHandler>();
        

        // Configure Rebus with routing for multiple queues
        // services.AddMessaging(rabbitConn, inputQueue, workers: 2);
        
        services.AddMessagingWithRouting(rabbitConn, inputQueue, routing =>
            {
                routing.Map<WatchPathChangedEvent>(inputQueue);
            }, workers
        : 1);
        
        services.AddDbContext(ctx.Configuration);

        services.AddHttpClient();
        services.AddTransient<ITmdbApiService, TmdbApiService>();
        services.AddSingleton<IJellyFinServiceConfiguration, JellyFinServiceConfiguration>();
        services.AddScoped<IJellyFinService, JellyFinService>();
        // Register watcher services with proper buffer management
        services.AddTransient<IFileSystemWatcherFactory, FileSystemWatcherFactory>();
        services.AddTransient<IFileSystemService, FileSystemService>();
        services.AddTransient<IConsoleLogger, ConsoleLogger>();
        services.AddSingleton<IWatcherManager, WatcherManager>();

    });

using var host = builder.Build();

// Resolve Rebus bus through DI
var bus = host.Services.GetRequiredService<Rebus.Bus.IBus>();
await bus.Subscribe<FileFoundEvent>();
await bus.Subscribe<WatchPathChangedEvent>();

await host.StartAsync();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

logger.LogInformation($"{title} v{appVersion}");
logger.LogInformation("Press 'q' to quit or Ctrl+C to exit.");

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

// Stop Rebus/Host

await host.StopAsync();

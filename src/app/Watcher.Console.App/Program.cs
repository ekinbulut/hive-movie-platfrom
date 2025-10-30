// See https://aka.ms/new-console-template for more information

/*
 * Date: 2025-09-24
 * Description: A simple console app that watches a folder for new files and publishes events to RabbitMQ using Rebus.
 *
 * Written by: ChatGPT-4.0 Copilot
 * Reviewed by: Ekin BULUT
 */

using base_transport;
using Domain.Events;
using Infrastructure.Database.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Watcher.Console.App;
using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Factories;
using Watcher.Console.App.Handlers;
using Watcher.Console.App.Services;

string title = "Hive Folder Watcher";
Console.Title = title;

//write the app version and name
var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

// --- Host/DI bootstrap -------------------------------------------------------
var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) => { config.AddEnvironmentVariables(); }) // Add environment variables
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
        services.AddTransportationLayer(ctx.Configuration);

        services.AddDbContext(ctx.Configuration);

        services.AddHttpClient();

        services.AddTransient<IFileSystemWatcherFactory, FileSystemWatcherFactory>();
        services.AddTransient<IFileSystemService, FileSystemService>();
        services.AddTransient<IConsoleLogger, ConsoleLogger>();
        services.AddSingleton<IWatcherManager, WatcherManager>();
        services.AddSingleton<IStartupService, StartupService>();

    });

using var host = builder.Build();

await host.StartAsync();

var cancellationTokenSource = new CancellationTokenSource();

await MessageHandlerExecutor.StartHandlerAsync<WatchPathChangedEvent>(host.Services, "config.changed",cancellationTokenSource.Token);

var logger = host.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation($"{title} v{appVersion}");

// Initialize watchers for existing users
var startupService = host.Services.GetRequiredService<IStartupService>();

logger.LogInformation("=== Environment Variables ===");

PrintEnvironmentVariables(logger,"RabbitMQ__HostName");
PrintEnvironmentVariables(logger,"JELLYFIN_BASE_URL");

static void PrintEnvironmentVariables(ILogger logger, params string[] variableNames)
{
    
    foreach (var name in variableNames)
    {
        var value = Environment.GetEnvironmentVariable(name);
        logger.LogInformation($"{name} = {value ?? "(not set)"}");
    }
    
}
    logger.LogInformation("============================");


logger.LogInformation("Press 'q' to quit or Ctrl+C to exit.");

await startupService.InitializeWatchersAsync(cancellationTokenSource.Token);

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

await startupService.StopAllWatchersAsync();

await host.StopAsync();
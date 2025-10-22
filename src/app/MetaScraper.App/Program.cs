using base_transport;
using Domain.Interfaces;
using Infrastructure.Database.Extensions;
using Infrastructure.Integration.Services;
using Infrastructure.Integration.Services.JellyFin;
using MetaScraper.App.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


namespace MetaScraper.App;

class Program
{
    static async Task Main(string[] args)
    {
        string title = "MetaScraper Service";
        Console.Title = title;

        //write the app version and name
        var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) => { config.AddEnvironmentVariables(); })
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
                services.AddTransient<ITmdbApiService, TmdbApiService>();
                services.AddSingleton<IJellyFinServiceConfiguration, JellyFinServiceConfiguration>();
                services.AddScoped<IJellyFinService, JellyFinService>();

                services.AddSingleton<MessageHandler>();
            });

        using var host = builder.Build();

        await host.StartAsync();
        var cancellationTokenSource = new CancellationTokenSource();

        var handler = host.Services.GetRequiredService<MessageHandler>();
        await handler.StartListeningAsync(cancellationTokenSource.Token);

        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        logger.LogInformation($"{title} v{appVersion}");
        logger.LogInformation("Press 'q' to quit or Ctrl+C to exit.");


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
    }
}
using base_transport;
using Domain.Abstraction;
using Domain.Events;
using Domain.Interfaces;
using Infrastructure.Database.Extensions;
using Infrastructure.Integration.Services;
using Infrastructure.Integration.Services.JellyFin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Factories;
using Watcher.Console.App.Services;

namespace Watcher.Console.App;

class App : BaseApp
{
    public App(string title) : base(title)
    {
        Title = title;
    }

    public App()
    {
    }

    protected override string Title { get; set; } = "Test App";

    protected override Task<IHostBuilder> GetHostBuilderAsync(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
            }) // Add environment variables
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

        return Task.FromResult(builder);
    }

    public override Task<IHost> StartAsync(string[] args, CancellationTokenSource cancellationTokenSource)
    {
        return GetHostBuilderAsync(args).ContinueWith(async builderTask =>
        {
            System.Console.Title = Title;

            var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var host = builderTask.Result.Build();
            await host.StartAsync(cancellationTokenSource.Token);

            await MessageHandlerExecutor.StartHandlerAsync<WatchPathChangedEvent>(host.Services, "config.changed",
                cancellationTokenSource.Token);



            var logger = host.Services.GetRequiredService<ILogger<Program>>();


            logger.LogInformation($"{Title} v{appVersion}");
            logger.LogInformation("=== Environment Variables ===");
            PrintEnvironmentVariables(logger, "RabbitMQ__HostName");
            static void PrintEnvironmentVariables(ILogger logger, params string[] variableNames)
            {
                foreach (var name in variableNames)
                {
                    var value = Environment.GetEnvironmentVariable(name);
                    logger.LogInformation($"{name} = {value ?? "(not set)"}");
                }
            }
            logger.LogInformation("============================");

            var startupService = host.Services.GetRequiredService<IStartupService>();
            await startupService.InitializeWatchersAsync(cancellationTokenSource.Token);

            return host;
        }, cancellationTokenSource.Token).Unwrap();
    }

    public override async Task RunInteractiveLoop(IHost host, CancellationTokenSource cancellationTokenSource)
    {
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        System.Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            logger.LogWarning($"{Title} shutting down...");
            cancellationTokenSource.Cancel();
        };

        if (Environment.UserInteractive && !System.Console.IsInputRedirected)
        {
            logger.LogInformation("Press 'q' to quit or Ctrl+C to exit.");

            // Run the interactive loop in a background task
            _ = Task.Run(async () =>
            {
                try
                {
                    while (!cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        var key = System.Console.ReadKey(true);
                        if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                        {
                            logger.LogWarning($"{Title} shutting down...");
                            await cancellationTokenSource.CancelAsync();
                            break;
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    // Console input not available, ignore
                }
            }, cancellationTokenSource.Token);
        }
        else
        {
            logger.LogInformation("Running in non-interactive mode. Use Ctrl+C to exit.");
        }

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
            // Expected when cancellation is requested
        }

        await host.StopAsync();

        logger.LogWarning($"{Title} stopped.");
    }
}
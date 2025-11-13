// See https://aka.ms/new-console-template for more information

/*
 * Date: 2025-09-24
 * Description: A simple console app that watches a folder for new files and publishes events to RabbitMQ.
 *
 * Written by: ChatGPT-4.0 Copilot
 * Reviewed by: Ekin BULUT
 */

namespace Watcher.Console.App;

class Program
{
    static async Task Main(string[] args)
    {
        string title = "Hive Folder Watcher";

        var cancellationTokenSource = new CancellationTokenSource();

        var app = AppFactory.CreateApp(title);
        var host = await app.StartAsync(args, cancellationTokenSource);
        await app.RunInteractiveLoop(host, cancellationTokenSource);
    }
}
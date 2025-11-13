using System.Diagnostics;
using MetaScraper.App.Handlers;


namespace MetaScraper.App;

class Program
{
    static async Task Main(string[] args)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        
        var app = AppFactory.CreateApp("MetaScraper Service");
        var host = await app.StartAsync(args, cancellationTokenSource);
        await app.RunInteractiveLoop(host, cancellationTokenSource);
    }
}
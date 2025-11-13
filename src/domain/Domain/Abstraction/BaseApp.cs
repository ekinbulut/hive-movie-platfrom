using Microsoft.Extensions.Hosting;

namespace Domain.Abstraction;

public abstract class BaseApp
{
    protected abstract string Title { get; set; }

    protected BaseApp()
    {
        
    }
    protected BaseApp(string title)
    {
        Title = title;
    }
    
    protected abstract Task<IHostBuilder> GetHostBuilderAsync(string[] args);
    public abstract Task<IHost> StartAsync(string[] args, CancellationTokenSource cancellationTokenSource);
    public abstract Task RunInteractiveLoop(IHost host, CancellationTokenSource cancellationTokenSource);
}
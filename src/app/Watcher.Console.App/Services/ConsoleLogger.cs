using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Services;

public class ConsoleLogger : IConsoleLogger
{
    public void WriteLine(string message)
    {
        System.Console.WriteLine(message);
    }

    public void Write(string message)
    {
        System.Console.Write(message);
    }
}
namespace Watcher.Console.App.Abstracts;

public interface IConsoleLogger
{
    void WriteLine(string message);
    void Write(string message);
}
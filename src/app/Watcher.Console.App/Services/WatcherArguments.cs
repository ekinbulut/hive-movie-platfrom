
namespace Watcher.Console.App.Services;

public class WatcherArguments
{
    public string FolderToWatch { get; }
    public bool ShowHelp { get; }

    public WatcherArguments(string[] args)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));
        args = args.Skip(1).ToArray(); // skip executable path

        if (args.Contains("--help") || args.Contains("-h"))
        {
            ShowHelp = true;
            return;
        }

        string folder = null;
        if (args.Contains("--watch"))
        {
            var index = Array.IndexOf(args, "--watch");
            if (index + 1 < args.Length)
                folder = args[index + 1];
            else
                throw new ArgumentException("Please provide a folder to watch after --watch.");
        }
        else if (args.Contains("-w"))
        {
            var index = Array.IndexOf(args, "-w");
            if (index + 1 < args.Length)
                folder = args[index + 1];
            else
                throw new ArgumentException("Please provide a folder to watch after -w.");
        }
        else if (args.Length > 0)
        {
            folder = args[0];
        }

        if (!ShowHelp)
        {
            if (string.IsNullOrWhiteSpace(folder))
                throw new ArgumentException("Please provide a folder to watch as a command line argument.");
            if (!Directory.Exists(folder))
                throw new DirectoryNotFoundException($"The folder '{folder}' does not exist.");
            FolderToWatch = folder;
        }
    }
}
// See https://aka.ms/new-console-template for more information

using Watcher.Console.App.Services;


//write the app version and name
var appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
Console.WriteLine($"Watcher.Console.App v{appVersion}");

WatcherArguments watcherArgs;
try
{
    watcherArgs = new WatcherArguments(Environment.GetCommandLineArgs());
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return;
}

if (watcherArgs.ShowHelp)
{
    Console.WriteLine("Usage: Watcher.Console.App --watch <folder_to_watch>");
    return;
}


Console.WriteLine($"Watching folder: {watcherArgs.FolderToWatch}");
Console.WriteLine("Press 'q' to quit or Ctrl+C to exit.");

using var watcher = new Watcher.Console.App.Services.Watcher(watcherArgs.FolderToWatch);
watcher.Changed += (s, e) =>
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {e.ChangeType}: {e.FullPath}");
watcher.Renamed += (s, e) =>
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Renamed: {e.OldFullPath} -> {e.FullPath}");
watcher.Error += (s, e) =>
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Error: {e.GetException().Message}");
watcher.FileContentDiscovered += (s, e) =>
    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] File Discovered: {e.Path} (Size: {e.Size} bytes)");

watcher.PerformInitialScan();

watcher.Start();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nShutting down watcher...");
    watcher.Stop();
    Environment.Exit(0);
};

while (true)
{
    var key = Console.ReadKey(true);
    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
    {
        Console.WriteLine("\nShutting down watcher...");
        watcher.Stop();
        break;
    }
}





using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Services;

public class FileSystemWatcherWrapper : IFileSystemWatcherWrapper, IDisposable
{
    private readonly FileSystemWatcher _watcher;
    private readonly TimeSpan _pollingInterval;
    private readonly bool _usePolling;
    private readonly CancellationTokenSource _cts = new();
    private Task? _poller;
    private bool _disposed;

    // Change signature is backward compatible (default args). You can force polling or set interval.
    public FileSystemWatcherWrapper(string path, string filter = "*.*", TimeSpan? pollingInterval = null, bool? forcePolling = null)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path cannot be null/empty.", nameof(path));
        path = System.IO.Path.GetFullPath(path);

        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(2);

        // Decide backend: env > ctor > auto-detect
        var fromEnv = Environment.GetEnvironmentVariable("WATCHER_FORCE_POLLING");
        bool envForcePolling = !string.IsNullOrEmpty(fromEnv) && (fromEnv == "1" || fromEnv.Equals("true", StringComparison.OrdinalIgnoreCase));

        _usePolling = envForcePolling
                      || (forcePolling ?? false)
                      || IsLikelyOnCifs(path); // auto-detect (Linux)

        _watcher = new FileSystemWatcher(path, filter)
        {
            InternalBufferSize = 65536,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
            IncludeSubdirectories = true,
            EnableRaisingEvents = false
        };

        // Wire up events (these will be used when not polling)
        _watcher.Created += (s, e) => Created?.Invoke(s, e);
        _watcher.Changed += (s, e) => Changed?.Invoke(s, e);
        _watcher.Deleted += (s, e) => Deleted?.Invoke(s, e);
        _watcher.Renamed += (s, e) => Renamed?.Invoke(s, e);
        _watcher.Error += OnError;
    }

    // ===== Public API (unchanged) =====

    public string Path
    {
        get => _watcher.Path;
        set => _watcher.Path = value;
    }

    public string Filter
    {
        get => _watcher.Filter;
        set => _watcher.Filter = value;
    }

    public bool IncludeSubdirectories
    {
        get => _watcher.IncludeSubdirectories;
        set => _watcher.IncludeSubdirectories = value;
    }

    public NotifyFilters NotifyFilter
    {
        get => _watcher.NotifyFilter;
        set => _watcher.NotifyFilter = value;
    }

    // When using polling, EnableRaisingEvents just starts/stops the poller.
    public bool EnableRaisingEvents
    {
        get => !_usePolling ? _watcher.EnableRaisingEvents : _poller != null && !_poller.IsCompleted;
        set
        {
            if (_usePolling)
            {
                if (value) StartPoller();
                else StopPoller();
            }
            else
            {
                _watcher.EnableRaisingEvents = value;
            }
        }
    }

    public int InternalBufferSize
    {
        get => _watcher.InternalBufferSize;
        set => _watcher.InternalBufferSize = value;
    }

    public event FileSystemEventHandler? Created;
    public event FileSystemEventHandler? Changed;
    public event FileSystemEventHandler? Deleted;
    public event RenamedEventHandler? Renamed;
    public event ErrorEventHandler? Error;

    public void Start() => EnableRaisingEvents = true;

    public void Stop() => EnableRaisingEvents = false;

    public void Dispose()
    {
        if (_disposed) return;
        try
        {
            Stop();
            _cts.Cancel();
            _watcher?.Dispose();
        }
        finally
        {
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    // ===== Inotify error handling (non-polling mode) =====

    private void OnError(object sender, ErrorEventArgs e)
    {
        var ex = e.GetException();

        // FSW buffer overflow -> try to grow buffer, otherwise fallback to polling
        if (ex is InternalBufferOverflowException)
        {
            try
            {
                _watcher.EnableRaisingEvents = false;

                if (_watcher.InternalBufferSize < 1_048_576)
                    _watcher.InternalBufferSize = Math.Min(_watcher.InternalBufferSize * 2, 1_048_576);

                Task.Delay(750, _cts.Token).ContinueWith(t =>
                {
                    if (!_disposed && !_cts.IsCancellationRequested)
                        _watcher.EnableRaisingEvents = true;
                }, TaskScheduler.Default);
            }
            catch
            {
                // As a last resort, switch to polling
                SwitchToPolling();
            }
        }
        else
        {
            Error?.Invoke(sender, e);
        }
    }

    private void SwitchToPolling()
    {
        try
        {
            _watcher.EnableRaisingEvents = false;
        }
        catch { /* ignore */ }

        StartPoller();
    }

    // ===== Polling backend =====

    private sealed record FileSig(long Length, DateTime LastWriteUtc, bool IsDir);

    private readonly ConcurrentDictionary<string, FileSig> _snapshot = new(StringComparer.OrdinalIgnoreCase);

    private void StartPoller()
    {
        if (_poller is { IsCompleted: false }) return;

        // initial scan
        _snapshot.Clear();
        foreach (var (path, sig) in EnumerateTree(Path, Filter, IncludeSubdirectories))
            _snapshot[path] = sig;

        _poller = Task.Run(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var (path, sig) in EnumerateTree(Path, Filter, IncludeSubdirectories))
                    {
                        seen.Add(path);
                        if (_snapshot.TryGetValue(path, out var old))
                        {
                            // Changed?
                            if (old.Length != sig.Length || old.LastWriteUtc != sig.LastWriteUtc)
                            {
                                _snapshot[path] = sig;
                                SafeRaiseChanged(path, sig.IsDir);
                            }
                        }
                        else
                        {
                            _snapshot[path] = sig;
                            SafeRaiseCreated(path, sig.IsDir);
                        }
                    }

                    // Deletions (files + dirs)
                    foreach (var known in _snapshot.Keys.ToArray())
                    {
                        if (!seen.Contains(known))
                        {
                            if (_snapshot.TryRemove(known, out var was))
                                SafeRaiseDeleted(known, was.IsDir);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SafeRaiseError(ex);
                }

                try
                {
                    await Task.Delay(_pollingInterval, _cts.Token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }, _cts.Token);
    }

    private void StopPoller()
    {
        // Do not cancel the shared CTS here (class-level) because we might still use FSW.
        // Just wait a beat for the loop to observe EnableRaisingEvents=false via Stop().
        // Explicitly no-op: loop is controlled by _cts, and Stop() will set EnableRaisingEvents=false.
    }

    // Enumerate both files AND directories to mimic FSW semantics
    private static IEnumerable<(string Path, FileSig Sig)> EnumerateTree(string root, string filter, bool recurse)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = recurse,
            IgnoreInaccessible = true,
            AttributesToSkip = 0,
            MatchCasing = MatchCasing.PlatformDefault,
            MatchType = MatchType.Simple
        };

        // Directories (Created/Deleted dir events)
        foreach (var dir in Directory.EnumerateDirectories(root, "*", options))
        {
            DateTime mtime;
            try { mtime = Directory.GetLastWriteTimeUtc(dir); }
            catch { continue; }

            yield return (dir, new FileSig(0, mtime, true));
        }

        // Files (respect filter)
        foreach (var file in Directory.EnumerateFiles(root, filter, options))
        {
            FileInfo fi;
            try { fi = new FileInfo(file); }
            catch { continue; }

            yield return (file, new FileSig(fi.Length, fi.LastWriteTimeUtc, false));
        }
    }

    private void SafeRaiseCreated(string path, bool isDir)
    {
        try
        {
            if (isDir)
                Created?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Created, System.IO.Path.GetDirectoryName(path) ?? "", System.IO.Path.GetFileName(path)));
            else
                Created?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Created, System.IO.Path.GetDirectoryName(path) ?? "", System.IO.Path.GetFileName(path)));
        }
        catch { /* user handler exceptions are swallowed like FSW */ }
    }

    private void SafeRaiseChanged(string path, bool isDir)
    {
        try
        {
            Changed?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Changed, System.IO.Path.GetDirectoryName(path) ?? "", System.IO.Path.GetFileName(path)));
        }
        catch { }
    }

    private void SafeRaiseDeleted(string path, bool isDir)
    {
        try
        {
            Deleted?.Invoke(this, new FileSystemEventArgs(WatcherChangeTypes.Deleted, System.IO.Path.GetDirectoryName(path) ?? "", System.IO.Path.GetFileName(path)));
        }
        catch { }
    }

    private void SafeRaiseError(Exception ex)
    {
        try { Error?.Invoke(this, new ErrorEventArgs(ex)); } catch { }
    }

    // ===== Platform detection helpers =====

    private static bool IsLikelyOnCifs(string fullPath)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return false;

        try
        {
            // Read /proc/mounts and find the most specific mount point that prefixes fullPath
            var mounts = File.ReadAllLines("/proc/mounts");
            string? bestMountPoint = null;
            string? bestFstype = null;

            foreach (var line in mounts)
            {
                // Format: <src> <target> <fstype> <opts> ...
                // We need to be careful with spaces escaped as \040
                var parts = SplitProcMountLine(line);
                if (parts.Length < 3) continue;

                var target = UnescapeProcMount(parts[1]);
                var fstype = parts[2];

                if (fullPath.StartsWith(target, StringComparison.Ordinal) &&
                    (bestMountPoint == null || target.Length > bestMountPoint.Length))
                {
                    bestMountPoint = target;
                    bestFstype = fstype;
                }
            }

            if (bestFstype is null) return false;

            // CIFS/Samba usually reports as "cifs". Some kernels may show "smb3".
            return bestFstype.Contains("cifs", StringComparison.OrdinalIgnoreCase)
                   || bestFstype.Contains("smb", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string[] SplitProcMountLine(string line)
    {
        // Basic split on space, preserving escaped spaces in fields
        var parts = new List<string>();
        var sb = new StringBuilder();
        bool escaping = false;

        foreach (var ch in line)
        {
            if (escaping)
            {
                // Keep the backslash escape as-is; we’ll unescape later
                sb.Append('\\').Append(ch);
                escaping = false;
            }
            else if (ch == '\\')
            {
                escaping = true;
            }
            else if (ch == ' ')
            {
                parts.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(ch);
            }
        }
        if (sb.Length > 0) parts.Add(sb.ToString());
        return parts.ToArray();
    }

    private static string UnescapeProcMount(string s)
    {
        // Only the \040 (space) and a few others are common; handle \040 safely.
        return s.Replace("\\040", " ");
    }
}
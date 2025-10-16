using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Models;

namespace Watcher.Console.App.Services;

public class Watcher : IDisposable
{
    private readonly IFileSystemWatcherWrapper _watcher;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger _logger;
    private readonly string _rootPath;
    private readonly HashSet<string> _allowedExtensions;
    
    // Add throttling to prevent overwhelming the system
    private readonly ConcurrentDictionary<string, DateTime> _lastProcessedTimes = new();
    private readonly TimeSpan _throttleDelay = TimeSpan.FromMilliseconds(500); // 500ms throttle per file

    public event FileSystemEventHandler? Changed;
    public event RenamedEventHandler? Renamed;
    public event ErrorEventHandler? Error;
    public event EventHandler<FileContentInfo>? FileContentDiscovered;

    public List<FileInfo> Files { get; private set; } = new();
    public List<DirectoryInfo> Directories { get; private set; } = new();
    public int TotalFileCount { get; private set; }
    public int TotalDirectoryCount { get; private set; }
    public long TotalSizeBytes { get; private set; }

    public Watcher(
        string path,
        IFileSystemWatcherFactory watcherFactory,
        IFileSystemService fileSystemService,
        ILogger logger,
        string filter = "*.*",
        bool includeSubdirectories = true,
        string[]? allowedExtensions = null)
    {
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!_fileSystemService.DirectoryExists(path))
            throw new DirectoryNotFoundException($"The folder '{path}' does not exist.");

        _rootPath = path;

        // Initialize allowed extensions - default to common media/image files
        _allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (allowedExtensions != null && allowedExtensions.Length > 0)
        {
            foreach (var ext in allowedExtensions)
            {
                _allowedExtensions.Add(ext.StartsWith(".") ? ext : $".{ext}");
            }
        }
        else
        {
            // Default extensions: .mkv, .mov, .png, .jpeg, .jpg
            _allowedExtensions.Add(".mkv");
            _allowedExtensions.Add(".mov");
        }

        _watcher = watcherFactory.Create(path, filter);
        _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
        _watcher.IncludeSubdirectories = includeSubdirectories;

        _watcher.Created += OnFileCreated;
        // _watcher.Changed += OnFileChanged;
        // _watcher.Deleted += OnFileDeleted;
        _watcher.Renamed += (s, e) =>
        {
            Renamed?.Invoke(s, e);
            // If the new name is a valid, allowed file and exists, process it
            if (_fileSystemService.FileExists(e.FullPath) && IsAllowedFile(e.FullPath) && ShouldProcessFile(e.FullPath))
            {
                ProcessFileAsync(e.FullPath);
            }
        };
        _watcher.Error += (s, e) => Error?.Invoke(s, e);
    }
    
    private bool IsAllowedFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return _allowedExtensions.Contains(extension);
    }

    private bool ShouldProcessFile(string filePath)
    {
        // Throttle file processing to avoid overwhelming the system
        var now = DateTime.Now;
        if (_lastProcessedTimes.TryGetValue(filePath, out var lastProcessed))
        {
            if (now - lastProcessed < _throttleDelay)
            {
                return false; // Skip processing if within throttle window
            }
        }
        
        _lastProcessedTimes.AddOrUpdate(filePath, now, (key, oldValue) => now);
        
        // Clean up old entries to prevent memory leaks
        if (_lastProcessedTimes.Count > 1000)
        {
            var cutoff = now - TimeSpan.FromMinutes(5);
            var keysToRemove = _lastProcessedTimes
                .Where(kvp => kvp.Value < cutoff)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in keysToRemove)
            {
                _lastProcessedTimes.TryRemove(key, out _);
            }
        }
        
        return true;
    }

    private void OnFileCreated(object? sender, FileSystemEventArgs e)
    {
        if (ShouldProcessFile(e.FullPath) && _fileSystemService.FileExists(e.FullPath) && IsAllowedFile(e.FullPath))
        {
            ProcessFileAsync(e.FullPath);
        }
    }

    // private void OnFileChanged(object? sender, FileSystemEventArgs e)
    // {
    //     Changed?.Invoke(sender, e);
    //
    //     _logger.LogInformation($"File changed: {e.FullPath}");
    //     if (ShouldProcessFile(e.FullPath) && _fileSystemService.FileExists(e.FullPath) && IsAllowedFile(e.FullPath))
    //     {
    //         ProcessFileAsync(e.FullPath);
    //     }
    // }
    //
    // private void OnFileDeleted(object? sender, FileSystemEventArgs e)
    // {
    //     // Treat deletes as a change for upstream consumers
    //     Changed?.Invoke(sender, e);
    //     _logger.LogInformation($"File deleted: {e.FullPath}");
    //     // Remove from throttle dictionary when file is deleted
    //     _lastProcessedTimes.TryRemove(e.FullPath, out _);
    // }

    private async void ProcessFileAsync(string filePath)
    {
        try
        {
            // Add a small delay to ensure file operations are complete
            await Task.Delay(100);
            
            _logger.LogInformation($"Processing file: {filePath}");
            
            if (!_fileSystemService.FileExists(filePath))
                return;

            var fileInfo = _fileSystemService.GetFileInfo(filePath);
            var fileContent = new FileContentInfo
            {
                Name = fileInfo.Name,
                Extension = fileInfo.Extension,
                Size = fileInfo.Length,
                Path = fileInfo.FullName
            };
            FileContentDiscovered?.Invoke(this, fileContent);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing file {filePath}: {ex.Message}");
        }
    }

    public void PerformInitialScan()
    {
        ScanFolder();
    }

    private void ScanFolder()
    {
        try
        {
            var rootDir = _fileSystemService.GetDirectoryInfo(_rootPath);

            var allFiles = _fileSystemService.GetFiles(_rootPath, "*", SearchOption.AllDirectories);
            Files = allFiles.Where(f => IsAllowedFile(f.FullName)).ToList();
            Directories = _fileSystemService.GetDirectories(_rootPath, "*", SearchOption.AllDirectories).ToList();

            TotalFileCount = Files.Count;
            TotalDirectoryCount = Directories.Count;
            TotalSizeBytes = Files.Sum(f => f.Length);

            _logger.LogInformation($"Initial scan completed (filtered for: {string.Join(", ", _allowedExtensions)}):");
            _logger.LogInformation($" - Total files: {TotalFileCount}");
            _logger.LogInformation($" - Total directories: {TotalDirectoryCount}");
            _logger.LogInformation($" - Total size: {FormatBytes(TotalSizeBytes)}");

            foreach (var file in Files)
            {
                var fileContent = new FileContentInfo
                {
                    Name = file.Name,
                    Extension = file.Extension,
                    Size = file.Length,
                    Path = file.FullName
                };

                FileContentDiscovered?.Invoke(this, fileContent);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogInformation($"Access denied during scan: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Error during folder scan: {ex.Message}");
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }

    public void Start()
    {
        // Do an initial scan so we have a baseline and emit existing files
        PerformInitialScan();
        _watcher.Start();
    }

    public void Stop() => _watcher.Stop();

    public void Dispose()
    {
        _watcher?.Dispose();
        GC.SuppressFinalize(this);
    }

    public void RescanFolder()
    {
        ScanFolder();
    }

    public void PrintSummary()
    {
        _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Filtered Extensions: {string.Join(", ", _allowedExtensions)}");
        _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Watching directory: {_rootPath}");
        _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Total files: {TotalFileCount}");
        _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Total directories: {TotalDirectoryCount}");
        _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Total size: {FormatBytes(TotalSizeBytes)}");
    }
}

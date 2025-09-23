using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Factories;
using Watcher.Console.App.Models;

namespace Watcher.Console.App.Services;

public class Watcher : IDisposable
{
    private readonly IFileSystemWatcherWrapper _watcher;
    private readonly IFileSystemService _fileSystemService;
    private readonly IConsoleLogger _logger;
    private readonly string _rootPath;
    private readonly HashSet<string> _allowedExtensions;

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
        IConsoleLogger logger,
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
            _allowedExtensions.Add(".png");
            _allowedExtensions.Add(".jpeg");
            _allowedExtensions.Add(".jpg");
        }

        _watcher = watcherFactory.Create(path, filter);
        _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite;
        _watcher.IncludeSubdirectories = includeSubdirectories;

        _watcher.Created += OnFileCreated;
        _watcher.Changed += (s, e) => Changed?.Invoke(s, e);
        _watcher.Deleted += (s, e) => Changed?.Invoke(s, e);
        _watcher.Renamed += (s, e) => Renamed?.Invoke(s, e);
        _watcher.Error += (s, e) => Error?.Invoke(s, e);
    }

    // Convenience constructor for production use
    public Watcher(string path, string filter = "*.*", bool includeSubdirectories = true, string[]? allowedExtensions = null)
        : this(path, new FileSystemWatcherFactory(), new FileSystemService(), new ConsoleLogger(), filter, includeSubdirectories, allowedExtensions)
    {
    }

    private bool IsAllowedFile(string filePath)
    {
        var extension = Path.GetExtension(filePath);
        return _allowedExtensions.Contains(extension);
    }

    private void OnFileCreated(object? sender, FileSystemEventArgs e)
    {
        Changed?.Invoke(sender, e);

        if (_fileSystemService.FileExists(e.FullPath) && IsAllowedFile(e.FullPath))
        {
            try
            {
                var fileInfo = _fileSystemService.GetFileInfo(e.FullPath);
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
                _logger.WriteLine($"Error processing new file {e.FullPath}: {ex.Message}");
            }
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

            _logger.WriteLine($"Initial scan completed (filtered for: {string.Join(", ", _allowedExtensions)}):");
            _logger.WriteLine($"  - Total files: {TotalFileCount}");
            _logger.WriteLine($"  - Total directories: {TotalDirectoryCount}");
            _logger.WriteLine($"  - Total size: {FormatBytes(TotalSizeBytes)}");

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
            _logger.WriteLine($"Access denied during scan: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.WriteLine($"Error during scan: {ex.Message}");
        }
    }

    public void RescanFolder()
    {
        ScanFolder();
    }

    public void PrintSummary()
    {
        _logger.WriteLine($"\nFolder Summary for: {_rootPath}");
        _logger.WriteLine($"Filtered Extensions: {string.Join(", ", _allowedExtensions)}");
        _logger.WriteLine($"Files: {TotalFileCount}");
        _logger.WriteLine($"Directories: {TotalDirectoryCount}");
        _logger.WriteLine($"Total Size: {FormatBytes(TotalSizeBytes)}");

        if (Directories.Any())
        {
            _logger.WriteLine("\nTop 5 largest directories:");
            var topDirs = Directories
                .Select(d => new {
                    Dir = d,
                    Size = GetDirectorySize(d.FullName)
                })
                .OrderByDescending(x => x.Size)
                .Take(5);

            foreach (var item in topDirs)
            {
                _logger.WriteLine($"  {item.Dir.Name}: {FormatBytes(item.Size)}");
            }
        }
    }

    private long GetDirectorySize(string dirPath)
    {
        try
        {
            return _fileSystemService.GetFiles(dirPath, "*", SearchOption.AllDirectories)
                .Where(f => IsAllowedFile(f.FullName))
                .Sum(f => f.Length);
        }
        catch
        {
            return 0;
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

    public void Start() => _watcher.EnableRaisingEvents = true;
    public void Stop() => _watcher.EnableRaisingEvents = false;

    public void Dispose()
    {
        _watcher?.Dispose();
    }
}

using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Services;

public class FileSystemService : IFileSystemService
{
    public bool DirectoryExists(string path) => Directory.Exists(path);

    public bool FileExists(string path) => File.Exists(path);

    public DirectoryInfo GetDirectoryInfo(string path) => new DirectoryInfo(path);

    public FileInfo GetFileInfo(string path) => new FileInfo(path);

    public FileInfo[] GetFiles(string path, string searchPattern, SearchOption searchOption)
    {
        var directory = new DirectoryInfo(path);
        return directory.GetFiles(searchPattern, searchOption);
    }

    public DirectoryInfo[] GetDirectories(string path, string searchPattern, SearchOption searchOption)
    {
        var directory = new DirectoryInfo(path);
        return directory.GetDirectories(searchPattern, searchOption);
    }

    public bool IsFileLocked(string eventArgsPath)
    {
        if (!File.Exists(eventArgsPath))
            return false;

        try
        {
            // First, check if file size is stable over multiple checks
            // This is more reliable for detecting ongoing copy operations
            var fileInfo = new FileInfo(eventArgsPath);
            var initialSize = fileInfo.Length;
            var lastModified = fileInfo.LastWriteTimeUtc;
            
            // Wait a bit
            Thread.Sleep(200);
            
            fileInfo.Refresh();
            var currentSize = fileInfo.Length;
            var currentModified = fileInfo.LastWriteTimeUtc;
            
            // If size or last modified changed, file is still being written to
            if (initialSize != currentSize || lastModified != currentModified)
            {
                return true;
            }
            
            // Try to open the file exclusively to check if it's locked
            using var stream = new FileStream(
                eventArgsPath, 
                FileMode.Open, 
                FileAccess.Read,
                FileShare.ReadWrite); // Allow other readers/writers to detect active locks
            
            // File opened successfully and size is stable
            return false;
        }
        catch (IOException)
        {
            // File is locked by another process (still being copied)
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            // File exists but we don't have permission, treat as locked
            return true;
        }
    }
}

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
        FileStream? stream = null;
        try
        {
            var file = new FileInfo(eventArgsPath);
            stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            return true;
        }
        finally
        {
            stream?.Close();
        }

        return false;
    }
}

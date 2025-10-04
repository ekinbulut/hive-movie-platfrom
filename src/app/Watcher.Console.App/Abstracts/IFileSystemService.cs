using System.IO;

namespace Watcher.Console.App.Abstracts;

public interface IFileSystemService
{
    bool DirectoryExists(string path);
    bool FileExists(string path);
    DirectoryInfo GetDirectoryInfo(string path);
    FileInfo GetFileInfo(string path);
    FileInfo[] GetFiles(string path, string searchPattern, SearchOption searchOption);
    DirectoryInfo[] GetDirectories(string path, string searchPattern, SearchOption searchOption);
}
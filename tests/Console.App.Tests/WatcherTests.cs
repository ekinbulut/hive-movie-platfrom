using Moq;
using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Models;

namespace Console.App.Tests;

public class WatcherTests
{
    [Fact]
    public void Constructor_ThrowsDirectoryNotFoundException_WhenDirectoryDoesNotExist()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(false);

        Assert.Throws<DirectoryNotFoundException>(() => 
            new Watcher.Console.App.Services.Watcher("/nonexistent", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object));
    }

    [Fact]
    public void Constructor_UsesDefaultExtensions_WhenNoExtensionsProvided()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);

        Assert.NotNull(watcher);
    }

    [Fact]
    public void PerformInitialScan_FiresFileContentDiscoveredEvent_ForAllowedFiles()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        var testFiles = new[]
        {
            new FileInfo("/test/video.mkv"),
            new FileInfo("/test/image.png"),
            new FileInfo("/test/document.txt")
        };

        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystemService.Setup(x => x.GetDirectoryInfo(It.IsAny<string>())).Returns(new DirectoryInfo("/test"));
        mockFileSystemService.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(testFiles);
        mockFileSystemService.Setup(x => x.GetDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new DirectoryInfo[0]);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);
        var discoveredFiles = new List<FileContentInfo>();
        watcher.FileContentDiscovered += (s, e) => discoveredFiles.Add(e);

        watcher.PerformInitialScan();

        Assert.Equal(2, discoveredFiles.Count);
        Assert.Contains(discoveredFiles, f => f.Name == "video.mkv");
        Assert.Contains(discoveredFiles, f => f.Name == "image.png");
        Assert.DoesNotContain(discoveredFiles, f => f.Name == "document.txt");
    }

    [Fact]
    public void PerformInitialScan_FiltersFilesByCustomExtensions_WhenExtensionsProvided()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        var testFiles = new[]
        {
            new FileInfo("/test/video.mp4"),
            new FileInfo("/test/image.png"),
            new FileInfo("/test/document.txt")
        };

        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystemService.Setup(x => x.GetDirectoryInfo(It.IsAny<string>())).Returns(new DirectoryInfo("/test"));
        mockFileSystemService.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(testFiles);
        mockFileSystemService.Setup(x => x.GetDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new DirectoryInfo[0]);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object, allowedExtensions: new[] { ".mp4", ".txt" });
        var discoveredFiles = new List<FileContentInfo>();
        watcher.FileContentDiscovered += (s, e) => discoveredFiles.Add(e);

        watcher.PerformInitialScan();

        Assert.Equal(2, discoveredFiles.Count);
        Assert.Contains(discoveredFiles, f => f.Name == "video.mp4");
        Assert.Contains(discoveredFiles, f => f.Name == "document.txt");
        Assert.DoesNotContain(discoveredFiles, f => f.Name == "image.png");
    }

    [Fact]
    public void PerformInitialScan_HandlesUnauthorizedAccessException_Gracefully()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystemService.Setup(x => x.GetDirectoryInfo(It.IsAny<string>())).Returns(new DirectoryInfo("/test"));
        mockFileSystemService.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
            .Throws(new UnauthorizedAccessException("Access denied"));
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);

        watcher.PerformInitialScan();

        mockLogger.Verify(x => x.WriteLine(It.Is<string>(s => s.Contains("Access denied"))), Times.Once);
    }

    [Fact]
    public void OnFileCreated_FiresFileContentDiscoveredEvent_ForAllowedFile()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();

        var mockVideoFile = new Mock<FileInfo>("/test/video.mkv");
        mockVideoFile.Setup(f => f.Name).Returns("video.mkv");
        mockVideoFile.Setup(f => f.Extension).Returns(".mkv");
        mockVideoFile.Setup(f => f.FullName).Returns("/test/video.mkv");
        mockVideoFile.Setup(f => f.Length).Returns(1500);

        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystemService.Setup(x => x.FileExists("/test/video.mkv")).Returns(true);
        mockFileSystemService.Setup(x => x.GetFileInfo("/test/video.mkv")).Returns(mockVideoFile.Object);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);
        var discoveredFiles = new List<FileContentInfo>();
        watcher.FileContentDiscovered += (s, e) => discoveredFiles.Add(e);

        mockWatcherWrapper.Raise(x => x.Created += null, mockWatcherWrapper.Object, new FileSystemEventArgs(WatcherChangeTypes.Created, "/test", "video.mkv"));

        Assert.Single(discoveredFiles);
        Assert.Equal("video.mkv", discoveredFiles[0].Name);
        Assert.Equal(".mkv", discoveredFiles[0].Extension);
    }

    [Fact]
    public void OnFileCreated_DoesNotFireEvent_ForDisallowedFile()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystemService.Setup(x => x.FileExists("/test/document.txt")).Returns(true);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);
        var discoveredFiles = new List<FileContentInfo>();
        watcher.FileContentDiscovered += (s, e) => discoveredFiles.Add(e);

        mockWatcherWrapper.Raise(x => x.Created += null, mockWatcherWrapper.Object, new FileSystemEventArgs(WatcherChangeTypes.Created, "/test", "document.txt"));

        Assert.Empty(discoveredFiles);
    }

    [Fact]
    public void OnFileCreated_HandlesException_WhenProcessingFile()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystemService.Setup(x => x.FileExists("/test/video.mkv")).Returns(true);
        mockFileSystemService.Setup(x => x.GetFileInfo("/test/video.mkv")).Throws(new IOException("File locked"));
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);

        mockWatcherWrapper.Raise(x => x.Created += null, mockWatcherWrapper.Object, new FileSystemEventArgs(WatcherChangeTypes.Created, "/test", "video.mkv"));

        mockLogger.Verify(x => x.WriteLine(It.Is<string>(s => s.Contains("Error processing new file"))), Times.Once);
    }

    [Fact]
    public void Start_EnablesFileSystemWatcher()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);

        watcher.Start();

        mockWatcherWrapper.VerifySet(x => x.EnableRaisingEvents = true, Times.Once);
    }

    [Fact]
    public void Stop_DisablesFileSystemWatcher()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);

        watcher.Stop();

        mockWatcherWrapper.VerifySet(x => x.EnableRaisingEvents = false, Times.Once);
    }

    [Fact]
    public void Dispose_DisposesFileSystemWatcher()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);

        watcher.Dispose();

        mockWatcherWrapper.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void RescanFolder_UpdatesFileCountsAndSizes()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        var testFiles = new[]
        {
            new FileInfo("/test/video.mkv"),
            new FileInfo("/test/image.png") 
        };

        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystemService.Setup(x => x.GetDirectoryInfo(It.IsAny<string>())).Returns(new DirectoryInfo("/test"));
        mockFileSystemService.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(testFiles);
        mockFileSystemService.Setup(x => x.GetDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new DirectoryInfo[0]);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);

        watcher.RescanFolder();

        Assert.Equal(2, watcher.TotalFileCount);
        Assert.Equal(1500, watcher.TotalSizeBytes);
    }

    [Fact]
    public void PrintSummary_LogsFilteredExtensionsAndStats()
    {
        var mockFileSystemService = new Mock<IFileSystemService>();
        var mockWatcherFactory = new Mock<IFileSystemWatcherFactory>();
        var mockWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
        var mockLogger = new Mock<IConsoleLogger>();
        
        mockFileSystemService.Setup(x => x.DirectoryExists(It.IsAny<string>())).Returns(true);
        mockFileSystemService.Setup(x => x.GetDirectoryInfo(It.IsAny<string>())).Returns(new DirectoryInfo("/test"));
        mockFileSystemService.Setup(x => x.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new FileInfo[0]);
        mockFileSystemService.Setup(x => x.GetDirectories(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(new DirectoryInfo[0]);
        mockWatcherFactory.Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>())).Returns(mockWatcherWrapper.Object);

        var watcher = new Watcher.Console.App.Services.Watcher("/test", mockWatcherFactory.Object, mockFileSystemService.Object, mockLogger.Object);

        watcher.PrintSummary();

        mockLogger.Verify(x => x.WriteLine(It.Is<string>(s => s.Contains("Filtered Extensions:"))), Times.Once);
        mockLogger.Verify(x => x.WriteLine(It.Is<string>(s => s.Contains("/test"))), Times.Once);
    }
}
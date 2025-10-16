using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Rebus.Bus;
using Watcher.Console.App.Abstracts;
using Watcher.Console.App.Events;
using Watcher.Console.App.Models;

namespace Watcher.Console.App.Services;

public class WatcherManager : IWatcherManager, IDisposable
{
    private readonly ConcurrentDictionary<string, Watcher> _watchers = new();
    private readonly IFileSystemWatcherFactory _watcherFactory;
    private readonly IFileSystemService _fileSystemService;
    private readonly ILogger<WatcherManager> _logger;
    private readonly IBus _bus;
    private readonly string _defaultCorrelationId;

    public WatcherManager(
        IFileSystemWatcherFactory watcherFactory,
        IFileSystemService fileSystemService,
        ILogger<WatcherManager> logger,
        IBus bus)
    {
        _watcherFactory = watcherFactory;
        _fileSystemService = fileSystemService;
        _logger = logger;
        _bus = bus;
        _defaultCorrelationId = Guid.NewGuid().ToString("N");
    }

    public async Task ChangeWatchPathAsync(string newPath, Guid userId)
    {
        var id = userId.ToString();
        if (!_fileSystemService.DirectoryExists(newPath))
        {
            _logger.LogWarning("Path does not exist: {Path} for user {UserId}", newPath, id);
            throw new DirectoryNotFoundException($"The path '{newPath}' does not exist.");
        }

        // Stop existing watcher for this user if exists
        if (_watchers.TryGetValue(id, out var existingWatcher))
        {
            _logger.LogInformation("Stopping existing watcher for user {UserId}", id);
            existingWatcher.Stop();
            existingWatcher.Dispose();
            _watchers.TryRemove(id, out _);
        }

        // Start new watcher with the new path
        StartWatcher(newPath, id);

        _logger.LogInformation("Changed watch path to {Path} for user {UserId}", newPath, id);
        
        await Task.CompletedTask;
    }

    public void StartWatcher(string path, string userId)
    {
        if (_watchers.ContainsKey(userId))
        {
            _logger.LogWarning("Watcher already exists for user {UserId}", userId);
            return;
        }

        var watcher = new Watcher(
            path: path,
            watcherFactory: _watcherFactory,
            fileSystemService: _fileSystemService,
            logger: _logger,
            filter: "*.*",
            includeSubdirectories: true,
            allowedExtensions: new[] { ".mkv", ".mov", ".mp4" }
        );

        // Subscribe to file discovery events
        watcher.FileContentDiscovered += (sender, eventArgs) =>
        {
            _logger.LogInformation(
                "File Discovered for user {UserId}: {Path} (Size: {Size} bytes)",
                userId,
                eventArgs.Path,
                eventArgs.Size);

            var fileEvent = new FileFoundEvent
            {
                UserId = userId,
                FilePaths = new List<string> { eventArgs.Path },
                MetaData = new MetaData(eventArgs.Name, eventArgs.Size, eventArgs.Extension),
                CausationId = _defaultCorrelationId
            };

            _bus.Publish(fileEvent);
        };

        watcher.Start();
        _watchers.TryAdd(userId, watcher);

        _logger.LogInformation("Started watcher for user {UserId} on path {Path}", userId, path);
    }

    public void StopWatcher(string userId)
    {
        if (_watchers.TryRemove(userId, out var watcher))
        {
            watcher.Stop();
            watcher.Dispose();
            _logger.LogInformation("Stopped watcher for user {UserId}", userId);
        }
    }

    public void StopAllWatchers()
    {
        foreach (var kvp in _watchers)
        {
            kvp.Value.Stop();
            kvp.Value.Dispose();
        }
        _watchers.Clear();
        _logger.LogInformation("Stopped all watchers");
    }

    public void Dispose()
    {
        StopAllWatchers();
        GC.SuppressFinalize(this);
    }
}

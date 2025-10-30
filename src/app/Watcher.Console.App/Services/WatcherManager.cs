using System.Collections.Concurrent;
using base_transport;
using Domain.Events;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Watcher.Console.App.Abstracts;

namespace Watcher.Console.App.Services;

public class WatcherManager(
    IFileSystemWatcherFactory watcherFactory,
    IFileSystemService fileSystemService,
    ILogger<WatcherManager> logger,
    IBasicMessagingService basicMessagingService)
    : IWatcherManager, IDisposable
{
    private readonly ConcurrentDictionary<string, Watcher> _watchers = new();
    private readonly string _defaultCorrelationId = Guid.NewGuid().ToString("N");
    
    private const string Queue = "file.found";

    public async Task ChangeWatchPathAsync(string newPath, Guid userId)
    {
        var id = userId.ToString();
        if (!fileSystemService.DirectoryExists(newPath))
        {
            logger.LogWarning("Path does not exist: {Path} for user {UserId}", newPath, id);
            throw new DirectoryNotFoundException($"The path '{newPath}' does not exist.");
        }

        // Stop existing watcher for this user if exists
        if (_watchers.TryGetValue(id, out var existingWatcher))
        {
            logger.LogInformation("Stopping existing watcher for user {UserId}", id);
            existingWatcher.Stop();
            existingWatcher.Dispose();
            _watchers.TryRemove(id, out _);
        }

        // Start new watcher with the new path
        StartWatcher(newPath, id);

        logger.LogInformation("Changed watch path to {Path} for user {UserId}", newPath, id);
        
        await Task.CompletedTask;
    }

    public void StartWatcher(string path, string userId)
    {
        if (_watchers.ContainsKey(userId))
        {
            logger.LogWarning("Watcher already exists for user {UserId}", userId);
            return;
        }

        var watcher = new Watcher(
            path: path,
            watcherFactory: watcherFactory,
            fileSystemService: fileSystemService,
            logger: logger,
            filter: "*.*",
            includeSubdirectories: true,
            allowedExtensions: new[] { ".mkv", ".mov", ".mp4" }
        );

        // Subscribe to file discovery events
        watcher.FileContentDiscovered += (sender, eventArgs) =>
        {
            logger.LogInformation(
                "File Discovered for user {UserId}: {Path} (Size: {Size} bytes)",
                userId,
                eventArgs.Path,
                eventArgs.Size);

            // check if file is still being written to
            while (fileSystemService.IsFileLocked(eventArgs.Path))
            {
                logger.LogInformation("Waiting for file to be copied: {Path}", eventArgs.Path);
                Thread.Sleep(500);
            }
            
            var size= fileSystemService.GetFileInfo(eventArgs.Path).Length;

            var fileEvent = new FileFoundEvent
            {
                UserId = Guid.Parse(userId),
                FilePaths = new List<string> { eventArgs.Path },
                MetaData = new MetaData(eventArgs.Name, size, eventArgs.Extension),
                CausationId = _defaultCorrelationId
            };
            var @event = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(fileEvent);
            basicMessagingService.BasicPublishAsync(Queue, @event);
            
            logger.LogInformation("Published FileFoundEvent for user {UserId}: {Path}", userId, eventArgs.Name);
        };

        watcher.Start();
        _watchers.TryAdd(userId, watcher);

        logger.LogInformation("Started watcher for user {UserId} on path {Path}", userId, path);
    }

    public void StopWatcher(string userId)
    {
        if (_watchers.TryRemove(userId, out var watcher))
        {
            watcher.Stop();
            watcher.Dispose();
            logger.LogInformation("Stopped watcher for user {UserId}", userId);
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
        logger.LogInformation("Stopped all watchers");
    }

    public Task StopAllWatchersAsync()
    {
        StopAllWatchers();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        StopAllWatchers();
        GC.SuppressFinalize(this);
    }
}

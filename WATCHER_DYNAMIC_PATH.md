# Dynamic Path Configuration for Watcher.Console.App

## Overview

The Watcher.Console.App now supports dynamic path changes at runtime via RabbitMQ events. Users can change the watched directory without restarting the container.

## Architecture

```
API → Database → RabbitMQ → Watcher.Console.App
```

1. **API receives path change request** → saves to database → publishes `WatchPathChangedEvent` to RabbitMQ
2. **Watcher.Console.App** subscribes to the event → stops old watcher → starts new watcher on the new path

## Components Added

### 1. Event: `WatchPathChangedEvent`
```csharp
public class WatchPathChangedEvent : BaseEvent, IMessage
{
    public string UserId { get; set; }
    public string NewPath { get; set; }
    public string? CausationId { get; init; }
}
```

### 2. Handler: `WatchPathChangedHandler`
- Subscribes to `WatchPathChangedEvent` from RabbitMQ
- Maps host paths to container paths
- Calls `WatcherManager` to change the watch path

### 3. Service: `WatcherManager`
- Manages multiple watcher instances (one per user)
- Dynamically starts/stops watchers
- Handles file discovery events and publishes to RabbitMQ

### 4. Interface: `IWatcherManager`
```csharp
public interface IWatcherManager
{
    Task ChangeWatchPathAsync(string newPath, string userId);
    void StartWatcher(string path, string userId);
    void StopWatcher(string userId);
    void StopAllWatchers();
}
```

## Docker Configuration

The `docker-compose.yaml` has been updated:

**Before:**
```yaml
volumes:
  - "/home/user/shared/Plex/Shared Movies:/app/shared"
```

**After:**
```yaml
volumes:
  - "/home/user/shared:/mnt/host"
```

This change mounts the entire `/home/user/shared` directory, allowing the watcher to access any subdirectory dynamically.

## Usage

### From Your API

When a user changes their watch path via your API:

1. Save the new path to the database
2. Publish the event to RabbitMQ **topic** `hive-api.path-changed`:

```csharp
// Example API endpoint
[HttpPost("users/{userId}/watch-path")]
public async Task<IActionResult> ChangeWatchPath(string userId, [FromBody] ChangePathRequest request)
{
    // Save to database
    await _repository.UpdateUserWatchPath(userId, request.NewPath);
    
    // Publish event to RabbitMQ topic (not direct queue)
    var pathChangeEvent = new WatchPathChangedEvent
    {
        UserId = userId,
        NewPath = request.NewPath  // e.g., "Plex/Shared Movies" or "/mnt/host/Plex/Shared Movies"
    };
    
    // Publish to topic instead of queue
    await _bus.Advanced.Topics.Publish("hive-api.path-changed", pathChangeEvent);
    
    return Ok();
}
```

### Path Mapping

The handler automatically maps paths:
- If path starts with `/mnt/host` → uses as-is
- Otherwise → prepends `/mnt/host/` to the path

**Examples:**
- User input: `"Plex/Shared Movies"` → Container path: `"/mnt/host/Plex/Shared Movies"`
- User input: `"/mnt/host/Movies"` → Container path: `"/mnt/host/Movies"`

## RabbitMQ Queue

The watcher subscribes to:
- Queue: `hive-watcher`
- Events: `FileFoundEvent`, `WatchPathChangedEvent`

## Multi-User Support

The `WatcherManager` maintains a dictionary of watchers by userId:
- Each user can have their own watch path
- When a path change event is received, only that user's watcher is restarted
- No container restart required

## Testing

### 1. Publish a test event to RabbitMQ:

```csharp
var testEvent = new WatchPathChangedEvent
{
    UserId = "test-user-123",
    NewPath = "Plex/Shared Movies"
};

await bus.Publish(testEvent);
```

### 2. Check logs:

```bash
docker logs -f hive-watcher
```

You should see:
```
[2025-10-16 12:00:00] Received WatchPathChangedEvent for UserId: test-user-123, NewPath: Plex/Shared Movies
[2025-10-16 12:00:00] Stopping existing watcher for user test-user-123
[2025-10-16 12:00:00] Started watcher for user test-user-123 on path /mnt/host/Plex/Shared Movies
[2025-10-16 12:00:00] Successfully changed watch path to /mnt/host/Plex/Shared Movies for user test-user-123
```

## Important Notes

1. **Volume Mount**: Ensure `/home/user/shared` on the host contains all directories users might want to watch
2. **Permissions**: The container user must have read access to all subdirectories
3. **Path Validation**: The handler validates that the directory exists before starting the watcher
4. **Error Handling**: If the path doesn't exist, an error is logged and the event is rejected
5. **Rebus Integration**: The app uses Rebus for message handling, which is already configured in your infrastructure

## Environment Variables

Make sure these are set in `docker-compose.yaml`:

```yaml
environment:
  - RABBITMQ__CONNECTION=amqp://guest:guest@rabbitmq:5672
  - JELLYFIN_BASE_URL=http://192.168.1.112:8096
  - JELLYFIN_ACCESS_TOKEN=your_token_here
```

## Next Steps

1. **API Implementation**: Add an endpoint in your API to handle path change requests
2. **Database Schema**: Add a `watch_path` column to your users table
3. **Frontend**: Create a UI for users to change their watch path
4. **Validation**: Add path validation to ensure users can't access unauthorized directories

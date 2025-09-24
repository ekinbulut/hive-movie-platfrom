using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Messaging.Handlers;
using Newtonsoft.Json;
using Watcher.Console.App.Events;

namespace Watcher.Console.App.Handlers;


public class MessageHandler : BaseMessageHandler<FileFoundEvent>
{
    private IMovieRepository _movieRepository;
    
    public MessageHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }
    
    protected override Task OnHandle(FileFoundEvent message, string? causationId)
    {

        
        _movieRepository.AddMovie(new Movie()
        {
            FilePath = message.FilePaths.FirstOrDefault(),
            CreatedTime = DateTime.UtcNow,
            FileSize = message.MetaData.size,
            Name = message.MetaData.Name,
            IsActive = true,
            SubTitleFilePath = null,
            Image = null
        });
        
        return Task.CompletedTask;
    }
}
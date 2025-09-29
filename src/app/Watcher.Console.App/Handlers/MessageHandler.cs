using System.Text.RegularExpressions;
using Common.Crypto;
using Common.Parser;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Messaging.Handlers;
using Newtonsoft.Json;
using Watcher.Console.App.Events;

namespace Watcher.Console.App.Handlers;


public class MessageHandler(IMovieRepository movieRepository) : BaseMessageHandler<FileFoundEvent>
{
    protected override Task OnHandle(FileFoundEvent message, string? causationId)
    {
        var title = TitleParser.ExtractTitle(message.MetaData.Name);
        var hashValue = HashHelper.ComputeSha256Hash(title);
        var exists = movieRepository.GetMovieByHashValue(hashValue);

        if (!exists)
        {
            movieRepository.AddMovie(new Movie()
            {
                FilePath = message.FilePaths.FirstOrDefault(),
                CreatedTime = DateTime.UtcNow,
                FileSize = message.MetaData.size,
                Name = title,
                IsActive = true,
                SubTitleFilePath = null,
                Image = null,
                HashValue = hashValue
            });
        }
        
        return Task.CompletedTask;
    }
    
}
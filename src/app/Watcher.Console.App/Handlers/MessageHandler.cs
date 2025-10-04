using System.Text.RegularExpressions;
using Common.Crypto;
using Common.Parser;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Messaging.Handlers;
using Newtonsoft.Json;
using Watcher.Console.App.Events;

namespace Watcher.Console.App.Handlers;


public class MessageHandler(IMovieRepository movieRepository, ITmdbApiService tmdbApiService) : BaseMessageHandler<FileFoundEvent>
{
    protected override Task OnHandle(FileFoundEvent message, string? causationId)
    {
        var title = TitleParser.ExtractTitle(message.MetaData.Name);
        var hashValue = HashHelper.ComputeSha256Hash(title);
        var exists = movieRepository.GetMovieByHashValue(hashValue);

        if (!exists)
        {
            var year = TitleParser.ExtractYear(message.MetaData.Name) ?? 0;
        
            var result = tmdbApiService.SearchMoviesAsync(title, year).GetAwaiter().GetResult();
            var imagePath = result.Results.FirstOrDefault(x => 
                DateTime.TryParse(x.ReleaseDate, out var releaseDate) && 
                releaseDate.Year == year)?.PosterPath;
            
            movieRepository.AddMovie(new Movie()
            {
                FilePath = message.FilePaths.FirstOrDefault(),
                CreatedTime = DateTime.UtcNow,
                FileSize = message.MetaData.size,
                Name = title,
                IsActive = true,
                SubTitleFilePath = null,
                Image = imagePath,
                HashValue = hashValue,
                ReleaseDate = year 
            });
        }
        
        return Task.CompletedTask;
    }
    
}
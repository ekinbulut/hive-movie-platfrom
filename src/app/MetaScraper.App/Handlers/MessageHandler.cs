using base_transport;
using Common.Crypto;
using Common.Parser;
using Domain.Entities;
using Domain.Events;
using Domain.Interfaces;
using Infrastructure.Integration.Services.JellyFin;

namespace MetaScraper.App.Handlers;

[Obsolete("Use BaseMessageHandler<T> instead")]
public class MessageHandler(IMovieRepository movieRepository, 
    ITmdbApiService tmdbApiService,  IJellyFinService jellyFinService,
    IConfigurationRepository configurationRepository,
    IJellyFinServiceConfiguration jellyFinServiceConfiguration,
    IBasicMessagingService messagingService)
{
    
    private const string Queue = "file.found";
    
    public async Task StartListeningAsync(CancellationToken ct = default)
    {
        await messagingService.ConnectAsync(ct);

        messagingService.ReceivedAsync += async (sender, args) =>
        {
            var body = args.Body.ToArray();
            var messageString = System.Text.Encoding.UTF8.GetString(body);
            var watchPathChangedEvent = System.Text.Json.JsonSerializer.Deserialize<FileFoundEvent>(messageString);
            
            if (watchPathChangedEvent != null)
            {
                await HandleAsync(watchPathChangedEvent);
            }
        };

        await messagingService.BasicConsumeAsync(Queue, autoAck: true, ct);
    }

    private async Task HandleAsync(FileFoundEvent message)
    {
        //get configuration for user
        var config = await configurationRepository.GetConfigurationByUserIdAsync(message.UserId);
        if (config == null)
        {
            return;
        }

        //update jellyfin service configuration
        jellyFinServiceConfiguration.ApiKey = config.Settings.JellyFinApiKey;
        jellyFinServiceConfiguration.BaseUrl = config.Settings.JellyFinServer;
        jellyFinService.SetConfiguration(jellyFinServiceConfiguration);
        
        var title = TitleParser.ExtractTitle(message.MetaData.Name);
        var hashValue = HashHelper.ComputeSha256Hash(title);
        var exists = movieRepository.GetMovieByHashValue(hashValue);

        if (!exists)
        {
            var year = TitleParser.ExtractYear(message.MetaData.Name) ?? 0;
            
            // Get JellyFin movie ID
            var jellyFinMovieId = await jellyFinService.GetMovieIdByNameAsync(title, year > 0 ? year : null);
        
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
                ReleaseDate = year,
                JellyFinId = jellyFinMovieId,
                UserId = message.UserId
            });
        }
    }
    
}
using base_transport;
using Common.Crypto;
using Common.Parser;
using Domain.Entities;
using Domain.Events;
using Domain.Interfaces;
using Infrastructure.Integration.Services.JellyFin;
using Microsoft.Extensions.Logging;

namespace MetaScraper.App.Handlers;

public class FileFoundHandler(
    ILogger<FileFoundHandler> logger,
    IBasicMessagingService service,
    IMovieRepository movieRepositor,
    IJellyFinService jellyFinService,
    IJellyFinServiceConfiguration jellyFinServiceConfiguration,
    ITmdbApiService tmdbApiService,
    IConfigurationRepository configurationRepository,
    IBasicMessagingService messagingService)
    : BaseMessageHandler<FileFoundEvent>(service)
{
    public override async Task HandleAsync(FileFoundEvent message, ulong deliveryTag,
        CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
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
            var exists = movieRepositor.GetMovieByHashValue(hashValue);

            if (!exists)
            {
                var year = TitleParser.ExtractYear(message.MetaData.Name) ?? 0;

                // Get JellyFin movie ID
                var jellyFinMovieId = await jellyFinService.GetMovieIdByNameAsync(title, year > 0 ? year : null);

                var result = tmdbApiService.SearchMoviesAsync(title, year).GetAwaiter().GetResult();
                var imagePath = result.Results.FirstOrDefault(x =>
                    DateTime.TryParse(x.ReleaseDate, out var releaseDate) &&
                    releaseDate.Year == year)?.PosterPath;

                movieRepositor.AddMovie(new Movie()
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
        catch (Exception e)
        {
            logger.LogError(e.Message);
        }
        finally
        {
            await messagingService.AcknowledgeMessageAsync(deliveryTag, cancellationToken);
        }
    }
}
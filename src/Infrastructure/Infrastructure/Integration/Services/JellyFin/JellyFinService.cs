using System.Text.Json;
using System.Threading;
using Domain.Interfaces;
using Infrastructure.Integration.Services.JellyFin.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Integration.Services.JellyFin;

public class JellyFinService(IHttpClientFactory httpClientFactory, ILogger<JellyFinService> logger, string apiKey = "", string baseUrl = "")
    : IJellyFinService
{
    private static readonly SemaphoreSlim _throttle = new SemaphoreSlim(2); // Limit to 2 concurrent requests
    private static readonly TimeSpan _throttleDelay = TimeSpan.FromMilliseconds(500); // Optional delay between requests

    public async Task<string?> GetMovieIdByNameAsync(string movieName, int? year = null)
    {
        await _throttle.WaitAsync();
        try
        {
            await Task.Delay(_throttleDelay); // Throttle delay
            var httpClient = httpClientFactory.CreateClient();
            var url =
                $"{baseUrl}/Search/Hints?api_key={apiKey}&SearchTerm={Uri.EscapeDataString(movieName)}&IncludeItemTypes=Movie&Limit=10";

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonSerializer.Deserialize<JellyFinSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (searchResponse?.SearchHints?.Any() == true)
            {
                var movie = year.HasValue
                    ? searchResponse.SearchHints.FirstOrDefault(x => x.ProductionYear == year)
                    : searchResponse.SearchHints.FirstOrDefault();

                return movie?.Id;
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching movie ID from JellyFin for movie: {MovieName}", movieName);
            return null;
        }
        finally
        {
            _throttle.Release();
        }
    }
}
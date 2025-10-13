using System.Text.Json;
using Domain.Interfaces;
using Infrastructure.Integration.Services.JellyFin.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Integration.Services.JellyFin;

public class JellyFinService(HttpClient httpClient, IConfiguration configuration, ILogger<JellyFinService> logger)
    : IJellyFinService
{
    private readonly string _baseUrl = configuration["JellyFin:BaseUrl"]
                                       ?? Environment.GetEnvironmentVariable("JELLYFIN_BASE_URL")
                                       ?? throw new ArgumentNullException("JellyFin:BaseUrl not configured");

    private readonly string _apiKey = configuration["JellyFin:ApiKey"]
                                      ?? Environment.GetEnvironmentVariable("JELLYFIN_ACCESS_TOKEN")
                                      ?? throw new ArgumentNullException("JellyFin:ApiKey not configured");

    public async Task<string?> GetMovieIdByNameAsync(string movieName, int? year = null)
    {
        try
        {
            var url =
                $"{_baseUrl}/Search/Hints?api_key={_apiKey}&SearchTerm={Uri.EscapeDataString(movieName)}&IncludeItemTypes=Movie&Limit=10";

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
    }
}
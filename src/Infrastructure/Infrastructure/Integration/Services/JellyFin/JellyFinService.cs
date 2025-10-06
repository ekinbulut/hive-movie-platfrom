using System.Text.Json;
using Domain.Interfaces;
using Infrastructure.Integration.Services.JellyFin.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Integration.Services.JellyFin;

public class JellyFinService : IJellyFinService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JellyFinService> _logger;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public JellyFinService(HttpClient httpClient, IConfiguration configuration, ILogger<JellyFinService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _baseUrl = configuration["JellyFin:BaseUrl"] ?? throw new ArgumentNullException("JellyFin:BaseUrl not configured");
            _apiKey = configuration["JellyFin:ApiKey"] ?? throw new ArgumentNullException("JellyFin:ApiKey not configured");
        }

        public async Task<string?> GetMovieIdByNameAsync(string movieName, int? year = null)
        {
            try
            {
                var url = $"{_baseUrl}/Search/Hints?api_key={_apiKey}&SearchTerm={Uri.EscapeDataString(movieName)}&IncludeItemTypes=Movie&Limit=10";
                
                var response = await _httpClient.GetAsync(url);
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
                _logger.LogError(ex, "Error fetching movie ID from JellyFin for movie: {MovieName}", movieName);
                return null;
            }
        }
    }
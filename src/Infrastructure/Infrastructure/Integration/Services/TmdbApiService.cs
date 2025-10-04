using System.Net.Http.Headers;
using System.Text.Json;
using Domain.DTOs;
using Domain.Interfaces;
using Infrastructure.DTOs;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Integration.Services;

public class TmdbApiService : ITmdbApiService
{
    private readonly HttpClient _httpClient;

    public TmdbApiService(HttpClient httpClient, IConfiguration configuration)
    {
        var tmdbSection = configuration.GetSection("TmdbApi");
        if (tmdbSection == null)
        {
            throw new ArgumentNullException("TmdbApi configuration section is missing.");
        }
        
        var baseUrl = tmdbSection["BaseUrl"]; 
        
        _httpClient = httpClient;
        var bearerToken = configuration["TmdbApi:BearerToken"];
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<TmdbMovieDto> GetMovieByIdAsync(int movieId)
    {
        var response = await _httpClient.GetAsync($"movie/{movieId}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TmdbMovieDto>(json);
    }

    public async Task<TmdbSearchResultDto> SearchMoviesAsync(string query, int year = 0, int page = 1)
    {
        var queryParams = $"search/movie?query={Uri.EscapeDataString(query)}&include_adult=false&language=en-US&page={page}";
        
        if (year > 0)
        {
            queryParams += $"&year={year}";
        }

        var response = await _httpClient.GetAsync(queryParams);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TmdbSearchResultDto>(json);
    }
}

using System.Net.Http.Headers;
using System.Text.Json;
using Domain.DTOs;
using Domain.Interfaces;
using Infrastructure.DTOs;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Integration.Services;

public class TmdbApiService : ITmdbApiService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;
    
    private static readonly SemaphoreSlim _throttle = new SemaphoreSlim(2); // Limit to 2 concurrent requests
    private static readonly TimeSpan _throttleDelay = TimeSpan.FromMilliseconds(500); // Optional delay between requests

    public TmdbApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        var tmdbSection = configuration.GetSection("TmdbApi");
        if (tmdbSection == null)
        {
            throw new ArgumentNullException("TmdbApi configuration section is missing.");
        }
        
        var baseUrl = tmdbSection["BaseUrl"]; 
        
        _httpClientFactory = httpClientFactory;
        var bearerToken = configuration["TmdbApi:BearerToken"];
        _httpClient = _httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", bearerToken);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<TmdbMovieDto> GetMovieByIdAsync(int movieId)
    {
        await _throttle.WaitAsync();
        try
        {
            await Task.Delay(_throttleDelay); // Throttle delay
            
            var response = await _httpClient.GetAsync($"movie/{movieId}");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TmdbMovieDto>(json);
        }
        catch (Exception e)
        {
            throw new Exception($"Error fetching movie with ID {movieId} from TMDb", e);
        }
        finally
        {
            _throttle.Release();
        }
    }

    public async Task<TmdbSearchResultDto> SearchMoviesAsync(string query, int year = 0, int page = 1)
    {
        await _throttle.WaitAsync();

        try
        {
            await Task.Delay(_throttleDelay); // Throttle delay

            var queryParams =
                $"search/movie?query={Uri.EscapeDataString(query)}&include_adult=false&language=en-US&page={page}";

            if (year > 0)
            {
                queryParams += $"&year={year}";
            }

            var response = await _httpClient.GetAsync(queryParams);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TmdbSearchResultDto>(json);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            _throttle.Release();
        }

    }
}

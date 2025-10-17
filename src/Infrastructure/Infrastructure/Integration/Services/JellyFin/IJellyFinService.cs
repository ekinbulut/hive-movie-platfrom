namespace Infrastructure.Integration.Services.JellyFin;

public interface IJellyFinService
{
    Task<string?> GetMovieIdByNameAsync(string movieName, int? year = null);
    void SetConfiguration(IJellyFinServiceConfiguration jellyFinServiceConfiguration);
}
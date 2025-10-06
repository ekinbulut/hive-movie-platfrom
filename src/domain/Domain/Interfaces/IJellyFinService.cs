namespace Domain.Interfaces;

public interface IJellyFinService
{
    Task<string?> GetMovieIdByNameAsync(string movieName, int? year = null);
}
using Domain.DTOs;

namespace Domain.Interfaces;

public interface ITmdbApiService
{
    Task<TmdbMovieDto> GetMovieByIdAsync(int movieId);
    Task<TmdbSearchResultDto> SearchMoviesAsync(string query, int year = 0, int page = 1);
}

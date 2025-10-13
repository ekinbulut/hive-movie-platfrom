using Domain.Entities;

namespace Domain.Interfaces;

public interface IMovieRepository
{
    List<Movie> GetAllMovies();
    Task<List<Movie>> GetAllMoviesAsync(int pageNumber, int pageSize);
    Movie? GetMovieById(Guid id);
    bool GetMovieByHashValue(string name);
    
    void AddMovie(Movie movie);
    void UpdateMovie(Movie movie);
    void DeleteMovie(Guid id);
    Task<int> GetTotalMoviesCountAsync();
    Task<List<Movie>> GetMoviesByFilterAsync(int? queryYear, int pageNumber, int pageSize);
    Task<int> GetTotalMoviesCountByFilterAsync(int? queryYear);
    Task<List<int>> GetFiltersAsync();
}
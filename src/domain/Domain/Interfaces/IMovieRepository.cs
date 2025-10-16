using Domain.Entities;

namespace Domain.Interfaces;

public interface IMovieRepository
{
    List<Movie> GetAllMovies();
    Task<List<Movie>> GetAllMoviesAsync(int pageNumber, int pageSize, Guid userId);
    Movie? GetMovieById(Guid id);
    bool GetMovieByHashValue(string name);
    
    void AddMovie(Movie movie);
    void UpdateMovie(Movie movie);
    void DeleteMovie(Guid id);
    Task<int> GetTotalMoviesCountAsync(Guid userId);
    Task<List<Movie>> GetMoviesByFilterAsync(int? queryYear, int pageNumber, int pageSize, Guid userId);
    Task<int> GetTotalMoviesCountByFilterAsync(int? queryYear, Guid userId);
    Task<List<int>> GetFiltersAsync(Guid userId);
}
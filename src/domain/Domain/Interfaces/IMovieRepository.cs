using Domain.Entities;

namespace Domain.Interfaces;

public interface IMovieRepository
{
    List<Movie> GetAllMovies();
    List<Movie> GetAllMovies(int pageNumber, int pageSize);
    Movie GetMovieById(int id);
    void AddMovie(Movie movie);
    void UpdateMovie(Movie movie);
    void DeleteMovie(int id);
}
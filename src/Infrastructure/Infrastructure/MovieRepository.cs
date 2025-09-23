using Domain.Entities;
using Infrastructure.Context;

namespace Infrastructure;

public class MovieRepository : IMovieRepository
{
    private readonly HiveDbContext _context;
    
    public MovieRepository(HiveDbContext context)
    {
        _context = context;
    }
    
    // Add a method to get all movies
    public List<Movie> GetAllMovies()
    {
        return _context.Movies.ToList();
    }
    
    // Add a method to get a movie by id
    public Movie GetMovieById(int id)
    {
        return _context.Movies.FirstOrDefault(m => m.Id == id);
    }
    // Add a method to add a movie
    public void AddMovie(Movie movie)
    {
        _context.Movies.Add(movie);
        _context.SaveChanges();
    }
    // Add a method to update a movie
    public void UpdateMovie(Movie movie)
    {
        _context.Movies.Update(movie);
        _context.SaveChanges();
    }
    // Add a method to delete a movie
    public void DeleteMovie(int id)
    {
        var movie = _context.Movies.FirstOrDefault(m => m.Id == id);
        if (movie != null)
        {
            _context.Movies.Remove(movie);
            _context.SaveChanges();
        }
    }
    // Add a method to get all movies with pagination
    public List<Movie> GetAllMovies(int pageNumber, int pageSize)
    {
        return _context.Movies.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
    }

}
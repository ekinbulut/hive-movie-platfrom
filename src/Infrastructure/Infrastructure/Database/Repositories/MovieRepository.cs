using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repositories;

public class MovieRepository(HiveDbContext context) : IMovieRepository
{
    // Add a method to get all movies
    public List<Movie> GetAllMovies()
    {
        return context.Movies.ToList();
    }
    
    // Add a method to get a movie by id
    public Movie? GetMovieById(Guid id)
    {
        return context.Movies.FirstOrDefault(m => m.Id == id);
    }

    public bool GetMovieByHashValue(string name)
    {
       return context.Movies.FirstOrDefault(m => m.HashValue == name) != null;
    }

    // Add a method to add a movie
    public void AddMovie(Movie movie)
    {
        context.Movies.Add(movie);
        context.SaveChanges();
    }
    // Add a method to update a movie
    public void UpdateMovie(Movie movie)
    {
        context.Movies.Update(movie);
        context.SaveChanges();
    }
    // Add a method to delete a movie
    public void DeleteMovie(Guid id)
    {
        var movie = context.Movies.FirstOrDefault(m => m.Id == id);
        if (movie != null)
        {
            context.Movies.Remove(movie);
            context.SaveChanges();
        }
    }

    public async Task<int> GetTotalMoviesCountAsync()
    {
        return await context.Movies.CountAsync();
    }

    public async Task<List<Movie>> GetMoviesByFilterAsync(int? queryYear, int pageNumber, int pageSize)
    {
        return await context.Movies
            .Where(m => !queryYear.HasValue || m.ReleaseDate.HasValue && m.ReleaseDate == queryYear.Value)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalMoviesCountByFilterAsync(int? queryYear)
    {
        return await context.Movies
            .Where(m =>
                !queryYear.HasValue || m.ReleaseDate.HasValue && m.ReleaseDate == queryYear.Value)
            .CountAsync();
    }

    // Add a method to get all movies with pagination
    public async Task<List<Movie>> GetAllMoviesAsync(int pageNumber, int pageSize)
    {
        return await context.Movies.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
    }

}
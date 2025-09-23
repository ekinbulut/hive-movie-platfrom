using Domain.Entities;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Infrastructure.Tests;

public class MovieRepositoryTests
{
    [Fact]
    public void GetAllMovies_ReturnsAllMovies()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new HiveDbContext(options);
        context.Movies.AddRange(
            new Movie { Id = 1, Name = "Movie 1" },
            new Movie { Id = 2, Name = "Movie 2" }
        );
        context.SaveChanges();

        var repository = new MovieRepository(context);

        var result = repository.GetAllMovies();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.Name == "Movie 1");
        Assert.Contains(result, m => m.Name == "Movie 2");
    }

    [Fact]
    public void GetAllMovies_ReturnsEmptyList_WhenNoMoviesExist()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new HiveDbContext(options);

        var repository = new MovieRepository(context);

        var result = repository.GetAllMovies();

        Assert.Empty(result);
    }

[Fact]
    public void GetMovieById_ReturnsMovie_WhenMovieExists()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    
        using var context = new HiveDbContext(options);
        var movie = new Movie { Id = 1, Name = "Movie 1" };
        context.Movies.Add(movie);
        context.SaveChanges();
    
        var repository = new MovieRepository(context);
    
        var result = repository.GetMovieById(1);
    
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Movie 1", result.Name);
    }
    
    [Fact]
    public void GetMovieById_ReturnsNull_WhenMovieDoesNotExist()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    
        using var context = new HiveDbContext(options);
    
        var repository = new MovieRepository(context);
    
        var result = repository.GetMovieById(1);
    
        Assert.Null(result);
    }
    
    [Fact]
    public void AddMovie_AddsMovieToDatabase()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    
        using var context = new HiveDbContext(options);
        var repository = new MovieRepository(context);
        var movie = new Movie { Id = 1, Name = "Movie 1" };
    
        repository.AddMovie(movie);
    
        var result = context.Movies.FirstOrDefault(m => m.Id == 1);
        Assert.NotNull(result);
        Assert.Equal("Movie 1", result.Name);
    }
    
    [Fact]
    public void UpdateMovie_UpdatesExistingMovie()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    
        using var context = new HiveDbContext(options);
        var movie = new Movie { Id = 1, Name = "Movie 1" };
        context.Movies.Add(movie);
        context.SaveChanges();
    
        var repository = new MovieRepository(context);
        movie.Name = "Updated Movie";
        repository.UpdateMovie(movie);
    
        var result = context.Movies.FirstOrDefault(m => m.Id == 1);
        Assert.NotNull(result);
        Assert.Equal("Updated Movie", result.Name);
    }
    
    [Fact]
    public void DeleteMovie_RemovesMovieFromDatabase_WhenMovieExists()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    
        using var context = new HiveDbContext(options);
        var movie = new Movie { Id = 1, Name = "Movie 1" };
        context.Movies.Add(movie);
        context.SaveChanges();
    
        var repository = new MovieRepository(context);
        repository.DeleteMovie(1);
    
        var result = context.Movies.FirstOrDefault(m => m.Id == 1);
        Assert.Null(result);
    }
    
    [Fact]
    public void DeleteMovie_DoesNothing_WhenMovieDoesNotExist()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    
        using var context = new HiveDbContext(options);
    
        var repository = new MovieRepository(context);
        repository.DeleteMovie(1);
    
        Assert.Empty(context.Movies);
    }
    
    [Fact]
    public void GetAllMoviesWithPagination_ReturnsCorrectPage()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    
        using var context = new HiveDbContext(options);
        context.Movies.AddRange(
            new Movie { Id = 1, Name = "Movie 1" },
            new Movie { Id = 2, Name = "Movie 2" },
            new Movie { Id = 3, Name = "Movie 3" }
        );
        context.SaveChanges();
    
        var repository = new MovieRepository(context);
    
        var result = repository.GetAllMovies(2, 1);
    
        Assert.Single(result);
        Assert.Equal("Movie 2", result[0].Name);
    }
    
    [Fact]
    public void GetAllMoviesWithPagination_ReturnsEmptyList_WhenPageOutOfRange()
    {
        var options = new DbContextOptionsBuilder<HiveDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    
        using var context = new HiveDbContext(options);
        context.Movies.AddRange(
            new Movie { Id = 1, Name = "Movie 1" },
            new Movie { Id = 2, Name = "Movie 2" }
        );
        context.SaveChanges();
    
        var repository = new MovieRepository(context);
    
        var result = repository.GetAllMovies(3, 2);
    
        Assert.Empty(result);
    }
    

}
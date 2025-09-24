using Domain.Interfaces;
using FastEndpoints;

namespace Features.GetAllMovies;

public class GetAllMoviesEndpoint(IMovieRepository movieRepository) : Endpoint<GetMoviesRequest, GetMovieResponse>
{
    public override void Configure()
    {
        Post("v1/api/movies"); // TODO: define v1/api prefix in a constant
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetMoviesRequest req, CancellationToken ct)
    {
        
        var movies = movieRepository.GetAllMovies(req.pageNumber, req.pageSize);
        
        await Send.OkAsync(new GetMovieResponse()
        {
            Movies = movies.Select(m => new MovieDTO()
            {
                Id = m.Id,
                Name = m.Name,
            }).ToList(),
            PageSize = req.pageSize,
            PageNumber = req.pageNumber,
        });
    }
}
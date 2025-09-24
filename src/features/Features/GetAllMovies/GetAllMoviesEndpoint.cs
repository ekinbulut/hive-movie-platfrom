using Domain.Interfaces;
using FastEndpoints;
using Features.Extensions;

namespace Features.GetAllMovies;

public class GetAllMoviesEndpoint(IMovieRepository movieRepository) : Endpoint<GetMoviesRequest, GetMovieResponse>
{
    public override void Configure()
    {
        Post("/movies");
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
                FilePath = m.FilePath,
                SubTitleFilePath = m.SubTitleFilePath,
                FileSize = m.FileSize.ToHumanReadableSize(),
                Image = m.Image
                
            }).ToList(),
            PageSize = req.pageSize,
            PageNumber = req.pageNumber,
        }, ct);
    }
}
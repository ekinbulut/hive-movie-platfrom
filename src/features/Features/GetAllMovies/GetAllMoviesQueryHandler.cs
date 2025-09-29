using Domain.Abstraction.Mediator;
using Domain.Interfaces;
using Features.Extensions;

namespace Features.GetAllMovies;

public class GetAllMoviesQueryHandler : IQueryHandler<GetAllMoviesQuery, GetMovieResponse>
{
    private readonly IMovieRepository _movieRepository;

    public GetAllMoviesQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<GetMovieResponse> HandleAsync(GetAllMoviesQuery query, CancellationToken cancellationToken = default)
    {
        var movies = _movieRepository.GetAllMovies(query.PageNumber, query.PageSize);
        
        return await Task.FromResult(new GetMovieResponse()
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
            PageSize = query.PageSize,
            PageNumber = query.PageNumber,
        });
    }
}
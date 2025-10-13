using Domain.Abstraction.Mediator;
using Domain.Interfaces;
using Features.Extensions;

namespace Features.GetAllMovies;

public class GetAllMoviesQueryHandler(IMovieRepository movieRepository)
    : IQueryHandler<GetAllMoviesQuery, GetMovieResponse>
{
    public async Task<GetMovieResponse> HandleAsync(GetAllMoviesQuery query, CancellationToken cancellationToken = default)
    {
        var movies = await movieRepository.GetAllMoviesAsync(query.PageNumber, query.PageSize);
        var total = await movieRepository.GetTotalMoviesCountAsync();
        
        return await Task.FromResult(new GetMovieResponse()
        {
            Movies = movies.Select(m => new MovieDTO()
            {
                Id = m.Id,
                Name = m.Name,
                FilePath = m.FilePath,
                SubTitleFilePath = m.SubTitleFilePath,
                FileSize = m.FileSize.ToHumanReadableSize(),
                Image = m.Image,
                CreatedTime = m.CreatedTime,
                StreamId = m.JellyFinId,
                ReleaseDate = m.ReleaseDate
                
            })
                .OrderByDescending(x=> x.CreatedTime)
                .ToList(),
            PageSize = query.PageSize,
            PageNumber = query.PageNumber,
            Total = total
        });
    }
}
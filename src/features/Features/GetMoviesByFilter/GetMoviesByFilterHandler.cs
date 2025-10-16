using Domain.Abstraction.Mediator;
using Domain.Interfaces;
using Features.Extensions;
using Features.GetAllMovies;

namespace Features.GetMoviesByFilter;

public class GetMoviesByFilterHandler(IMovieRepository movieRepository) : IQueryHandler<GetMoviesByFilterQuery, GetMoviesByFilterResponse>
{
    public async Task<GetMoviesByFilterResponse> HandleAsync(GetMoviesByFilterQuery query, CancellationToken cancellationToken = default)
    {
        var movies = await movieRepository.GetMoviesByFilterAsync(query.Year, query.PageNumber, query.PageSize, query.UserId);
        var total = await movieRepository.GetTotalMoviesCountByFilterAsync(query.Year, query.UserId);

        return await Task.FromResult(new GetMoviesByFilterResponse()
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
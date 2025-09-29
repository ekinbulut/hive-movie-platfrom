using Domain.Abstraction.Mediator;

namespace Features.GetAllMovies;

public record GetMoviesRequest(int pageNumber, int pageSize);

public class GetAllMoviesQuery : IQuery<GetMovieResponse>
{
    public int PageNumber { get; }
    public int PageSize { get; }

    public GetAllMoviesQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
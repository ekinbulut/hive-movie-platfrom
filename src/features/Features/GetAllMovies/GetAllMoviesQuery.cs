using Domain.Abstraction.Mediator;

namespace Features.GetAllMovies;

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
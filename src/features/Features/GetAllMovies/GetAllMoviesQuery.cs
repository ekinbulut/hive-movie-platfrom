using Domain.Abstraction.Mediator;

namespace Features.GetAllMovies;

public class GetAllMoviesQuery : IQuery<GetMovieResponse>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public Guid UserId { get; set; }

    public GetAllMoviesQuery(int pageNumber, int pageSize, Guid userId)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        UserId = userId;
    }
}
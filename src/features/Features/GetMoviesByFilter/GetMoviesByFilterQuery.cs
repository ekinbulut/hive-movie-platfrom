using Domain.Abstraction.Mediator;

namespace Features.GetMoviesByFilter;

public class GetMoviesByFilterQuery : IQuery<GetMoviesByFilterResponse>
{
    public int? Year { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
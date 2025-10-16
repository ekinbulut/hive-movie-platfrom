using Domain.Abstraction.Mediator;

namespace Features.GetFilters;

public class GetFiltersQuery : IQuery<GetFiltersResponse>
{
    public Guid UserId { get; set; }
}
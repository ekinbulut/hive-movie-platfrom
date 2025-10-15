using Domain.Abstraction.Mediator;
using Domain.Interfaces;
using Features.GetMoviesByFilter;

namespace Features.GetFilters;

public class GetFiltersQueryHandler(IMovieRepository movieRepository) : IQueryHandler<GetFiltersQuery, GetFiltersResponse>
{
    public async Task<GetFiltersResponse> HandleAsync(GetFiltersQuery query, CancellationToken cancellationToken = default)
    {
        var years = await movieRepository.GetFiltersAsync();
        
        return await Task.FromResult(new GetFiltersResponse()
        {
            Filters = new Filters()
            {
                Years = years
            }
        });
    }
}
using FastEndpoints;
using FluentValidation;

namespace Features.GetAllMovies;

public class GetMoviesRequestValidator : Validator<GetMoviesRequest>
{
    public GetMoviesRequestValidator()
    {
        RuleFor(x=> x.pageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");
        
        RuleFor(x=> x.pageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be less than or equal to 100");
    }
}
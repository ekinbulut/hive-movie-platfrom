namespace Features.GetMoviesByFilter;

public class GetMoviesByFilterRequest
{
    public int? Year { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
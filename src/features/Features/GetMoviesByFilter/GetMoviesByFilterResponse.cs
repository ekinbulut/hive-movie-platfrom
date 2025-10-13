using Features.GetAllMovies;

namespace Features.GetMoviesByFilter;

public class GetMoviesByFilterResponse
{
    public ICollection<MovieDTO> Movies { get; set; }

    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    
    public int Total { get; set; }

    public GetMoviesByFilterResponse()
    {
        Movies = new List<MovieDTO>();
    }
}
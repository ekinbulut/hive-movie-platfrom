namespace Features.GetAllMovies;

public class GetMovieResponse
{
    public ICollection<MovieDTO> Movies { get; set; }

    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    
    public GetMovieResponse()
    {
        Movies = new List<MovieDTO>();
    }
}
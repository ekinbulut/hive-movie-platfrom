namespace Features.GetAllMovies;

public class MovieDTO
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? FilePath { get; set; }

    public string? FileSize { get; set; }

    public string? SubTitleFilePath { get; set; }

    public string? Image { get; set; }
    
    //get full image url
    public string? FullImageUrl => string.IsNullOrEmpty(Image) ? null : $"https://image.tmdb.org/t/p/w500{Image}";
    public DateTime? CreatedTime { get; set; }

    public string? StreamId { get; set; }
}
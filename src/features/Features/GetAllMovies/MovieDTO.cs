namespace Features.GetAllMovies;

public class MovieDTO
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? FilePath { get; set; }

    public DateTime? CreatedTime { get; set; }

    public DateTime? ModifiedTime { get; set; }

    public bool? IsActive { get; set; }

    public long? FileSize { get; set; }

    public string? SubTitleFilePath { get; set; }

    public string? Image { get; set; }
}
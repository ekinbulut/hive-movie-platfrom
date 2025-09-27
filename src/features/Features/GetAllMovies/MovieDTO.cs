namespace Features.GetAllMovies;

public class MovieDTO
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public string? FilePath { get; set; }

    public string? FileSize { get; set; }

    public string? SubTitleFilePath { get; set; }

    public string? Image { get; set; }
}
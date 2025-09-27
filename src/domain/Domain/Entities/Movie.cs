namespace Domain.Entities;

public partial class Movie
{
    public string? Name { get; set; }

    public string? FilePath { get; set; }

    public DateTime? CreatedTime { get; set; }

    public DateTime? ModifiedTime { get; set; }

    public bool? IsActive { get; set; }

    public long? FileSize { get; set; }

    public string? SubTitleFilePath { get; set; }

    public string? Image { get; set; }

    public Guid Id { get; set; }

    public string? HashValue { get; set; }
}

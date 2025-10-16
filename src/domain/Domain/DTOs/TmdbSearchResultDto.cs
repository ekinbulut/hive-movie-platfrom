using System.Text.Json.Serialization;

namespace Domain.DTOs;

public class TmdbSearchResultDto
{
    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("results")]
    public TmdbMovieDto[] Results { get; set; } = Array.Empty<TmdbMovieDto>();

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }
}
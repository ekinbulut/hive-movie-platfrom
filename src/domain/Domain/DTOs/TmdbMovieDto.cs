using System.Text.Json.Serialization;

namespace Domain.DTOs;

// Infrastructure/DTOs/TmdbMovieDto.cs
public class TmdbMovieDto
{
    [JsonPropertyName("adult")]
    public bool Adult { get; set; }

    [JsonPropertyName("backdrop_path")]
    public string? BackdropPath { get; set; }

    [JsonPropertyName("genre_ids")]
    public int[] GenreIds { get; set; } = Array.Empty<int>();

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("original_language")]
    public string OriginalLanguage { get; set; } = string.Empty;

    [JsonPropertyName("original_title")]
    public string OriginalTitle { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonPropertyName("popularity")]
    public double Popularity { get; set; }

    [JsonPropertyName("poster_path")]
    public string? PosterPath { get; set; }

    [JsonPropertyName("release_date")]
    public string ReleaseDate { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("video")]
    public bool Video { get; set; }

    [JsonPropertyName("vote_average")]
    public double VoteAverage { get; set; }

    [JsonPropertyName("vote_count")]
    public int VoteCount { get; set; }

    // Helper method to get full poster URL
    public string GetFullPosterUrl(string baseImageUrl = "https://image.tmdb.org/t/p/w780")
    {
        return string.IsNullOrEmpty(PosterPath) ? string.Empty : $"{baseImageUrl}{PosterPath}";
    }
}


// Infrastructure/DTOs/TmdbSearchResultDto.cs
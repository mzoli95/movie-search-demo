using System.Text.Json.Serialization;

namespace MovieInformationProvider.DTOs.Movie;

public class OmdbSearchResponseDto
{
    public List<OmdbSearchItem>? Search { get; set; }
    [JsonPropertyName("totalResults")]
    public string? TotalResults { get; set; }
    public string? Response { get; set; }
    public string? Error { get; set; }
}


public class OmdbSearchItem
{
    public string? Title { get; set; }
    public string? Year { get; set; }
    [JsonPropertyName("imdbID")]
    public string? ImdbId { get; set; }
    public string? Type { get; set; }
}

public class OmdbMovieDetailsResponse
{
    public string? Title { get; set; }
    public string? Year { get; set; }
    public string? Director { get; set; }
    public string? Response { get; set; }
    public string? Error { get; set; }
}
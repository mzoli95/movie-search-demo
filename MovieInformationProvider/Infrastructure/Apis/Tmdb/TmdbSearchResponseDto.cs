using System.Text.Json.Serialization;

namespace MovieInformationProvider.Infrastructure.Apis.Tmdb;

public class TmdbSearchResponseDto
{
    public int Page { get; set; }

    [JsonPropertyName("results")]
    public List<TmdbMovieItem>? Results { get; set; }

    [JsonPropertyName("total_results")]
    public int TotalResults { get; set; }

    [JsonPropertyName("total_pages")]
    public int TotalPages { get; set; }
}

public class TmdbMovieItem
{
    public int Id { get; set; }
    public string? Title { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }
}

public class TmdbCreditsResponse
{
    public List<TmdbCrewMember>? Crew { get; set; }
}

public class TmdbCrewMember
{
    public string? Name { get; set; }
    public string? Job { get; set; }
}
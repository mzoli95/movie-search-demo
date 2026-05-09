using System.Text.Json.Serialization;

namespace MovieInformationProvider.Application.Movies.DTOs;

public sealed record MovieDto
{
    public required string Title { get; init; }

    public required string Year { get; init; }

    [JsonPropertyName("Director")]
    public required IReadOnlyList<string> Directors { get; init; }
}
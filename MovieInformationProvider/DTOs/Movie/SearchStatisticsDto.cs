namespace MovieInformationProvider.DTOs.Movie;

public class SaveSearchStatisticDto
{
    public required string Api { get; init; }
    public required string Query { get; init; }
    public int ResultCount { get; init; }
    public bool ServedFromCache { get; init; }
    public int DurationMs { get; init; }
}
using MovieInformationProvider.Domain.Enums;

namespace MovieInformationProvider.Application.SearchStatistics.DTOs;

public sealed record SaveSearchStatisticDto
{
    public required MovieApiProvider Api { get; init; }
    public required string Query { get; init; }
    public int ResultCount { get; init; }
    public bool ServedFromCache { get; init; }
    public int DurationMs { get; init; }
}
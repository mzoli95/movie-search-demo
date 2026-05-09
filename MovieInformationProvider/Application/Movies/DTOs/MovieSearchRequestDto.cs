using MovieInformationProvider.Domain.Enums;

namespace MovieInformationProvider.Application.Movies.DTOs;

public sealed record MovieSearchRequestDto
{
    public required MovieApiProvider Api { get; init; }
    public required string MovieTitle { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
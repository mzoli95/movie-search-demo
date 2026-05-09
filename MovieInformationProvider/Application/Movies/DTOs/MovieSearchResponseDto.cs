namespace MovieInformationProvider.Application.Movies.DTOs;

public sealed record MovieSearchResponseDto
{
    public required IReadOnlyList<MovieDto> Movies { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalResults { get; init; }
    public int TotalPages { get; init; }
}
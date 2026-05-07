namespace MovieInformationProvider.DTOs.Movie;

public class MovieSearchResponseDto
{
    public required List<MovieDto> Movies { get; init; }
    public int CurrentPage { get; init; }
    public int PageSize { get; init; }
    public int TotalResults { get; init; }
    public int TotalPages { get; init; }
}
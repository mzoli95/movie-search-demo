namespace MovieInformationProvider.DTOs.Movie;

public class MovieSearchRequestDto
{
    public required string Api { get; init; }
    public required string MovieTitle { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
namespace MovieInformationProvider.DTOs.Movie;

public class MovieDto
{
    public required string Title { get; init; }

    public required string Year { get; init; }

    public required List<string> Director { get; init; }
}
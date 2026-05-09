using MovieInformationProvider.Application.Movies.DTOs;

namespace MovieInformationProvider.Application.Movies.Interfaces;

public interface IMovieService
{
    Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default);
}
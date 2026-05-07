using MovieInformationProvider.DTOs;
using MovieInformationProvider.DTOs.Movie;

namespace MovieInformationProvider.Services.Interfaces;

public interface IMovieService
{
    Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default);
}
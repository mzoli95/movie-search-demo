using MovieInformationProvider.DTOs;
using MovieInformationProvider.DTOs.Movie;

namespace MovieInformationProvider.Services.Interfaces;

public interface IMovieApiClient
{
    string ApiName { get; }

    Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default);
}
using MovieInformationProvider.Application.Movies.DTOs;
using MovieInformationProvider.Domain.Enums;

namespace MovieInformationProvider.Application.Movies.Interfaces;

public interface IMovieApiClient
{
    MovieApiProvider ApiName { get; }

    Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default);
}
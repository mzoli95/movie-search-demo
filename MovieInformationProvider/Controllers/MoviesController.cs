using Microsoft.AspNetCore.Mvc;
using MovieInformationProvider.Application.Movies.DTOs;
using MovieInformationProvider.Application.Movies.Interfaces;
using MovieInformationProvider.Application.SearchStatistics.Interfaces;
using MovieInformationProvider.Domain.Enums;

namespace MovieInformationProvider.Controllers;

[ApiController]
[Route("movies")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly ISearchStatisticService _searchStatisticService;

    public MoviesController(
        IMovieService movieService,
        ISearchStatisticService searchStatisticService)
    {
        _movieService = movieService;
        _searchStatisticService = searchStatisticService;
    }

    [HttpGet("{movieTitle}")]
    public async Task<IActionResult> GetMovies(
        string movieTitle,
        [FromQuery] string api,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<MovieApiProvider>(api, ignoreCase: true, out var apiProvider))
        {
            return BadRequest("Invalid API. Allowed: omdb, tmdb");
        }

        MovieSearchResponseDto result = await _movieService.SearchMoviesAsync(
            new MovieSearchRequestDto
            {
                Api = apiProvider,
                MovieTitle = movieTitle,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> GetAutocomplete(
        [FromQuery] string query,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest("Query parameter is required");
        }

        List<string> suggestions = await _searchStatisticService.GetAutocompleteAsync(
            query,
            limit,
            cancellationToken);

        return Ok(suggestions);
    }
}
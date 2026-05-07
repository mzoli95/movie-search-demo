using Microsoft.AspNetCore.Mvc;
using MovieInformationProvider.DTOs.Movie;
using MovieInformationProvider.Services.Interfaces;

namespace MovieInformationProvider.Controllers;

[ApiController]
[Route("movies")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet("{movieTitle}")]
    public async Task<IActionResult> GetMovies(
        string movieTitle,
        [FromQuery] string api,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _movieService.SearchMoviesAsync(
            new MovieSearchRequestDto
            {
                Api = api,
                MovieTitle = movieTitle,
                Page = page,
                PageSize = pageSize
            },
            cancellationToken);

        return Ok(result);
    }
}
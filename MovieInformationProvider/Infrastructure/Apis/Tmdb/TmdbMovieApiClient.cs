using MovieInformationProvider.Application.Movies.DTOs;
using MovieInformationProvider.Application.Movies.Interfaces;
using MovieInformationProvider.Domain.Enums;

namespace MovieInformationProvider.Infrastructure.Apis.Tmdb;

/// <summary>
/// TMDb (The Movie Database) API client implementation
/// </summary>
public class TmdbMovieApiClient : IMovieApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TmdbMovieApiClient> _logger;

    public MovieApiProvider ApiName => MovieApiProvider.Tmdb;

    public TmdbMovieApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TmdbMovieApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        string baseUrl = _configuration["ExternalApis:Tmdb:BaseUrl"]!;
        string apiKey = _configuration["ExternalApis:Tmdb:ApiKey"]!;

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("TMDB BaseUrl is missing.");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("TMDB ApiKey is missing.");

        string searchUrl =
            $"{baseUrl}/search/movie" +
            $"?api_key={apiKey}" +
            $"&query={Uri.EscapeDataString(request.MovieTitle)}" +
            $"&include_adult=true" +
            $"&page={request.Page}";

        TmdbSearchResponseDto? searchResponse =
            await _httpClient.GetFromJsonAsync<TmdbSearchResponseDto>(
                searchUrl,
                cancellationToken);

        if (searchResponse?.Results is null || searchResponse.Results.Count == 0)
        {
            return EmptyResponse(request);
        }

        List<TmdbMovieItem> pagedResults = searchResponse.Results
            .Take(request.PageSize)
            .ToList();

        IEnumerable<Task<MovieDto?>> detailTasks = pagedResults
            .Select(x => GetMovieDetailsAsync(
                baseUrl,
                apiKey,
                x,
                cancellationToken));

        MovieDto?[] detailResults = await Task.WhenAll(detailTasks);

        return new MovieSearchResponseDto
        {
            Movies = detailResults
                .Where(x => x is not null)
                .Select(x => x!)
                .ToList(),

            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalResults = searchResponse.TotalResults,
            TotalPages = CalculateTotalPages(
                searchResponse.TotalResults,
                request.PageSize)
        };
    }

    private async Task<MovieDto?> GetMovieDetailsAsync(
        string baseUrl,
        string apiKey,
        TmdbMovieItem movie,
        CancellationToken cancellationToken)
    {
        try
        {
            string creditsUrl = $"{baseUrl}/movie/{movie.Id}/credits?api_key={apiKey}";

            _logger.LogDebug("Fetching TMDb credits for movie ID: {MovieId}", movie.Id);

            TmdbCreditsResponse? credits = await _httpClient.GetFromJsonAsync<TmdbCreditsResponse>(
                creditsUrl,
                cancellationToken);

            List<string> directors = credits?.Crew?
                .Where(x => x.Job?.Equals("Director", StringComparison.OrdinalIgnoreCase) == true)
                .Select(x => x.Name)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Cast<string>()
                .Distinct()
                .ToList() ?? new List<string>();

            MovieDto movieDto = new MovieDto
            {
                Title = movie.Title ?? "Unknown",
                Year = ExtractYear(movie.ReleaseDate),
                Directors = directors
            };

            return movieDto;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to fetch details for TMDb movie ID: {MovieId}", movie.Id);
            return null;
        }
    }

    private static string ExtractYear(string? releaseDate)
    {
        if (string.IsNullOrWhiteSpace(releaseDate))
            return "Unknown";

        bool success = DateTime.TryParse(releaseDate, out DateTime parsedDate);
        return success ? parsedDate.Year.ToString() : "Unknown";
    }

    private static int CalculateTotalPages(int totalResults, int pageSize)
    {
        if (totalResults <= 0)
            return 0;

        int pages = (int)Math.Ceiling(totalResults / (double)pageSize);
        return pages;
    }

    private static MovieSearchResponseDto EmptyResponse(MovieSearchRequestDto request)
    {
        MovieSearchResponseDto response = new MovieSearchResponseDto
        {
            Movies = new List<MovieDto>(),
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalResults = 0,
            TotalPages = 0
        };

        return response;
    }
}
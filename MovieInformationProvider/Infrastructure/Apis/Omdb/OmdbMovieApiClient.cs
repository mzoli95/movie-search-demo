using MovieInformationProvider.Application.Movies.DTOs;
using MovieInformationProvider.Application.Movies.Interfaces;
using MovieInformationProvider.Domain.Enums;

namespace MovieInformationProvider.Infrastructure.Apis.Omdb;

public class OmdbMovieApiClient : IMovieApiClient
{
    private const int OmdbPageSize = 10;

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OmdbMovieApiClient> _logger;

    public MovieApiProvider ApiName => MovieApiProvider.Omdb;

    public OmdbMovieApiClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<OmdbMovieApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        string? baseUrl = _configuration["ExternalApis:Omdb:BaseUrl"];
        string? apiKey = _configuration["ExternalApis:Omdb:ApiKey"];

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("OMDb BaseUrl is missing.");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OMDb ApiKey is missing.");

        OmdbSearchResponseDto? firstPage = await GetSearchPageAsync(
            baseUrl,
            apiKey,
            request.MovieTitle,
            page: 1,
            cancellationToken);

        if (firstPage?.Search is null || firstPage.Response == "False")
        {
            _logger.LogInformation("No results found for '{MovieTitle}' in OMDb", request.MovieTitle);
            return EmptyResponse(request);
        }

        int totalResults = ParseTotalResults(firstPage.TotalResults);
        int totalPages = CalculateTotalPages(totalResults, request.PageSize);

        _logger.LogDebug("OMDb found {TotalResults} results for '{MovieTitle}'", totalResults, request.MovieTitle);

        int requiredOmdbPages = CalculateRequiredOmdbPages(
            requestedPage: request.Page,
            requestedPageSize: request.PageSize);

        List<OmdbSearchResponseDto> searchPages = await GetRequiredSearchPagesAsync(
            baseUrl,
            apiKey,
            request.MovieTitle,
            firstPage,
            requiredOmdbPages,
            cancellationToken);

        List<OmdbSearchItem> pagedSearchItems = searchPages
            .SelectMany(x => x.Search ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.ImdbId))
            .GroupBy(x => x.ImdbId)
            .Select(x => x.First())
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        IEnumerable<Task<MovieDto?>> detailTasks = pagedSearchItems
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
            TotalResults = totalResults,
            TotalPages = totalPages
        };
    }

    private async Task<List<OmdbSearchResponseDto>> GetRequiredSearchPagesAsync(
        string baseUrl,
        string apiKey,
        string movieTitle,
        OmdbSearchResponseDto firstPage,
        int requiredOmdbPages,
        CancellationToken cancellationToken)
    {
        if (requiredOmdbPages <= 1)
            return [firstPage];

        IEnumerable<Task<OmdbSearchResponseDto?>> remainingPageTasks = Enumerable.Range(2, requiredOmdbPages - 1)
            .Select(page => GetSearchPageAsync(
                baseUrl,
                apiKey,
                movieTitle,
                page,
                cancellationToken));

        OmdbSearchResponseDto?[] remainingPages = await Task.WhenAll(remainingPageTasks);

        return [firstPage, .. remainingPages.Where(x => x is not null).Select(x => x!)];
    }

    private async Task<OmdbSearchResponseDto?> GetSearchPageAsync(
        string baseUrl,
        string apiKey,
        string movieTitle,
        int page,
        CancellationToken cancellationToken)
    {
        string searchUrl =
            $"{baseUrl}?s={Uri.EscapeDataString(movieTitle)}&type=movie&page={page}&apikey={apiKey}";

        return await _httpClient.GetFromJsonAsync<OmdbSearchResponseDto>(
            searchUrl,
            cancellationToken);
    }

    private async Task<MovieDto?> GetMovieDetailsAsync(
        string baseUrl,
        string apiKey,
        OmdbSearchItem searchItem,
        CancellationToken cancellationToken)
    {
        string detailsUrl =
            $"{baseUrl}?i={Uri.EscapeDataString(searchItem.ImdbId!)}&apikey={apiKey}";

        OmdbMovieDetailsResponse? details = await _httpClient.GetFromJsonAsync<OmdbMovieDetailsResponse>(
            detailsUrl,
            cancellationToken);

        if (details is null || details.Response == "False")
            return null;

        return new MovieDto
        {
            Title = details.Title ?? searchItem.Title ?? "Unknown",
            Year = details.Year ?? searchItem.Year ?? "Unknown",
            Directors = SplitDirectors(details.Director)
        };
    }

    private static int CalculateRequiredOmdbPages(
        int requestedPage,
        int requestedPageSize)
    {
        int requiredItems = requestedPage * requestedPageSize;

        return (int)Math.Ceiling(requiredItems / (double)OmdbPageSize);
    }

    private static int CalculateTotalPages(int totalResults, int pageSize)
    {
        if (totalResults <= 0)
            return 0;

        return (int)Math.Ceiling(totalResults / (double)pageSize);
    }

    private static MovieSearchResponseDto EmptyResponse(MovieSearchRequestDto request)
    {
        return new MovieSearchResponseDto
        {
            Movies = [],
            CurrentPage = request.Page,
            PageSize = request.PageSize,
            TotalResults = 0,
            TotalPages = 0
        };
    }

    private static List<string> SplitDirectors(string? director)
    {
        if (string.IsNullOrWhiteSpace(director) || director == "N/A")
            return [];

        return director
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();
    }

    private static int ParseTotalResults(string? totalResults)
    {
        return int.TryParse(totalResults, out var result)
            ? result
            : 0;
    }
}
using MovieInformationProvider.DTOs;
using MovieInformationProvider.DTOs.Movie;
using MovieInformationProvider.Services.Interfaces;

namespace MovieInformationProvider.Services.ApiClients;

public class OmdbMovieApiClient : IMovieApiClient
{
    private const int OmdbPageSize = 10;

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public string ApiName => "omdb";

    public OmdbMovieApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["ExternalApis:Omdb:BaseUrl"];
        var apiKey = _configuration["ExternalApis:Omdb:ApiKey"];

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("OMDb BaseUrl is missing.");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OMDb ApiKey is missing.");

        var firstPage = await GetSearchPageAsync(
            baseUrl,
            apiKey,
            request.MovieTitle,
            page: 1,
            cancellationToken);

        if (firstPage?.Search is null || firstPage.Response == "False")
        {
            return EmptyResponse(request);
        }

        var totalResults = ParseTotalResults(firstPage.TotalResults);
        var totalPages = CalculateTotalPages(totalResults, request.PageSize);

        var requiredOmdbPages = CalculateRequiredOmdbPages(
            requestedPage: request.Page,
            requestedPageSize: request.PageSize);

        var searchPages = await GetRequiredSearchPagesAsync(
            baseUrl,
            apiKey,
            request.MovieTitle,
            firstPage,
            requiredOmdbPages,
            cancellationToken);

        var pagedSearchItems = searchPages
            .SelectMany(x => x.Search ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.ImdbId))
            .GroupBy(x => x.ImdbId)
            .Select(x => x.First())
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var detailTasks = pagedSearchItems
            .Select(x => GetMovieDetailsAsync(
                baseUrl,
                apiKey,
                x,
                cancellationToken));

        var detailResults = await Task.WhenAll(detailTasks);

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

        var remainingPageTasks = Enumerable.Range(2, requiredOmdbPages - 1)
            .Select(page => GetSearchPageAsync(
                baseUrl,
                apiKey,
                movieTitle,
                page,
                cancellationToken));

        var remainingPages = await Task.WhenAll(remainingPageTasks);

        return [firstPage, .. remainingPages.Where(x => x is not null).Select(x => x!)];
    }

    private async Task<OmdbSearchResponseDto?> GetSearchPageAsync(
        string baseUrl,
        string apiKey,
        string movieTitle,
        int page,
        CancellationToken cancellationToken)
    {
        var searchUrl =
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
        var detailsUrl =
            $"{baseUrl}?i={Uri.EscapeDataString(searchItem.ImdbId!)}&apikey={apiKey}";

        var details = await _httpClient.GetFromJsonAsync<OmdbMovieDetailsResponse>(
            detailsUrl,
            cancellationToken);

        if (details is null || details.Response == "False")
            return null;

        return new MovieDto
        {
            Title = details.Title ?? searchItem.Title ?? "Unknown",
            Year = details.Year ?? searchItem.Year ?? "Unknown",
            Director = SplitDirectors(details.Director)
        };
    }

    private static int CalculateRequiredOmdbPages(
        int requestedPage,
        int requestedPageSize)
    {
        var requiredItems = requestedPage * requestedPageSize;

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
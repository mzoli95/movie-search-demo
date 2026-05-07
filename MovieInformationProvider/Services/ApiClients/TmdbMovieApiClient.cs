using MovieInformationProvider.DTOs.Movie;
using MovieInformationProvider.Services.Interfaces;

namespace MovieInformationProvider.Services.ApiClients;

public class TmdbMovieApiClient : IMovieApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public string ApiName => "tmdb";

    public TmdbMovieApiClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = _configuration["ExternalApis:Tmdb:BaseUrl"];
        var apiKey = _configuration["ExternalApis:Tmdb:ApiKey"];

        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("TMDB BaseUrl is missing.");

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("TMDB ApiKey is missing.");

        var searchUrl =
            $"{baseUrl}/search/movie" +
            $"?api_key={apiKey}" +
            $"&query={Uri.EscapeDataString(request.MovieTitle)}" +
            $"&include_adult=true" +
            $"&page={request.Page}";

        var searchResponse =
            await _httpClient.GetFromJsonAsync<TmdbSearchResponseDto>(
                searchUrl,
                cancellationToken);

        if (searchResponse?.Results is null || searchResponse.Results.Count == 0)
        {
            return EmptyResponse(request);
        }

        var pagedResults = searchResponse.Results
            .Take(request.PageSize)
            .ToList();

        var detailTasks = pagedResults
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
        var creditsUrl =
            $"{baseUrl}/movie/{movie.Id}/credits?api_key={apiKey}";

        var credits =
            await _httpClient.GetFromJsonAsync<TmdbCreditsResponse>(
                creditsUrl,
                cancellationToken);

        var directors = credits?.Crew?
            .Where(x =>
                x.Job?.Equals("Director", StringComparison.OrdinalIgnoreCase) == true)
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct()
            .ToList() ?? [];

        return new MovieDto
        {
            Title = movie.Title ?? "Unknown",
            Year = ExtractYear(movie.ReleaseDate),
            Director = directors
        };
    }

    private static string ExtractYear(string? releaseDate)
    {
        if (string.IsNullOrWhiteSpace(releaseDate))
            return "Unknown";

        if (DateTime.TryParse(releaseDate, out var parsedDate))
            return parsedDate.Year.ToString();

        return "Unknown";
    }

    private static int CalculateTotalPages(
        int totalResults,
        int pageSize)
    {
        if (totalResults <= 0)
            return 0;

        return (int)Math.Ceiling(totalResults / (double)pageSize);
    }

    private static MovieSearchResponseDto EmptyResponse(
        MovieSearchRequestDto request)
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
}
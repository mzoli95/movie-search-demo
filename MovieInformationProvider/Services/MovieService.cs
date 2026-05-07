using Microsoft.Extensions.Caching.Memory;
using MovieInformationProvider.DTOs;
using MovieInformationProvider.DTOs.Movie;
using MovieInformationProvider.Services.Interfaces;
using System.Diagnostics;

namespace MovieInformationProvider.Services;

public class MovieService : IMovieService
{
    private readonly IEnumerable<IMovieApiClient> _movieApiClients;
    private readonly ISearchStatisticService _searchStatisticService;
    private readonly IMemoryCache _memoryCache;

    public MovieService(
        IEnumerable<IMovieApiClient> movieApiClients,
        ISearchStatisticService searchStatisticService,
        IMemoryCache memoryCache)
    {
        _movieApiClients = movieApiClients;
        _searchStatisticService = searchStatisticService;
        _memoryCache = memoryCache;
    }

    public async Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var normalizedApi = request.Api.Trim().ToLower();
        var normalizedTitle = request.MovieTitle.Trim();

        var normalizedRequest = new MovieSearchRequestDto
        {
            Api = normalizedApi,
            MovieTitle = normalizedTitle,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var cacheKey =
            $"movies:{normalizedApi}:{normalizedTitle.ToLower()}:page:{request.Page}:size:{request.PageSize}";

        var stopwatch = Stopwatch.StartNew();
        var servedFromCache = false;

        if (!_memoryCache.TryGetValue(cacheKey, out MovieSearchResponseDto? response))
        {
            var apiClient = _movieApiClients.FirstOrDefault(x =>
                x.ApiName.Equals(normalizedApi, StringComparison.OrdinalIgnoreCase));

            if (apiClient is null)
                throw new ArgumentException("Unsupported API. Allowed values: omdb, tmdb.");

            response = await apiClient.SearchMoviesAsync(
                normalizedRequest,
                cancellationToken);

            _memoryCache.Set(cacheKey, response, TimeSpan.FromMinutes(15));
        }
        else
        {
            servedFromCache = true;
        }

        stopwatch.Stop();

        await _searchStatisticService.SaveSearchAsync(
            new SaveSearchStatisticDto
            {
                Api = normalizedApi,
                Query = normalizedTitle,
                ResultCount = response!.TotalResults,
                ServedFromCache = servedFromCache,
                DurationMs = (int)stopwatch.ElapsedMilliseconds
            },
            cancellationToken);

        return response;
    }

    private static void ValidateRequest(MovieSearchRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Api))
            throw new ArgumentException("Api parameter is required.");

        if (string.IsNullOrWhiteSpace(request.MovieTitle))
            throw new ArgumentException("Movie title is required.");

        if (request.Page < 1)
            throw new ArgumentException("Page must be greater than 0.");

        if (request.PageSize < 1 || request.PageSize > 50)
            throw new ArgumentException("PageSize must be between 1 and 50.");
    }
}
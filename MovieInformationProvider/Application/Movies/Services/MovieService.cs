using MovieInformationProvider.Application.Common.Caching;
using MovieInformationProvider.Application.Movies.Caching;
using MovieInformationProvider.Application.Movies.DTOs;
using MovieInformationProvider.Application.Movies.Interfaces;
using MovieInformationProvider.Application.Movies.Validators;
using MovieInformationProvider.Application.SearchStatistics.DTOs;
using MovieInformationProvider.Application.SearchStatistics.Interfaces;
using MovieInformationProvider.Domain.Enums;
using System.Diagnostics;

namespace MovieInformationProvider.Application.Movies.Services;

public sealed class MovieService : IMovieService
{
    private readonly IEnumerable<IMovieApiClient> _movieApiClients;
    private readonly ISearchStatisticService _searchStatisticService;
    private readonly ICacheService _cacheService;

    public MovieService(
        IEnumerable<IMovieApiClient> movieApiClients,
        ISearchStatisticService searchStatisticService,
        ICacheService cacheService)
    {
        _movieApiClients = movieApiClients;
        _searchStatisticService = searchStatisticService;
        _cacheService = cacheService;
    }

    public async Task<MovieSearchResponseDto> SearchMoviesAsync(
        MovieSearchRequestDto request,
        CancellationToken cancellationToken = default)
    {
        MovieSearchRequestValidator.Validate(request);

        MovieSearchRequestDto normalizedRequest = request with
        {
            Api = request.Api,
            MovieTitle = request.MovieTitle.Trim()
        };

        string cacheKey = MovieCacheKeyBuilder.Build(normalizedRequest);

        Stopwatch stopwatch = Stopwatch.StartNew();
        bool servedFromCache = false;

        if (!_cacheService.TryGetValue(cacheKey, out MovieSearchResponseDto? response))
        {
            IMovieApiClient apiClient = GetApiClient(normalizedRequest.Api);

            response = await apiClient.SearchMoviesAsync(
                normalizedRequest,
                cancellationToken);

            _cacheService.Set(cacheKey, response, TimeSpan.FromMinutes(15));
        }
        else
        {
            servedFromCache = true;
        }

        stopwatch.Stop();

        await _searchStatisticService.SaveSearchAsync(
            new SaveSearchStatisticDto
            {
                Api = normalizedRequest.Api,
                Query = normalizedRequest.MovieTitle,
                ResultCount = response!.TotalResults,
                ServedFromCache = servedFromCache,
                DurationMs = (int)stopwatch.ElapsedMilliseconds
            },
            cancellationToken);

        return response;
    }

    private IMovieApiClient GetApiClient(MovieApiProvider api) 
    {
        return _movieApiClients.FirstOrDefault(x => x.ApiName == api)
            ?? throw new ArgumentException($"Unsupported API: {api}");
    }
}
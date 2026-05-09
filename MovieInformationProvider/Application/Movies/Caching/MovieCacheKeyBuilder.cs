using MovieInformationProvider.Application.Movies.DTOs;

namespace MovieInformationProvider.Application.Movies.Caching;

public static class MovieCacheKeyBuilder
{
    public static string Build(MovieSearchRequestDto request)
    {
        return
            $"movies:{request.Api.ToString().ToLowerInvariant()}:" +
            $"{request.MovieTitle.Trim().ToLowerInvariant()}:" +
            $"page:{request.Page}:size:{request.PageSize}";
    }
}
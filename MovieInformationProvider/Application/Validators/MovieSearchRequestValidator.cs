using MovieInformationProvider.Application.Common.Exceptions;
using MovieInformationProvider.Application.Movies.DTOs;

namespace MovieInformationProvider.Application.Movies.Validators;

public static class MovieSearchRequestValidator
{
    public static void Validate(MovieSearchRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (!Enum.IsDefined(request.Api))
        {
            errors[nameof(request.Api)] = ["Invalid API provider."];
        }

        if (string.IsNullOrWhiteSpace(request.MovieTitle))
        {
            errors[nameof(request.MovieTitle)] = ["Movie title is required."];
        }

        if (request.Page < 1)
        {
            errors[nameof(request.Page)] = ["Page must be greater than 0."];
        }

        if (request.PageSize < 1 || request.PageSize > 50)
        {
            errors[nameof(request.PageSize)] = ["PageSize must be between 1 and 50."];
        }

        if (errors.Count > 0)
        {
            throw new ValidationException("Validation failed.", errors);
        }
    }
}
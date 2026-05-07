using MovieInformationProvider.DTOs.Movie;

namespace MovieInformationProvider.Services.Interfaces;

public interface ISearchStatisticService
{
    Task SaveSearchAsync(
        SaveSearchStatisticDto dto,
        CancellationToken cancellationToken = default);
}
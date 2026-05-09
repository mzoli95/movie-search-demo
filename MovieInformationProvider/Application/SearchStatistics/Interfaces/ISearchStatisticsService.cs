using MovieInformationProvider.Application.SearchStatistics.DTOs;

namespace MovieInformationProvider.Application.SearchStatistics.Interfaces;

public interface ISearchStatisticService
{
    Task SaveSearchAsync(
        SaveSearchStatisticDto dto,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetAutocompleteAsync(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default);
}
using MovieInformationProvider.Data;
using MovieInformationProvider.Data.Common;
using MovieInformationProvider.DTOs.Movie;
using MovieInformationProvider.Services.Interfaces;

namespace MovieInformationProvider.Services;

public class SearchStatisticService : ISearchStatisticService
{
    private readonly AppDbContext _dbContext;

    public SearchStatisticService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveSearchAsync(
        SaveSearchStatisticDto dto,
        CancellationToken cancellationToken = default)
    {
        var statistic = new SearchStatistic
        {
            Api = dto.Api,
            Query = dto.Query,
            ResultCount = dto.ResultCount,
            ServedFromCache = dto.ServedFromCache,
            DurationMs = dto.DurationMs
        };

        _dbContext.SearchStatistics.Add(statistic);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
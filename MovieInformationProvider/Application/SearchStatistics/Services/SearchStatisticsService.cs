using MovieInformationProvider.Application.SearchStatistics.DTOs;
using MovieInformationProvider.Application.SearchStatistics.Interfaces;
using MovieInformationProvider.Domain.Entities;
using MovieInformationProvider.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MovieInformationProvider.Application.SearchStatistics.Services;

public class SearchStatisticService : ISearchStatisticService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<SearchStatisticService> _logger;

    public SearchStatisticService(AppDbContext dbContext, ILogger<SearchStatisticService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SaveSearchAsync(
        SaveSearchStatisticDto dto,
        CancellationToken cancellationToken = default)
    {

        _logger.LogInformation(
            "Saving search statistic: API={Api}, Query={Query}, Results={ResultCount}, Cache={ServedFromCache}, Duration={DurationMs}ms",
            dto.Api, dto.Query, dto.ResultCount, dto.ServedFromCache, dto.DurationMs);

        SearchStatistic statistic = new SearchStatistic
        {
            Api = dto.Api,
            Query = dto.Query,
            ResultCount = dto.ResultCount,
            ServedFromCache = dto.ServedFromCache,
            DurationMs = dto.DurationMs
        };

        _dbContext.SearchStatistics.Add(statistic);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogDebug("Search statistic saved successfully with ID={Id}", statistic.Id);
    }

    public async Task<List<string>> GetAutocompleteAsync(
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _logger.LogDebug("Empty query provided for autocomplete");
            return [];
        }

        string normalizedQuery = query.Trim().ToLowerInvariant();

        _logger.LogDebug("Getting autocomplete suggestions for query '{Query}' with limit {Limit}", normalizedQuery, limit);

        List<string> suggestions = await _dbContext.SearchStatistics
            .Where(s => EF.Functions.Like(s.Query.ToLower(), $"{normalizedQuery}%"))
            .GroupBy(s => s.Query)
            .Select(g => new
            {
                Query = g.Key,
                SearchCount = g.Count()
            })
            .OrderByDescending(s => s.SearchCount)
            .Take(limit)
            .Select(s => s.Query)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Found {Count} autocomplete suggestions for query '{Query}'",
            suggestions.Count, normalizedQuery);

        return suggestions;
    }
}
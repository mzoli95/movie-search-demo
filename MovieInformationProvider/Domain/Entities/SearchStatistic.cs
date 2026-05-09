using MovieInformationProvider.Domain.Common;
using MovieInformationProvider.Domain.Enums;

namespace MovieInformationProvider.Domain.Entities;

public class SearchStatistic : BaseEntity
{
    public required MovieApiProvider Api { get; set; }
    public required string Query { get; set; }
    public int ResultCount { get; set; }
    public bool ServedFromCache { get; set; }
    public int DurationMs { get; set; }
}
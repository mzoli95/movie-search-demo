namespace MovieInformationProvider.Data.Common;

public class SearchStatistic : BaseEntity
{
    public required string Api { get; set; }
    public required string Query { get; set; }
    public int ResultCount { get; set; }
    public bool ServedFromCache { get; set; }
    public int DurationMs { get; set; }
}
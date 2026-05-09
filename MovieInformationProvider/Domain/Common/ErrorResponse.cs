namespace MovieInformationProvider.Domain.Common;

public sealed record ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string TraceId { get; set; } = string.Empty;
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
    public string? StackTrace { get; set; }
}

namespace TextToSqlApi.Models.DTOs;

/// <summary>پاسخ خطای استاندارد بر اساس ProblemDetails.</summary>
public class ErrorResponse
{
    public string Type { get; set; } = "https://httpstatuses.io/500";
    public string Title { get; set; } = "An unexpected error occurred";
    public int Status { get; set; } = 500;
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>پاسخ Health Check ساده.</summary>
public class HealthResponse
{
    public string Status { get; set; } = "Healthy";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Components { get; set; } = new();
}

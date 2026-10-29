namespace SabtAhval.Models.Dtos
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public string? TraceId { get; set; }
    }
}

namespace SabtAhval.Models.Options
{
    public class SecurityOptions
    {
        string? ApiKeyHeader {  get; set; }
        int MaxLoggedBodyLength = 10000;
    }
}

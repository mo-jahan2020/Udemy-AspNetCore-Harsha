using Microsoft.AspNetCore.Http;

namespace SabtAhval.Common
{
    public static class Constants
    {
        public const string ApiKeyHeader = "Ali110@";//X-Api-Key
        public const string ApiClientItemKey = "ApiClient";

        // مسیرهایی که نباید قفل امنیتی داشته باشند
        private static readonly string[] BypassedPrefixes = { "/swagger", "/openapi", "/health" };

        public static bool IsBypassedPath(PathString path)
            => BypassedPrefixes.Any(p => path.StartsWithSegments(p));
    }
}
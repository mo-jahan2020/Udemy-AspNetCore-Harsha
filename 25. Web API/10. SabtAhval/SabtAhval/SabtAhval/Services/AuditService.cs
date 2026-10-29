using SabtAhval.Controllers;

namespace SabtAhval.Services
{
    public static class AuditService
    {
        public static async Task LogSuccessAsync(ILoggerFactory loggerFactory, string message)
        {
            var logger = loggerFactory.CreateLogger("AuditService");

            // عملیات async واقعی: نوشتن در فایل به صورت async
            // لاگ همگام در ILogger
            logger.LogInformation("Success: {Message}", message);
        }
        public static async Task LogRejectedAsync(ILoggerFactory loggerFactory, string reason)
        {
            var logger = loggerFactory.CreateLogger("AuditService");

            // عملیات async واقعی: نوشتن در فایل به صورت async
            // لاگ همگام در ILogger
            logger.LogInformation("Faild: {Message}", reason);
        }

    }
}

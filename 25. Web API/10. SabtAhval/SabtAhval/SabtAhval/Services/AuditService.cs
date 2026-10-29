using SabtAhval.Services.Interfaces;

namespace SabtAhval.Services
{
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;

        public AuditService(ILogger<AuditService> logger)
        {
            _logger = logger;
        }

        public Task LogSuccessAsync(string message)
        {
            _logger.LogInformation("Success: {Message}", message);
            return Task.CompletedTask;
        }

        public Task LogRejectedAsync(string reason)
        {
            _logger.LogInformation("Failed: {Message}", reason);
            return Task.CompletedTask;
        }
    }
}

// Services/AuditService.cs
using SabtAhval.Data;
using SabtAhval.Models.Entities;
using SabtAhval.Services.Interfaces;

namespace SabtAhval.Services
{
    public class AuditService : IAuditService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuditService> _logger;

        public AuditService(AppDbContext db, ILogger<AuditService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task LogRequestAsync(ApiRequestLog entry)
        {
            try
            {
                entry.CreatedAt = DateTime.UtcNow;   // منبع واحد حقیقت برای زمان
                _db.apiRequestLogs.Add(entry);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // قانون طلایی: شکستِ ثبت لاگ هرگز نباید درخواست کاربر را بشکند
                _logger.LogError(ex, "Audit save failed for {Method} {Path}",
                    entry.HttpMethod, entry.RequestPath);
            }
        }

        public Task LogSuccessAsync(string message)
        {
            _logger.LogInformation("Success: {Message}", message);
            return Task.CompletedTask;
        }

        public Task LogRejectedAsync(string reason)
        {
            _logger.LogWarning("Security rejection: {Reason}", reason);
            return Task.CompletedTask;
        }
    }
}
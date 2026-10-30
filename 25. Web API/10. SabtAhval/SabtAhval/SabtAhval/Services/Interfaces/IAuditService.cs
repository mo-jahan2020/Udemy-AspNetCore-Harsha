// Services/Interfaces/IAuditService.cs
using SabtAhval.Models.Entities;

namespace SabtAhval.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogRequestAsync(ApiRequestLog entry);   // ثبت کامل در دیتابیس (میان‌افزار Audit)
        Task LogSuccessAsync(string message);        // فقط لاگ Serilog
        Task LogRejectedAsync(string reason);        // فقط هشدار امنیتی Serilog
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SabtAhval.Models.Entities
{
    public class ApiRequestLog
    {
        public int Id { get; set; }

        // نال‌پذیر: درخواست‌های ردشده (401/403) هنوز کاربر شناخته‌شده ندارند
        public int? ApiClientId { get; set; }

        // ⚠️ قبلاً private بود → Include خراب می‌شد. باید public باشد!
        [ForeignKey(nameof(ApiClientId))]
        public ApiClient? ApiClient { get; set; }

        public string? Username { get; set; }        // اسنپ‌شات لحظه ثبت (حتی اگر بعداً کاربر حذف شود)
        [StringLength(64)]
        public string? ClientIp { get; set; }
        [StringLength(10)]
        public string? HttpMethod { get; set; }
        [StringLength(200)]
        public string? RequestPath { get; set; }
        [StringLength(200)]
        public string? QueryString { get; set; }
        public string? RequestBody { get; set; }     // nvarchar(max)
        public string? ResponseBody { get; set; }    // بدون محدودیت 250 تایی
        public short StatusCode { get; set; }
        public int DurationMs { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   // همیشه UTC — نه رشته شمسی!
    }
}
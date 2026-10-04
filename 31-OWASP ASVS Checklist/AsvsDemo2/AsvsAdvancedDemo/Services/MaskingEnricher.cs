using Serilog.Events;

namespace AsvsAdvancedDemo.Services {
    // بند OWASP ASVS: V7.1.2 - جلوگیری از نشت اطلاعات حساس (PII) در لاگ‌ها
    // توضیح: در محیط واقعی، این لاگ‌ها به سیستم‌های خارجی مثل Elasticsearch ارسال می‌شوند.
    // این کلاس مطمئن می‌شود که حتی اگر برنامه‌نویس فراموش کند، اطلاعات حساس ماسک شوند.
    public static class MaskingEnricher {
        public static bool MaskSensitiveData(LogEvent logEvent) {
            var message = logEvent.MessageTemplate.Text;
            // اگر پیام حاوی کلمات کلیدی حساس بود، آن را از لاگ حذف کن (یا ماسک کن)
            if (message.Contains(""Password"", System.StringComparison.OrdinalIgnoreCase) || 
                message.Contains(""CreditCard"", System.StringComparison.OrdinalIgnoreCase)) {
                return false; // false یعنی این لاگ ثبت نشود (Drop)
            }
            return true;
        }
    }
}

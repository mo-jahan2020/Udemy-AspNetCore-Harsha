using Serilog.Events;

namespace AsvsIntegratedDemo.Services {
    // بند OWASP ASVS: V7.1.2 - ماسک‌کردن اطلاعات حساس در لاگ
    public static class MaskingEnricher {
        public static bool MaskSensitiveData(LogEvent logEvent) {
            var message = logEvent.MessageTemplate.Text;
            if (message.Contains("Password", System.StringComparison.OrdinalIgnoreCase) || 
                message.Contains("CreditCard", System.StringComparison.OrdinalIgnoreCase)) {
                return false;
            }
            return true;
        }
    }
}

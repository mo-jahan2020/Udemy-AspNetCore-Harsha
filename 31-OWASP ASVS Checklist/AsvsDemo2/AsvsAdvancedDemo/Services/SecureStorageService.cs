using Microsoft.AspNetCore.DataProtection;

namespace AsvsAdvancedDemo.Services {
    // بند OWASP ASVS: V6.1.1 - رمزنگاری داده‌های حساس در حالت استراحت (Data at Rest)
    // توضیح: این سرویس شبیه‌سازی یک سیستم مدیریت کلید (KMS) خارجی را انجام می‌دهد.
    public interface ISecureStorageService {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public class SecureStorageService : ISecureStorageService {
        private readonly IDataProtector _protector;

        // IDataProtectionProvider در دات‌نت، مدیریت چرخش کلید (Key Rotation) را به صورت خودکار انجام می‌دهد
        // مشابه کاری که Azure Key Vault یا AWS KMS انجام می‌دهند.
        public SecureStorageService(IDataProtectionProvider provider) {
            // هدف (Purpose) مشخص می‌کند این رمزنگاری فقط برای همین بخش از برنامه معتبر است
            _protector = provider.CreateProtector(""AsvsAdvancedDemo.SensitiveData.v1"");
        }

        public string Encrypt(string plainText) {
            return _protector.Protect(plainText);
        }

        public string Decrypt(string cipherText) {
            return _protector.Unprotect(cipherText);
        }
    }
}

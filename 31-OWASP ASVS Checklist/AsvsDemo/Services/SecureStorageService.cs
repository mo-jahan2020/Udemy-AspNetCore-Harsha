using Microsoft.AspNetCore.DataProtection;

namespace AsvsIntegratedDemo.Services {
    // بند OWASP ASVS: V6.1.1 - رمزنگاری داده‌های حساس
    public interface ISecureStorageService {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    public class SecureStorageService : ISecureStorageService {
        private readonly IDataProtector _protector;

        public SecureStorageService(IDataProtectionProvider provider) {
            _protector = provider.CreateProtector("AsvsIntegratedDemo.SensitiveData.v1");
        }

        public string Encrypt(string plainText) => _protector.Protect(plainText);
        public string Decrypt(string cipherText) => _protector.Unprotect(cipherText);
    }
}

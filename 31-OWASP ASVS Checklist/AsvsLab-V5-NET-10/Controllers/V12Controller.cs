using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V12 — ارتباطات امن (Secure Communication)
//  اجرای واقعی TLS به گواهی و وب‌سرور نیاز دارد؛ طبق خواسته کاربر
//  این فصل متنی + پیکربندی ارائه می‌شود
// ═══════════════════════════════════════════════════════════════════
public class V12Controller : DemoBaseController
{
    public V12Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(12, "TLS و ارتباطات: نسخه‌های مجاز، Cipher Suite، اعتبارسنجی گواهی و رمزنگاری ارتباط داخلی. این فصل به دلیل نیاز به گواهی واقعی، متنی ارائه می‌شود.");

    public IActionResult TlsVersions() =>
        View("../Shared/DemoResult", TextDemo("v12-tls", "V12.1.1",
            "فقط نسخه‌های توصیه‌شده فعلی TLS (1.2 و 1.3) فعال باشند و 1.3 ترجیح داده شود.", "V12",
            "پیکربندی Kestrel در .NET 10:\n\n" +
            "builder.WebHost.ConfigureKestrel(o =>\n" +
            "{\n" +
            "    o.ConfigureEndpointDefaults(lo => lo.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1AndHttp2AndHttp3);\n" +
            "});\n" +
            "app.UseHttpsRedirection();  // در تولید\n\n" +
            "چرا TLS 1.0/1.1 ممنوع؟ پشتیبانی مرورگرها قطع شده و CBC قدیمی و RSA key exchange آسیب‌پذیر دارند.\n" +
            "بررسی واقعی: با testssl.sh یا sslyze به سرور تولیدی خود بزنید:\n" +
            "   sslyze --tlsv1_0 --tlsv1_1 example.com  → باید رد شوند."));

    public IActionResult CipherSuites() =>
        View("../Shared/DemoResult", TextDemo("v12-cipher", "V12.1.2",
            "فقط مجموعه‌های رمز توصیه‌شده فعال باشند و قوی‌ترین‌ها در اولویت.", "V12",
            "توصیه‌های کلیدی:\n" +
            "• حذف suites با RSA key exchange (استاتیک) — Forward Secrecy ندارند\n" +
            "• ترجیح: TLS_AES_256_GCM_SHA384، TLS_CHACHA20_POLY1305_SHA256، ECDHE + AES-GCM\n" +
            "• حذف 3DES، RC4، CBC-mode های قدیمی در TLS 1.2\n\n" +
            "در ویندوز (registry) یا Linux (openssl.cnf) قابل اعمال است؛ Kestrel در Linux از OpenSSL ارث می‌برد.\n" +
            "اجرای واقعی به OS-level configuration نیاز دارد؛ برای همین این بخش متنی است."));

    public IActionResult CertValidation() =>
        View("../Shared/DemoResult", TextDemo("v12-cert", "V12.3.2",
            "کلاینت‌های TLS باید گواهی سرور را قبل از ارتباط اعتبارسنجی کنند.", "V12",
            "خطرناک‌ترین الگوی دنیای .NET (در دموی واقعی اجرایش به endpoint خارجی و گواهی نیاز دارد):\n\n" +
            "// ❌ هرگز در تولید:\n" +
            "var handler = new HttpClientHandler\n" +
            "{ ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };  // MITM باز!\n\n" +
            "// ✅ درست: اعتبارسنجی پیش‌فرض باقی بماند؛ فقط برای mTLS داخلی:\n" +
            "builder.Services.AddHttpClient(\"internal\")\n" +
            "    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler\n" +
            "    {\n" +
            "        ClientCertificates = { new X509Certificate2(\"client.pfx\", pass) },\n" +
            "        // callback نگذارید! اعتبارسنجی سیستم انجام می‌شود (بند V12.1.3)\n" +
            "    });\n\n" +
            "همچنین OCSP Stapling برای ابطال به‌روز گواهی‌ها (بند V12.1.4) و ECH برای پنهان‌سازی SNI (بند V12.1.5) روی سرور تولیدی فعال کنید."));

    public IActionResult InternalTls() =>
        View("../Shared/DemoResult", TextDemo("v12-internal", "V12.3.3",
            "ارتباط بین سرویس‌های داخلی هم باید رمزنگاری شود — «شبکه داخلی امن است» یک باور خطرناک است.", "V12",
            "حمله واقعی: در شبکه‌های شرکتی با یک باگ کوچک در یک سرویس، مهاجم lateral movement می‌کند؛ ترافیک داخلی رمزنگاری‌نشده یعنی تمام توکن‌ها/داده‌ها در sniffer لو می‌رود.\n\n" +
            "راهکارها (Service Mesh / mTLS):\n" +
            "• mTLS مستقیم بین سرویس‌ها با گواهی داخلی (بند V12.3.4 — PKI داخلی مثل Cert-Manager در Kubernetes)\n" +
            "• Service Mesh مانند Istio/Linkerd: mTLS خودکار بین همه pods (بند V12.3.5 — احراز هویت قوی intra-service)\n\n" +
            "اجرای واقعی به زیرساخت چند سرویسی نیاز دارد؛ در این برنامه تک‌سرویسی، کل دیتای حساس فقط درون فرآیند جابه‌جا می‌شود."));
}

using System.Security.Cryptography;
using System.Text;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V11 — رمزنگاری (Cryptography)
//  چهار دموی اجرایی: AES-GCM، HMAC، CSPRNG و محدودیت MD5
// ═══════════════════════════════════════════════════════════════════
public class V11Controller : DemoBaseController
{
    public V11Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(11, "رمزنگاری: رمزنگاری احرازهویت‌شده (AES-GCM)، HMAC، تولید تصادفی امن و هش‌های مجاز. در 5.0 «ذخیره گذرواژه» هم به همین فصل منتقل شده (دموی V6).");

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: AES-GCM — بند V11.3.2 (رمزنگاری احرازهویت‌شده)
    // ─────────────────────────────────────────────────────────────
    public IActionResult AesGcm()
    {
        var r = New("v11-aes", "V11.3.2",
            "فقط رمزها/حالت‌های تأییدشده مثل AES با GCM (رمزنگاری احرازهویت‌شده) استفاده شوند.", "V11");
        r.VulnerableCode = """
            // ❌ ناامن: AES-CBC بدون MAC (حمله Padding Oracle) یا ECB
            var iv = new byte[16]; // IV تکراری!
            using var aes = Aes.Create();
            aes.Mode = CipherMode.ECB; // الگوها لو می‌روند
            """;
        r.SecureCode = """
            // ✅ امن: AES-GCM با nonce ۱۲ بایتی یگانه + tag ۱۶ بایتی
            var nonce = RandomNumberGenerator.GetBytes(12);
            using var gcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
            gcm.Encrypt(nonce, plaintext, ciphertext, tag);
            // دستکاری ciphertext → پرتاب CryptographicException هنگام رمزگشایی
            """;

        var key = RandomNumberGenerator.GetBytes(32);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plain = Encoding.UTF8.GetBytes("کارت بانکی: 6037-9911-****-1234");
        var cipher = new byte[plain.Length];
        var tag = new byte[16];

        using (var gcm = new AesGcm(key, 16))
            gcm.Encrypt(nonce, plain, cipher, tag);

        var decrypted = new byte[plain.Length];
        using (var gcm = new AesGcm(key, 16))
        {
            gcm.Decrypt(nonce, cipher, tag, decrypted);
        }
        var okText = Encoding.UTF8.GetString(decrypted);

        // دستکاری ciphertext → شکست
        cipher[0] ^= 0xFF;
        string tamperResult;
        try
        {
            using var gcm = new AesGcm(key, 16);
            gcm.Decrypt(nonce, cipher, tag, new byte[plain.Length]);
            tamperResult = "رمزگشایی شد (هرگز نباید رخ دهد!)";
        }
        catch (CryptographicException) { tamperResult = "🚫 پرتاب CryptographicException — دستکاری کشف شد"; }

        r.OutputLines.Add($"متن اصلی: {Encoding.UTF8.GetString(plain)}");
        r.OutputLines.Add($"رمزشده (hex): {Convert.ToHexString(cipher)[..32]}… | Tag: {Convert.ToHexString(tag)[..16]}…");
        r.OutputLines.Add($"رمزگشایی درست: ✅ «{okText}»");
        r.OutputLines.Add($"رمزگشایی بعد از دستکاری یک بایت: {tamperResult}");
        r.OutputLines.Add("تفاوت GCM با CBC: در GCM دستکاری داده محرمانه را «رد» می‌کند؛ در CBC بدون MAC ممکن است داده خراب پذیرفته شود.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: HMAC — بند V11.4.1 (هش تأییدشده برای HMAC)
    // ─────────────────────────────────────────────────────────────
    public IActionResult HmacSign()
    {
        var r = New("v11-hmac", "V11.4.1",
            "برای HMAC فقط توابع هش تأییدشده (SHA-256 و بالاتر) مجازند.", "V11");
        r.VulnerableCode = """
            // ❌ ناامن: امضا با MD5 یا بدون کلید
            var sig = Convert.ToHexString(MD5.HashData(message));
            // مهاجم می‌تواند پیام + امضای جدید با هم بسازد
            """;
        r.SecureCode = """
            // ✅ امن: HMAC-SHA256 با کلید مخفی
            using var hmac = new HMACSHA256(secretKey);
            var sig = hmac.ComputeHash(message);
            // مقایسه با CryptographicOperations.FixedTimeEquals
            """;
        var key = RandomNumberGenerator.GetBytes(32);
        var msg = Encoding.UTF8.GetBytes("amount=1000000&to=ali&ts=2026-09-09");
        string Sign(byte[] m) { using var h = new HMACSHA256(key); return Convert.ToHexString(h.ComputeHash(m)); }
        var sig = Sign(msg);
        var tampered = Encoding.UTF8.GetBytes("amount=9999999&to=ali&ts=2026-09-09");

        r.OutputLines.Add($"پیام: {Encoding.UTF8.GetString(msg)}");
        r.OutputLines.Add($"امضای HMAC-SHA256: {sig}");
        r.OutputLines.Add($"پیام دستکاری‌شده (مبلغ تغییر کرد): {Encoding.UTF8.GetString(tampered)}");
        r.OutputLines.Add($"بررسی امضای پیام دستکاری‌شده: {(Sign(tampered) == sig ? "✅ معتبر" : "🚫 نامعتبر — پیام رد شد")}");
        r.OutputLines.Add("کاربرد واقعی: امضای Webhook (مثل GitHub/Stripe) و درخواست‌های بانکی.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۳: CSPRNG — بند V11.5.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult SecureRandom()
    {
        var r = New("v11-random", "V11.5.1",
            "مقادیر غیرقابل حدس باید با CSPRNG تولید شوند، نه مولدهای معمولی.", "V11");
        r.VulnerableCode = """
            // ❌ ناامن: Random معمولی برای توکن بازیابی رمز
            var rnd = new Random();                    // seed = زمان سیستم!
            var token = rnd.Next(100000, 999999);
            // با دانلود چند توکن، seed قابل پیش‌بینی می‌شود
            """;
        r.SecureCode = """
            // ✅ امن: RandomNumberGenerator (CSPRNG سیستم‌عامل)
            var token = RandomNumberGenerator.GetHexString(16); // ۱۶ کاراکتر امن
            """;
        var rnd = new Random(42); // seed ثابت برای نمایش پیش‌بینی‌پذیری
        r.OutputLines.Add($"Random با seed ثابت ۴۲، سه خروجی پشت‌سرهم: {rnd.Next():N0}، {rnd.Next():N0}، {rnd.Next():N0}");
        r.OutputLines.Add("← هر کسی با همان seed دقیقاً همین اعداد را می‌گیرد! (قابل پیش‌بینی)");
        r.OutputLines.Add($"CSPRNG اول: {Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}");
        r.OutputLines.Add($"CSPRNG دوم: {Convert.ToHexString(RandomNumberGenerator.GetBytes(16))}");
        r.OutputLines.Add("CSPRNG از entropy سیستم‌عامل تغذیه می‌شود و قابل پیش‌بینی نیست.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۴: ممنوعیت MD5 — بند V11.4.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult Md5Forbidden()
    {
        var r = New("v11-md5", "V11.4.1",
            "هش‌های ممنوعه (MD5 و...) برای کاربردهای رمزنگاری استفاده نمی‌شوند.", "V11");
        var data = Encoding.UTF8.GetBytes("فایل قرارداد شماره ۱۰۲۵");
        var md5 = Convert.ToHexString(MD5.HashData(data));
        var sha = Convert.ToHexString(SHA256.HashData(data));
        r.OutputLines.Add($"داده: {Encoding.UTF8.GetString(data)}");
        r.OutputLines.Add($"MD5 (ممنوع — ۱۲۸ بیت، برخوردها هم‌سطح روز):  {md5}");
        r.OutputLines.Add($"SHA-256 (مجاز): {sha}");
        r.OutputLines.Add("چرا MD5 ممنوع؟ حمله برخورد با لپ‌تاپ معمولی در چند دقیقه ممکن است (فاجعه Flashback/Flame ۲۰۱۲).");
        r.OutputLines.Add("کاربرد مجاز MD5: چک‌سام غیرامنیتی (مثل بررسی انتقال فایل) — نه امضا/گواهی/HMAC/KDF.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: مدیریت کلید — بندهای V11.1.1 و V11.2.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult KeyManagement() =>
        View("../Shared/DemoResult", TextDemo("v11-keymgmt", "V11.1.1,V11.2.1",
            "سیاست مدیریت چرخه عمر کلید طبق استانداردی مثل NIST SP 800-57 مستند شود و از پیاده‌سازی‌های معتبر استفاده شود.", "V11",
            "چرخه عمر کلید باید شامل: تولید، توزیع، نگهداری، چرخش، آرشیو و ابطال باشد.\n\n" +
            "خطاهای رایج سازمان‌ها:\n" +
            "• کلید هاردکد در سورس‌کد (بندهای V13.3.1 را ببینید)\n" +
            "• یک کلید برای همه کاربردها (امضا + رمزنگاری + KDF)\n" +
            "• بدون برنامه چرخش — کلید ۱۰ ساله!\n" +
            "• پیاده‌سازی دستی الگوریتم‌ها به‌جای کتابخانه معتبر (بند V11.2.1)\n\n" +
            "در .NET: DataProtection (این برنامه در V14 استفاده کرده)، Azure Key Vault / HashiCorp Vault برای تولید و چرخش.\n" +
            "اجرای واقعی چرخش کلید به زیرساخت خارجی نیاز دارد؛ برای همین متنی ارائه شد."));
}

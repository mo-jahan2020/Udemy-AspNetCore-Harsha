using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V9 — توکن‌های خودبسنده (Self-contained Tokens / JWT)
//  JWT اینجا «دست‌ساز» با HMAC-SHA256 ساخته می‌شود تا همه اجزا دیده شوند
// ═══════════════════════════════════════════════════════════════════
public class V9Controller : DemoBaseController
{
    private const string Secret = "demo-secret-key-never-use-in-production-2026!";

    public V9Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(9, "JWT و توکن‌های مشابه: امضا، فهرست مجاز الگوریتم و منبع کلید. سه دموی اجرایی + توضیح متنی.");

    private static string B64(byte[] b) => Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    private static string B64Json<T>(T o) => B64(JsonSerializer.SerializeToUtf8Bytes(o));
    private static byte[] Sign(string data) => new HMACSHA256(Encoding.UTF8.GetBytes(Secret)).ComputeHash(Encoding.UTF8.GetBytes(data));

    // ─────────────────────────────────────────────────────────────
    // دموی ۱: ساخت و اعتبارسنجی JWT — بند V9.1.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult JwtCreate()
    {
        var r = New("v9-jwt", "V9.1.1",
            "قبل از پذیرش محتوای توکن خودبسنده، امضا/MAC آن اعتبارسنجی می‌شود.", "V9");
        r.VulnerableCode = """
            // ❌ ناامن: خواندن payload بدون بررسی امضا
            var payload = Base64Decode(token.Split('.')[1]);
            var role = payload.role; // مهاجم هرچه بخواهد می‌گذارد!
            """;
        r.SecureCode = """
            // ✅ امن: اول امضا، بعد محتوا
            var expected = HMAC_SHA256(secret, header + "." + payload);
            if (!FixedTimeEquals(expected, signature)) return Unauthorized();
            var claims = Decode(payload); // حالا قابل اعتماد است
            """;

        var header = new { alg = "HS256", typ = "JWT" };
        var payload = new { sub = "ali", role = "User", exp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600 };
        var h = B64Json(header); var p = B64Json(payload);
        var token = $"{h}.{p}.{B64(Sign($"{h}.{p}"))}";

        var parts = token.Split('.');
        var expectedSig = B64(Sign($"{parts[0]}.{parts[1]}"));
        r.OutputLines.Add($"Header:  {parts[0]} → {Encoding.UTF8.GetString(FromB64(parts[0]))}");
        r.OutputLines.Add($"Payload: {parts[1]} → {Encoding.UTF8.GetString(FromB64(parts[1]))}");
        r.OutputLines.Add($"Signature: {parts[2]}");
        r.OutputLines.Add($"اعتبارسنجی امضا با کلید سرور: {(expectedSig == parts[2] ? "✅ معتبر" : "🚫 نامعتبر")}");
        r.OutputLines.Add("نکته: توکن فقط base64 است؛ رمزنگاری نیست — هرکس محتوا را می‌بیند؛ امضا فقط «تغییرناپذیری» می‌دهد.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۲: دستکاری payload — بند V9.1.1
    // ─────────────────────────────────────────────────────────────
    public IActionResult JwtTamper()
    {
        var r = New("v9-tamper", "V9.1.1",
            "تغییر حتی یک کاراکتر در payload بدون کلید، امضا را نامعتبر می‌کند.", "V9");
        var header = new { alg = "HS256", typ = "JWT" };
        var payload = new { sub = "ali", role = "User", exp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600 };
        var h = B64Json(header); var p = B64Json(payload);
        var original = $"{h}.{p}.{B64(Sign($"{h}.{p}"))}";

        // مهاجم role را Admin می‌کند (بدون کلید!)
        var evilPayload = B64Json(new { sub = "ali", role = "Admin", exp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600 });
        var evilToken = $"{h}.{evilPayload}.{B64(Sign($"{h}.{p}"))}"; // امضای قدیمی حفظ شده

        var parts = evilToken.Split('.');
        var expected = B64(Sign($"{parts[0]}.{parts[1]}"));
        r.OutputLines.Add($"payload اصلی: {Encoding.UTF8.GetString(FromB64(p))}");
        r.OutputLines.Add($"payload دستکاری‌شده: {Encoding.UTF8.GetString(FromB64(evilPayload))}  ← مهاجم خودش را ادمین کرد!");
        r.OutputLines.Add($"بررسی امضای توکن دستکاری‌شده: {(expected == parts[2] ? "✅ معتبر" : "🚫 نامعتبر — حمله خنثی شد")}");
        r.OutputLines.Add("نتیجه: server باید توکن را رد کند؛ role=Admin هرگز پذیرفته نمی‌شود.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی ۳: حمله alg=none — بند V9.1.2
    // ─────────────────────────────────────────────────────────────
    public IActionResult AlgNone()
    {
        var r = New("v9-alg", "V9.1.2",
            "فقط الگوریتم‌های فهرست مجاز سرور برای ساخت/اعتبارسنجی توکن پذیرفته می‌شوند.", "V9");
        r.VulnerableCode = """
            // ❌ ناامن: اعتماد به هدر توکن برای انتخاب الگوریتم
            var alg = header.alg;            // مهاجم "none" می‌گذارد!
            if (alg == "none") return DecodePayload(token); // فاجعه
            """;
        r.SecureCode = """
            // ✅ امن: فهرست مجاز سرور، نه هدر مهاجم
            var allowed = new[] { "HS256", "RS256" };
            if (!allowed.Contains(header.alg)) return Unauthorized();
            """;
        var evilHeader = B64Json(new { alg = "none", typ = "JWT" });
        var evilPayload = B64Json(new { sub = "ali", role = "Admin" });
        var evilToken = $"{evilHeader}.{evilPayload}."; // بدون امضا

        var parts = evilToken.Split('.');
        var headerAlg = JsonSerializer.Deserialize<JsonElement>(Encoding.UTF8.GetString(FromB64(parts[0]))).GetProperty("alg").GetString();
        var allowed = new[] { "HS256", "RS256" };
        r.OutputLines.Add($"توکن مهاجم: {evilToken}");
        r.OutputLines.Add($"الگوریتم ادعایی هدر: {headerAlg}");
        r.OutputLines.Add($"در فهرست مجاز سرور ({string.Join("، ", allowed)})؟ {(allowed.Contains(headerAlg!) ? "بله" : "خیر → 🚫 توکن رد شد (Unauthorized)")}");
        r.OutputLines.Add("این حمله معروف CVE-2016-10555 در کتابخانه‌های قدیمی JWT بود.");
        return View("../Shared/DemoResult", r);
    }

    // ─────────────────────────────────────────────────────────────
    // دموی متنی: منبع کلید — بند V9.1.3
    // ─────────────────────────────────────────────────────────────
    public IActionResult KeySource() =>
        View("../Shared/DemoResult", TextDemo("v9-key", "V9.1.3",
            "کلید اعتبارسنجی توکن فقط از منابع پیکربندی‌شده مورد اعتماد صادرکننده بارگذاری شود.", "V9",
            "خطر اصلی: اگر کلید/کلید عمومی از «ورودی» خوانده شود (مثلاً هدر jwk خود توکن!) مهاجم با کلید خودش توکن امضا می‌کند و سرور آن را معتبر می‌پندارد (حمله JKU/X5U).\n\n" +
            "الگوی درست:\n" +
            "• HS256: کلید متقارن از Vault/پیکربندی امن سرور (بند V13.3.1)\n" +
            "• RS256: کلید عمومی صادرکننده از آدرس پیکربندی‌شده مثل JWKS endpoint شناخته‌شده\n" +
            "• هرگز کلید را از هدر توکن نخوانید.\n\n" +
            "در این دموها کلید demo ثابت در کد است و فقط آموزشی است — در تولید از مدیریت اسرار استفاده کنید (فصل V13 را ببینید)."));

    private static byte[] FromB64(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(s + new string('=', (4 - s.Length % 4) % 4));
    }
}

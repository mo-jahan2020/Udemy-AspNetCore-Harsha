using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  فصل V17 — امنیت WebRTC
//  اجرای واقعی به مرورگر + سرور Signaling/TURN نیاز دارد؛ متنی ارائه می‌شود
// ═══════════════════════════════════════════════════════════════════
public class V17Controller : DemoBaseController
{
    public V17Controller(AsvsRefService asvs, ProgressService progress) : base(asvs, progress) { }

    public IActionResult Index() =>
        SectionIndex(17, "WebRTC: کنترل TURN و مقاومت سرور Signaling. به دلیل نیاز به زیرساخت رسانه‌ای، دموهای این فصل متنی هستند.");

    public IActionResult Overview() =>
        View("../Shared/DemoResult", TextDemo("v17-overview", "V17.1.1",
            "سرویس TURN فقط به IPهای غیررزرو (خارج از شبکه داخلی/loopback) دسترسی بدهد.", "V17",
            "چرا مهم است؟ TURN یک «proxy کامل UDP/TCP» است؛ اگر مهاجم بتواند TURN را به آدرس‌های داخلی بچرخاند، " +
            "دقیقاً مثل یک SSRF کامل به شبکه داخلی شما (database.internal:5432 و...) پورت‌اسکن و اتصال می‌کند.\n\n" +
            "الگوی پیکربندی coturn:\n" +
            "   # بلوکه کردن IPهای رزرو:\n" +
            "   denied-peer-ip=10.0.0.0-10.255.255.255\n" +
            "   denied-peer-ip=127.0.0.0-127.255.255.255\n" +
            "   denied-peer-ip=169.254.0.0-169.254.255.255\n" +
            "   denied-peer-ip=192.168.0.0-192.168.255.255\n\n" +
            "اجرای واقعی به سرور TURN و کلاینت WebRTC نیاز دارد؛ این فصل (۱۲ بند) متنی ارائه شد.\n" +
            "بندهای تکمیلی: رسانه‌ها باید با DTLS-SRTP رمز شوند و ICE candidate های داخلی به سمت کلاینت نامطمئن لو نروند."));

    public IActionResult Signaling() =>
        View("../Shared/DemoResult", TextDemo("v17-signal", "V17.3.1",
            "سرور Signaling باید با Rate Limiting بتواند در برابر طغیان پیام، به مشتریان سالم ادامه دهد.", "V17",
            "سناریو: مهاجم با هزاران اتصال WebSocket به سرور signaling، پیام SDP حجیم پمپ می‌کند → CPU/حافظه تمام → " +
            "تماس‌های کاربران سالم قطع می‌شود (DoS).\n\n" +
            "کنترل‌ها:\n" +
            "• Rate limit پیام در هر اتصال و هر IP (همان الگوی دموی V2.4.1)\n" +
            "• سقف اندازه پیام SDP (مثلاً ۱۶KB) و قطع اتصال متخلف\n" +
            "• احراز هویت الزامی پیش از پذیرش هر پیام signaling\n" +
            "• صف و Backpressure برای حفظ سرویس کاربران سالم\n\n" +
            "در ASP.NET Core: AddRateLimiter + میان‌افزار WebSocket با AllowedOrigins (بند V4.4.2) دقیقاً همین نقش را دارد؛ " +
            "اجرای کامل به استقرار سرور WebRTC واقعی نیاز دارد."));
}

using AsvsLab.Models;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  کنترلر صفحه اصلی — داشبورد با جستجو + پیشرفت مطالعه + صفحات خطا
// ═══════════════════════════════════════════════════════════════════
public class HomeController : Controller
{
    private readonly AsvsRefService _asvs;
    private readonly ProgressService _progress;

    public HomeController(AsvsRefService asvs, ProgressService progress)
    {
        _asvs = asvs;
        _progress = progress;
    }

    /// <summary>داشبورد: ۱۷ فصل ASVS 5.0 با امکان جستجو (متن/شماره بند) و نوار پیشرفت</summary>
    public IActionResult Index(string? q)
    {
        var visited = _progress.Visited();
        var query = q?.Trim();

        var groups = Enumerable.Range(1, 17).Select(ch =>
        {
            var (fa, en) = _asvs.Chapter(ch);
            var items = DemoCatalog.ForChapter(ch);
            // جستجو در عنوان فارسی، شماره بند و شناسه دمو
            if (!string.IsNullOrEmpty(query))
                items = items.Where(d =>
                    d.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    d.AsvsRefs.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    d.Id.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            return new ChapterGroup
            {
                Chapter = ch,
                FaTitle = fa,
                EnTitle = en,
                OfficialReqCount = _asvs.CountOf(ch),
                Items = items
            };
        }).Where(g => g.Items.Count > 0).ToList();

        var quizPassed = _progress.QuizScores().Count(kv => kv.Value >= 60);

        return View(new DashboardViewModel
        {
            Chapters = groups,
            Query = query,
            Visited = visited,
            VisitedCount = visited.Count(v => DemoCatalog.Find(v) is not null),
            TotalCount = DemoCatalog.Total,
            QuizPassedCount = quizPassed
        });
    }

    // ─── بند V16.5.1 — صفحه خطای عمومی: هیچ جزئیات فنی (Stack Trace، کوئری، مسیر) افشا نمی‌شود ───
    public IActionResult Error() => View("../Shared/Error", "خطای غیرمنتظره‌ای رخ داد. لطفاً بعداً دوباره تلاش کنید.");

    // ─── بند V16.5.1 — پیام‌های عمومی برای کدهای وضعیت HTTP ───
    public IActionResult StatusCode(int? id)
    {
        var msg = id switch
        {
            404 => "صفحه مورد نظر پیدا نشد (404).",
            400 => "درخواست نامعتبر است (400).",
            401 => "برای دسترسی ابتدا وارد شوید (401).",
            403 => "شما مجوز دسترسی به این بخش را ندارید (403).",
            500 => "خطای سرور رخ داد (500) — جزئیات فنی عمداً پنهان شده است.",
            _ => $"پاسخ با کد وضعیت {id}."
        };
        return View("../Shared/Error", msg);
    }

    /// <summary>بازنشانی پیشرفت مطالعه (سشن کاربر)</summary>
    public IActionResult ResetProgress()
    {
        _progress.Reset();
        return RedirectToAction("Index");
    }
}

using AsvsLab.Models;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  کنترلر پایه همه دموها — کد مشترک برای ساخت نتیجه و ثبت پیشرفت
//  (برای خوانایی بیشتر، هر فصل ASVS یک کنترلر مجزا دارد)
// ═══════════════════════════════════════════════════════════════════
public abstract class DemoBaseController : Controller
{
    protected readonly AsvsRefService Asvs;
    protected readonly ProgressService Progress;

    protected DemoBaseController(AsvsRefService asvs, ProgressService progress)
    {
        Asvs = asvs;
        Progress = progress;
    }

    /// <summary>ساخت DemoResult + ثبت «دیده شدن» دمو در پیشرفت مطالعه</summary>
    protected DemoResult New(string id, string refs, string fa, string backController)
    {
        Progress.Mark(id); // ثبت پیشرفت مطالعه کاربر (در سشن)
        var item = DemoCatalog.Find(id);
        return new DemoResult
        {
            DemoId = id,
            Title = item?.Title ?? id,
            AsvsRef = refs,
            AsvsFa = fa,
            AsvsSummaryEn = Asvs.Official(refs),
            BackController = backController
        };
    }

    /// <summary>ساخت DemoResult متنی (برای بندهایی که اجرای واقعی ندارند)</summary>
    protected DemoResult TextDemo(string id, string refs, string fa, string backController, string notes)
    {
        var r = New(id, refs, fa, backController);
        r.Notes = notes;
        return r;
    }

    /// <summary>ساخت مدل فرم دمو (GET فرم‌های ورودی)</summary>
    protected IActionResult FormView(string id, string fieldName, string label, string placeholder,
        bool isUpload = false, string hint = "")
    {
        Progress.Mark(id);
        var item = DemoCatalog.Find(id);
        var refs = item?.AsvsRefs ?? "";
        return View("../Shared/DemoForm", new DemoFormViewModel
        {
            Title = item?.Title ?? id,
            AsvsRef = refs,
            AsvsSummaryEn = Asvs.Official(refs),
            Action = Url.Action(item?.Action, item?.Controller) ?? "/",
            FieldName = fieldName,
            FieldLabel = label,
            Placeholder = placeholder,
            Hint = hint,
            IsUpload = isUpload
        });
    }

    /// <summary>صفحه فهرست دموهای یک فصل — همه کنترلرهای فصل از این استفاده می‌کنند</summary>
    protected IActionResult SectionIndex(int chapter, string summary)
    {
        var (fa, en) = Asvs.Chapter(chapter);
        var scores = Progress.QuizScores();
        return View("../Shared/SectionIndex", new SectionIndexViewModel
        {
            Chapter = chapter,
            FaTitle = fa,
            EnTitle = en,
            ChapterSummary = summary,
            Items = DemoCatalog.ForChapter(chapter),
            QuizScore = scores.TryGetValue(chapter, out var s) ? s : -1
        });
    }
}

using AsvsLab.Models;
using AsvsLab.Services;
using Microsoft.AspNetCore.Mvc;

namespace AsvsLab.Controllers;

// ═══════════════════════════════════════════════════════════════════
//  کنترلر کوییز (امکان جدید نسخه ۲)
//  برای هر فصل ۲ سوال چهارگزینه‌ای؛ نمره در سشن ذخیره می‌شود
//  نمره ≥ ۶۰٪ یعنی فصل «قبول» (در داشبورد شمارش می‌شود)
// ═══════════════════════════════════════════════════════════════════
public class QuizController : Controller
{
    private readonly AsvsRefService _asvs;
    private readonly ProgressService _progress;

    public QuizController(AsvsRefService asvs, ProgressService progress)
    {
        _asvs = asvs;
        _progress = progress;
    }

    /// <summary>فهرست فصل‌ها + بهترین نمره هر کدام</summary>
    public IActionResult Index()
    {
        var scores = _progress.QuizScores();
        var model = Enumerable.Range(1, 17).Select(ch =>
        {
            var (fa, en) = _asvs.Chapter(ch);
            return new QuizChapterRow
            {
                Chapter = ch,
                FaTitle = fa,
                EnTitle = en,
                QuestionCount = QuizBank.ForChapter(ch).Count,
                BestScore = scores.TryGetValue(ch, out var s) ? s : -1
            };
        }).ToList();
        return View(model);
    }

    [HttpGet]
    public IActionResult Chapter(int ch)
    {
        if (ch is < 1 or > 17) return RedirectToAction("Index");
        var questions = QuizBank.ForChapter(ch);
        if (questions.Count == 0) return RedirectToAction("Index");
        return View(new QuizChapterViewModel
        {
            Chapter = ch,
            FaTitle = _asvs.Chapter(ch).Fa,
            Questions = questions,
            AlreadyGraded = false
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Chapter(int ch, IFormCollection form)
    {
        if (ch is < 1 or > 17) return RedirectToAction("Index");
        var questions = QuizBank.ForChapter(ch);
        if (questions.Count == 0) return RedirectToAction("Index");

        var vm = new QuizChapterViewModel
        {
            Chapter = ch,
            FaTitle = _asvs.Chapter(ch).Fa,
            Questions = questions,
            AlreadyGraded = true
        };

        var correct = 0;
        foreach (var (q, idx) in questions.Select((q, i) => (q, i)))
        {
            var answer = form[$"q{idx}"].ToString();
            var chosen = int.TryParse(answer, out var a) ? a : -1;
            var ok = chosen == q.Correct;
            if (ok) correct++;
            vm.Results.Add(new QuizAnswerResult(idx, chosen, ok, q));
        }

        vm.Percent = (int)Math.Round(100.0 * correct / questions.Count);
        _progress.SetQuizScore(ch, vm.Percent); // بهترین نمره حفظ می‌شود

        return View(vm);
    }

    /// <summary>بازنشانی همه پیشرفت‌ها (دمو + کوییز)</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Reset()
    {
        _progress.Reset();
        return RedirectToAction("Index");
    }
}

public class QuizChapterRow
{
    public int Chapter { get; set; }
    public string FaTitle { get; set; } = "";
    public string EnTitle { get; set; } = "";
    public int QuestionCount { get; set; }
    public int BestScore { get; set; } = -1;
}

public class QuizChapterViewModel
{
    public int Chapter { get; set; }
    public string FaTitle { get; set; } = "";
    public List<QuizQuestion> Questions { get; set; } = new();
    public bool AlreadyGraded { get; set; }
    public int Percent { get; set; }
    public List<QuizAnswerResult> Results { get; set; } = new();
}

public record QuizAnswerResult(int Index, int Chosen, bool Correct, QuizQuestion Q);

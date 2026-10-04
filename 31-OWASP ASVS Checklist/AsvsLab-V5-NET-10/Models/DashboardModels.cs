using AsvsLab.Models;

namespace AsvsLab.Services;

/// <summary>مدل داشبورد اصلی — گروه‌بندی دموها بر اساس ۱۷ فصل + وضعیت پیشرفت</summary>
public class DashboardViewModel
{
    public List<ChapterGroup> Chapters { get; set; } = new();
    public string? Query { get; set; }
    public int VisitedCount { get; set; }
    public int TotalCount { get; set; }
    public int QuizPassedCount { get; set; }
    public HashSet<string> Visited { get; set; } = new();
}

public class ChapterGroup
{
    public int Chapter { get; set; }
    public string FaTitle { get; set; } = "";
    public string EnTitle { get; set; } = "";
    public int OfficialReqCount { get; set; }      // تعداد بندهای رسمی فصل در ASVS 5.0
    public List<DemoItem> Items { get; set; } = new();
}

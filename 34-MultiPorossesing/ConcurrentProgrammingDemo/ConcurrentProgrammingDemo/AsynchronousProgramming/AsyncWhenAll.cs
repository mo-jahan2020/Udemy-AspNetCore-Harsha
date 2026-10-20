// ============================================================================
//  AsyncWhenAll.cs - اجرای همزمان چند Task و انتظار برای همگی
// ============================================================================
//  Task.WhenAll یک «Combinator» (ترکیب‌گر) است که چند Task را به یک Task
//  تبدیل می‌کند که پس از تکمیل همه آن‌ها کامل می‌شود.
//
//  ویژگی‌ها:
//   - اگر هر Task استثنا پرتاب کند، AggregateException دریافت می‌کنید.
//   - مقادیر بازگشتی به صورت آرایه (T[]) به دست می‌آیند.
//   - بهترین گزینه برای «Fan-Out/Fan-In»: همزمان راه‌اندازی چند کار و
//     سپس یکجا منتظر ماندن.
// ============================================================================

namespace ConcurrentProgrammingDemo.AsynchronousProgramming;

public static class AsyncWhenAll
{
    // --------------------------------------------------------------------------
    //  RunAsync: سناریوی دانلود همزمان چند URL
    // --------------------------------------------------------------------------
    public static async Task RunAsync()
    {
        Console.WriteLine("  [WhenAll] شبیه‌سازی دانلود همزمان 5 منبع");

        string[] urls = { "url1", "url2", "url3", "url4", "url5" };

        var sw = System.Diagnostics.Stopwatch.StartNew();

        // روش ۱: راه‌اندازی همه Taskها بدون انتظار (Fan-Out)
        Task<string>[] tasks = urls.Select(DownloadAsync).ToArray();

        // روش ۲: انتظار یکجا برای همگی (Fan-In)
        //  نکته: Task.WhenAll تنها یک بار منتظر می‌ماند، نه پنج بار.
        string[] contents = await Task.WhenAll(tasks);

        sw.Stop();

        for (int i = 0; i < contents.Length; i++)
        {
            Console.WriteLine($"  [WhenAll] {urls[i]} → {contents[i]}");
        }

        Console.WriteLine($"  [WhenAll] مجموع زمان: {sw.ElapsedMilliseconds} ms (به جای 5x تأخیر هر کار)");
    }

    // --------------------------------------------------------------------------
    //  DownloadAsync: شبیه‌سازی یک عملیات I/O (مانند HttpClient.GetAsync)
    // --------------------------------------------------------------------------
    //  نوع بازگشتی Task<string> یعنی «رشته‌ای در آینده».
    //  Task.Delay شبیه‌سازی تأخیر شبکه است؛ در پروژه واقعی از
    //  HttpClient.GetAsync یا FileStream.ReadAsync استفاده می‌شود.
    // --------------------------------------------------------------------------
    private static async Task<string> DownloadAsync(string url)
    {
        Random rnd = new(url.GetHashCode());
        int delay = rnd.Next(300, 800); // تأخیر تصادفی شبیه به شبکه واقعی

        await Task.Delay(delay);
        return $"محتوای {url} ({delay}ms)";
    }

    // --------------------------------------------------------------------------
    //  HandleExceptionsAsync: مدیریت استثناها در Task.WhenAll
    // --------------------------------------------------------------------------
    //  اگر چند Task استثنا پرتاب کنند، Task.WhenAll همه استثناها را در
    //  AggregateException جمع می‌کند. باید InnerExceptions را پیمود.
    // --------------------------------------------------------------------------
    public static async Task HandleExceptionsAsync()
    {
        var tasks = new[]
        {
            Task.Run(() => throw new InvalidOperationException("خطای 1")),
            Task.Run(() => throw new FormatException("خطای 2"))
        };

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex) when (ex is AggregateException ae)
        {
            Console.WriteLine($"  [WhenAll] {ae.InnerExceptions.Count} خطا جمع‌آوری شد");
            foreach (var inner in ae.InnerExceptions)
                Console.WriteLine($"    - {inner.GetType().Name}: {inner.Message}");
        }
    }
}

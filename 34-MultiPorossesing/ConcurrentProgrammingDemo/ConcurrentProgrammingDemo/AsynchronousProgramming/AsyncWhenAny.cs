// ============================================================================
//  AsyncWhenAny.cs - اجرای همزمان و واکنش به نخستین Task تکمیل‌شده
// ============================================================================
//  Task.WhenAny یک Combinator است که یک Task<Task> برمی‌گرداند: به محض
//  اینکه هر یک از Taskها تکمیل شود، خودش تکمیل می‌شود.
//
//  کاربردها:
//   - «Redundancy»: چند منبع را همزمان پرس‌وجو کن و نخستین پاسخ را برگردان.
//   - «Timeout»: یک کار اصلی و یک Task.Delay؛ هرکدام زودتر تمام شد.
//   - «Race»: رقابت چند کار و استفاده از برنده.
//
//  نکته: Taskهای بازمانده همچنان در پس‌زمینه اجرا می‌شوند؛ مدیریت
//  این موضوع (لغو یا نادیده گرفتن) بر عهده برنامه‌نویس است.
// ============================================================================

namespace ConcurrentProgrammingDemo.AsynchronousProgramming;

public static class AsyncWhenAny
{
    // --------------------------------------------------------------------------
    //  RunAsync: رقابت چند سرور و استفاده از پاسخ سریع‌ترین
    // --------------------------------------------------------------------------
    public static async Task RunAsync()
    {
        Console.WriteLine("  [WhenAny] رقابت 3 سرور و انتخاب سریع‌ترین پاسخ");

        string[] servers = { "Server-A", "Server-B", "Server-C" };

        // همزمان همه را شروع کن
        Task<string>[] tasks = servers.Select(QueryServerAsync).ToArray();

        // تا نخستین Task تکمیل شود منتظر بمان
        Task<string> winner = await Task.WhenAny(tasks);

        // حالا result از Task برنده را بخوان
        string result = await winner;
        Console.WriteLine($"  [WhenAny] برنده: {result}");

        //  نکته مهم: بقیه Taskها هنوز اجرا می‌شوند! در سناریوی واقعی
        //  باید با CancellationToken آن‌ها را لغو کنید (مثال بعدی).
    }

    // --------------------------------------------------------------------------
    //  RunWithTimeoutAsync: اجرای یک کار با محدودیت زمانی
    // --------------------------------------------------------------------------
    //  الگو: یک Task کار اصلی + یک Task.Delay؛ هرکدام زودتر تمام شد، برنده است.
    // --------------------------------------------------------------------------
    public static async Task RunWithTimeoutAsync()
    {
        Console.WriteLine("  [WhenAny] اجرای یک کار با تایم‌اوت 1 ثانیه");

        var cts = new CancellationTokenSource();

        // کار اصلی که 3 ثانیه طول می‌کشد (بیش از تایم‌اوت)
        Task workTask = Task.Delay(TimeSpan.FromSeconds(3), cts.Token);

        // تایم‌اوت: بعد از 1 ثانیه برنده می‌شود
        Task timeoutTask = Task.Delay(TimeSpan.FromSeconds(1));

        Task firstFinished = await Task.WhenAny(workTask, timeoutTask);

        if (firstFinished == timeoutTask)
        {
            Console.WriteLine("  [WhenAny] تایم‌اوت رخ داد - کار بیش از حد طول کشید");
            cts.Cancel(); // لغو کار اصلی برای جلوگیری از اجرای بیهوده
        }
        else
        {
            Console.WriteLine("  [WhenAny] کار موفقیت‌آمیز بود");
        }
    }

    // --------------------------------------------------------------------------
    //  QueryServerAsync: شبیه‌سازی پرس‌وجو از سرور با تأخیر تصادفی
    // --------------------------------------------------------------------------
    private static async Task<string> QueryServerAsync(string server)
    {
        Random rnd = new(server.GetHashCode());
        int delay = rnd.Next(200, 1500);
        await Task.Delay(delay);
        return $"{server} پاسخ داد در {delay}ms";
    }
}

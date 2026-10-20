// ============================================================================
//  AsyncBasic.cs - مفاهیم پایه برنامه‌نویسی ناهمگام (async/await)
// ============================================================================
//  Async (Asynchronous) به معنای «ناهمگام» است: عملیات بدون مسدود کردن
//  رشته فراخوان (calling thread) انجام می‌شود. CPU در حین انتظار برای I/O
//  (مانند شبکه، دیسک، پایگاه داده) آزاد می‌ماند تا کار دیگری انجام دهد.
//
//  async/await تنها چند Task واقعی ایجاد نمی‌کند؛ بلکه از ThreadPool بهره
//  می‌برد و State Machine تولید می‌کند تا کد خوانا بماند.
//
//  مفاهیم کلیدی این فایل:
//   - کلمه کلیدی async
//   - عملگر await
//   - کلاس Task و Task<T>
//   - متد Task.Delay (به جای Thread.Sleep)
// ============================================================================

namespace ConcurrentProgrammingDemo.AsynchronousProgramming;

public static class AsyncBasic
{
    // --------------------------------------------------------------------------
    //  RunAsync: متد ورودی این فایل نمونه - همه مثال‌ها را اینجا فراخوانی می‌کند
    // --------------------------------------------------------------------------
    public static async Task RunAsync()
    {
        Console.WriteLine("  [AsyncBasic] آغاز نمایش مفاهیم پایه async/await");

        // فراخوانی متد ناهمگام که رشته فعلی را مسدود نمی‌کند.
        await DoWorkAsync("کار 1");

        // خروجی از متدی که مقداد را برمی‌گرداند
        int result = await ComputeAsync(10);
        Console.WriteLine($"  [AsyncBasic] نتیجه محاسبه: {result}");

        // مقایسه اجرای ترتیبی با ناهمگام
        await SequentialVsConcurrentAsync();
    }

    // --------------------------------------------------------------------------
    //  DoWorkAsync: یک متد ناهمگام ساده با Task (بدون مقدار بازگشتی)
    // --------------------------------------------------------------------------
    //  - کلمه async به کامپایلر می‌گوید این متد State Machine تولید می‌کند.
    //  - await Task.Delay شبیه‌سازی I/O با تأخیر بدون مسدود کردن رشته.
    //  - خروجی Task یعنی این متد چیزی برنمی‌گرداند اما منتظر می‌ماند.
    // --------------------------------------------------------------------------
    public static async Task DoWorkAsync(string label)
    {
        Console.WriteLine($"  [AsyncBasic] شروع {label} - رشته: {Environment.CurrentManagedThreadId}");
        await Task.Delay(500); // تأخیر 500ms بدون اشغال کردن رشته
        Console.WriteLine($"  [AsyncBasic] پایان  {label} - رشته: {Environment.CurrentManagedThreadId}");
    }

    // --------------------------------------------------------------------------
    //  ComputeAsync: متد ناهمگام با مقدار بازگشتی (Task<int>)
    // --------------------------------------------------------------------------
    //  Task<T> یعنی: «پاسخی از نوع T در آینده خواهد آمد».
    //  پس از await، نوع T استخراج می‌شود (نه Task<T>).
    // --------------------------------------------------------------------------
    public static async Task<int> ComputeAsync(int input)
    {
        Console.WriteLine($"  [AsyncBasic] محاسبه برای ورودی {input}");
        await Task.Delay(300); // شبیه‌سازی محاسبه سنگین یا I/O
        return input * input;  // مقدار واقعی T
    }

    // --------------------------------------------------------------------------
    //  SequentialVsConcurrentAsync: مقایسه اجرای ترتیبی و همزمان
    // --------------------------------------------------------------------------
    //  حالت ترتیبی: await پشت سرهم → مجموع تأخیرها = جمع تأخیرها
    //  حالت همزمان (با Task.WhenAll): همه همزمان → تقریباً برابر با طولانی‌ترین
    // --------------------------------------------------------------------------
    private static async Task SequentialVsConcurrentAsync()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // حالت ترتیبی: 500 + 500 + 500 = 1500ms
        await DoWorkAsync("ترتیبی 1");
        await DoWorkAsync("ترتیبی 2");
        await DoWorkAsync("ترتیبی 3");
        Console.WriteLine($"  [AsyncBasic] زمان اجرای ترتیبی: {sw.ElapsedMilliseconds} ms");

        sw.Restart();

        // حالت همزمان: همه همزمان شروع می‌شوند → تقریباً 500ms
        //  Task.WhenAll منتظر می‌ماند تا همه Taskها تمام شوند.
        await Task.WhenAll(
            DoWorkAsync("همزمان 1"),
            DoWorkAsync("همزمان 2"),
            DoWorkAsync("همزمان 3")
        );
        Console.WriteLine($"  [AsyncBasic] زمان اجرای همزمان: {sw.ElapsedMilliseconds} ms");
    }
}

// ============================================================================
//  AsyncStreams.cs - جریان‌های ناهمگام با IAsyncEnumerable<T>
// ============================================================================
//  IAsyncEnumerable<T> (معرفی شده در C# 8) معادل async برای IEnumerable<T>
//  است: به جای جمع‌آوری همه عناصر و بازگرداندن آن‌ها به‌صورت یکجا،
//  عناصر را به‌صورت «ناهمگام» و یکی‌یکی منتشر (yield) می‌کند.
//
//  مزایا:
//   - مصرف حافظه پایین: نیازی به بارگذاری همه داده‌ها در حافظه نیست.
//   - پاسخ سریع‌تر: نخستین عنصر به محض آماده شدن بازمی‌گردد.
//   - ایده‌آل برای: داده‌های زنده، صفحه‌بندی API، لاگ‌های زنده.
//
//  نحو استفاده: با `await foreach` پیمایش می‌شود.
// ============================================================================

namespace ConcurrentProgrammingDemo.AsynchronousProgramming;

public static class AsyncStreams
{
    // --------------------------------------------------------------------------
    //  RunAsync: پیمایش یک جریان ناهمگام از رویدادها
    // --------------------------------------------------------------------------
    public static async Task RunAsync()
    {
        Console.WriteLine("  [AsyncStreams] دریافت رویدادها از یک جریان ناهمگام");

        //  نکته: `await foreach` به‌صورت خودکار منتظر می‌ماند تا عنصر بعدی آماده شود.
        await foreach (var item in ProduceEventsAsync())
        {
            Console.WriteLine($"  [AsyncStreams] دریافت: {item}");
        }
    }

    // --------------------------------------------------------------------------
    //  ProduceEventsAsync: منبع جریان ناهمگام
    // --------------------------------------------------------------------------
    //  `IAsyncEnumerable<int>` یعنی دنباله‌ای از intها که به‌صورت ناهمگام تولید می‌شوند.
    //  `yield return` عنصر را به مصرف‌کننده پس می‌دهد و تابع را تعلیق می‌کند.
    //  `await` درون خود تابع می‌تواند پیدرپی رخ دهد (در حلقه while).
    // --------------------------------------------------------------------------
    private static async IAsyncEnumerable<int> ProduceEventsAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        for (int i = 1; i <= 5; i++)
        {
            // شبیه‌سازی تأخیر در تولید داده (مثلاً پینگ یک سرور)
            await Task.Delay(300, ct);

            // پس دادن مقدار به مصرف‌کننده
            yield return i;
        }
    }

    // --------------------------------------------------------------------------
    //  WithCancellationAsync: نحوه لغو جریان ناهمگام
    // --------------------------------------------------------------------------
    //  با `WithCancellation` می‌توانید CancellationToken را به منبع منتقل کنید
    //  حتی اگر signature تابع آن را نپذیرد.
    // --------------------------------------------------------------------------
    public static async Task WithCancellationAsync()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(1)); // لغو پس از 1 ثانیه

        try
        {
            await foreach (var i in ProduceEventsAsync().WithCancellation(cts.Token))
            {
                Console.WriteLine($"  [AsyncStreams] عنصر: {i}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  [AsyncStreams] جریان لغو شد");
        }
    }
}

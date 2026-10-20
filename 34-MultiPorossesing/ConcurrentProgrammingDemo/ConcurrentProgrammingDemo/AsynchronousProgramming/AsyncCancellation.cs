// ============================================================================
//  AsyncCancellation.cs - لغو عملیات ناهمگام با CancellationToken
// ============================================================================
//  CancellationToken مکانیزم استاندارد دات نت برای لغو همکارانه عملیات
//  طولانی است. برخلاف Thread.Abort (که منسوخ شده)، لغو با همکاری است:
//  منبع لغو را علامت می‌زند و Task باید خودش به این علامت توجه کند.
//
//  اجزای اصلی:
//   - CancellationTokenSource: منبع (کنترل‌کننده) - متد Cancel را فراخوانی می‌کنیم.
//   - CancellationToken: توکن (پیمایش‌گر) - به Task پاس داده می‌شود.
//   - IsCancellationRequested: علامت بولین.
//   - ThrowIfCancellationRequested: پرتاب OperationCanceledException.
// ============================================================================

namespace ConcurrentProgrammingDemo.AsynchronousProgramming;

public static class AsyncCancellation
{
    // --------------------------------------------------------------------------
    //  RunAsync: نمایش دو سناریوی لغو
    // --------------------------------------------------------------------------
    public static async Task RunAsync()
    {
        Console.WriteLine("  [Cancellation] سناریو ۱: لغو با CancelAfter");
        await CancelAfterScenarioAsync();

        Console.WriteLine("  [Cancellation] سناریو ۲: لغو صریح توسط کاربر (شبیه‌سازی)");
        await ExplicitCancelScenarioAsync();
    }

    // --------------------------------------------------------------------------
    //  CancelAfterScenarioAsync: لغو خودکار پس از مدت زمان مشخص
    // --------------------------------------------------------------------------
    //  `CancelAfter` یک تایمر داخلی راه‌اندازی می‌کند و پس از مدت مشخص
    //  توکن را لغو می‌کند. ایده‌آل برای Timeout در I/O.
    // --------------------------------------------------------------------------
    private static async Task CancelAfterScenarioAsync()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(500)); // لغو پس از 500ms

        try
        {
            // کار اصلی قرار است 5 ثانیه طول بکشد
            await LongRunningOperationAsync(cts.Token);
            Console.WriteLine("  [Cancellation] کار کامل شد (نمی‌بایست رخ دهد)");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  [Cancellation] کار به‌خاطر تایم‌اوت لغو شد ✓");
        }
    }

    // --------------------------------------------------------------------------
    //  ExplicitCancelScenarioAsync: لغو صریح (مانند دکمه کاربر)
    // --------------------------------------------------------------------------
    //  در اینجا ما صراحتاً Cancel را فراخوانی می‌کنیم. در دنیای واقعی،
    //  کاربر روی دکمه «لغو» کلیک می‌کند و کنترل‌گر رویداد این کار را انجام می‌دهد.
    // --------------------------------------------------------------------------
    private static async Task ExplicitCancelScenarioAsync()
    {
        using var cts = new CancellationTokenSource();

        // Task کار را همزمان شروع کن
        Task work = Task.Run(() => LongRunningOperationAsync(cts.Token));

        // شبیه‌سازی: کاربر پس از 300ms دکمه لغو را می‌زند
        await Task.Delay(300);
        cts.Cancel(); // <--- فراخوانی صریح Cancel

        try
        {
            await work; // منتظر می‌مانیم تا Task با استثنا یا موفقیت پایان یابد
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  [Cancellation] کار توسط کاربر لغو شد ✓");
        }
    }

    // --------------------------------------------------------------------------
    //  LongRunningOperationAsync: کار طولانی که به توکن توجه می‌کند
    // --------------------------------------------------------------------------
    //  این متد در یک حلقه اجرا می‌شود. در هر گام، ThrowIfCancellationRequested
    //  را فراخوانی می‌کنیم تا اگر کاربر لغو کرده بود، سریعاً خارج شویم.
    //
    //  الگو: اگر Task.Delay با توکن فراخوانی شود، خودش هنگام لغو،
    //  OperationCanceledException پرتاب می‌کند - نیازی به چک دستی نیست.
    // --------------------------------------------------------------------------
    private static async Task LongRunningOperationAsync(CancellationToken ct)
    {
        for (int i = 1; i <= 100; i++)
        {
            //  ThrowIfCancellationRequested: اگر لغو شده، استثنا پرتاب کن
            ct.ThrowIfCancellationRequested();

            //  Delay با توکن: هنگام لغو، استثنا پرتاب می‌کند
            await Task.Delay(100, ct);

            //  اگر Task.Delay با توکن استفاده نکنید، باید خودتان چک کنید:
            //  if (ct.IsCancellationRequested) return;
        }
    }

    // --------------------------------------------------------------------------
    //  CooperativePollingExample: بررسی دستی علامت لغو
    // --------------------------------------------------------------------------
    //  گاهی نمی‌توان همه عملیات را با توکن انجام داد. در حلقه‌های محاسباتی،
    //  می‌توان با IsCancellationRequested به‌صورت دوره‌ای علامت را چک کرد.
    // --------------------------------------------------------------------------
    public static void CooperativePollingExample()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(200));

        CancellationToken ct = cts.Token;
        long sum = 0;

        for (long i = 0; i < long.MaxValue; i++)
        {
            if (ct.IsCancellationRequested)
            {
                Console.WriteLine($"  [Cancellation] حلقه پس از {i} تکرار متوقف شد");
                return;
            }

            sum += i;
        }
    }
}

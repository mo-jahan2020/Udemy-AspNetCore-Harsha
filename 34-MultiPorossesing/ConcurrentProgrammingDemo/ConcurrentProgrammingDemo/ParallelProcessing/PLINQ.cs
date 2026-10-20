// ============================================================================
//  PLINQ.cs - LINQ موازی با AsParallel
// ============================================================================
//  PLINQ (Parallel LINQ) پسوند AsParallel() به LINQ اضافه می‌کند: کوئری
//  به چند بخش تقسیم شده و همزمان روی ThreadPool پردازش می‌شود.
//
//  متدهای کلیدی PLINQ:
//   - AsParallel()              - فعال‌سازی حالت موازی
//   - AsSequential()             - غیرفعال‌سازی دوباره (به حالت ترتیبی)
//   - WithDegreeOfParallelism(n) - تنظیم تعداد رشته‌ها
//   - WithMergeOptions(...)      - نحوه ترکیب نتایج
//   - WithCancellation(token)   - لغو کوئری
//   - AsOrdered()               - حفظ ترتیب اصلی (با کمی افت عملکرد)
//
//  هشدارها:
//   - کار باید به‌اندازه کافی سنگین باشد تا سود بدهد.
//   - عملگرهایی مانند Take() / Skip() به ترتیب وابسته‌اند - از AsOrdered استفاده کنید.
//   - عوارض جانبی (Side Effects) باید ایمن باشند.
// ============================================================================

namespace ConcurrentProgrammingDemo.ParallelProcessing;

public static class PLINQ
{
    // --------------------------------------------------------------------------
    //  Run: چندین مثال از PLINQ
    // --------------------------------------------------------------------------
    public static void Run()
    {
        BasicExample();
        AsOrderedExample();
        WithCancellationExample();
        WithMergeOptionsExample();
    }

    // ==========================================================================
    //  سناریو ۱: فیلتر و تبدیل موازی
    // ==========================================================================
    private static void BasicExample()
    {
        var numbers = Enumerable.Range(1, 100).ToArray();

        //  AsParallel(): کوئری پس از این متد موازی اجرا می‌شود
        var evens = numbers
            .AsParallel()
            .Where(n => n % 2 == 0)        // فیلتر
            .Select(n => n * n)           // تبدیل
            .ToArray();

        Console.WriteLine($"  [PLINQ] تعداد زوج‌های مجذور: {evens.Length} (انتظار: 50) ✓");
        Console.WriteLine($"  [PLINQ] نخستین ۵ عدد: {string.Join(", ", evens.Take(5))}");
    }

    // ==========================================================================
    //  سناریو ۲: حفظ ترتیب با AsOrdered
    // ==========================================================================
    //  PLINQ به‌صورت پیش‌فرض ترتیب حفظ نمی‌کند (هر رشته هر زمان پایان یابد).
    //  اگر خروجی باید به ترتیب ورودی باشد، AsOrdered را اضافه کنید.
    // ==========================================================================
    private static void AsOrderedExample()
    {
        var numbers = Enumerable.Range(1, 10).ToArray();

        // بدون AsOrdered - ترتیب احتمالاً مختل
        var unordered = numbers.AsParallel().Select(n => n * 10).ToArray();

        // با AsOrdered - ترتیب حفظ می‌شود
        var ordered = numbers.AsParallel().AsOrdered().Select(n => n * 10).ToArray();

        Console.WriteLine($"  [PLINQ] بدون AsOrdered: {string.Join(", ", unordered)}");
        Console.WriteLine($"  [PLINQ] با AsOrdered:   {string.Join(", ", ordered)}");
    }

    // ==========================================================================
    //  سناریو ۳: لغو PLINQ با WithCancellation
    // ==========================================================================
    private static void WithCancellationExample()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        try
        {
            var result = Enumerable
                .Range(1, int.MaxValue)
                .AsParallel()
                .WithCancellation(cts.Token)
                .Select(n => HeavyCompute(n))
                .Take(100)
                .ToArray();

            Console.WriteLine($"  [PLINQ] {result.Length} عدد محاسبه شد");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  [PLINQ] کوئری لغو شد ✓");
        }
    }

    // ==========================================================================
    //  سناریو ۴: WithMergeOptions - نحوه ترکیب نتایج
    // ==========================================================================
    //  MergeOptions:
    //   - Default           (AutoBuffered) - میان‌گره بافر می‌شود
    //   - NotBuffered       - هر نتیجه فوراً منتشر می‌شود
    //   - FullyBuffered     - همه نتایج با هم (سریع‌تر، اما حافظه بیشتر)
    // ==========================================================================
    private static void WithMergeOptionsExample()
    {
        var numbers = Enumerable.Range(1, 20).ToArray();

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var fullyBuffered = numbers
            .AsParallel()
            .WithMergeOptions(ParallelMergeOptions.FullyBuffered)
            .Select(n => n * 2)
            .ToList();
        sw.Stop();
        Console.WriteLine($"  [PLINQ] FullyBuffered: {sw.ElapsedTicks} ticks");

        sw.Restart();
        var notBuffered = numbers
            .AsParallel()
            .WithMergeOptions(ParallelMergeOptions.NotBuffered)
            .Select(n => n * 2)
            .ToList();
        sw.Stop();
        Console.WriteLine($"  [PLINQ] NotBuffered: {sw.ElapsedTicks} ticks");
    }

    // --------------------------------------------------------------------------
    //  HeavyCompute: شبیه‌سازی محاسبه سنگین
    // --------------------------------------------------------------------------
    private static int HeavyCompute(int n)
    {
        //  PLINQ برای کارهای سبک سود نخواهد داشت - اینجا کار را سنگین می‌کنیم
        int sum = 0;
        for (int i = 0; i < 10000; i++) sum += i;
        return n + sum;
    }
}

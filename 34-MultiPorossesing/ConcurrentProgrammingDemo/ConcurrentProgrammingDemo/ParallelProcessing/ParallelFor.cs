// ============================================================================
//  ParallelFor.cs - حلقه موازی شمارشی با Parallel.For
// ============================================================================
//  Parallel.For نسخه موازی for است. دامنه تکرار به چند «Chunk» تقسیم
//  می‌شود که هر کدام روی یک رشته ThreadPool اجرا می‌شود.
//
//  تفاوت با for ساده:
//   - ترتیب اجرا تضمین‌نشده (پیمایش‌ها ممکن است از هم بپرند).
//   - کار بدنه باید مستقل باشد (Stateless یا همگام‌سازی‌شده).
//   - اگر کار بسیار سبک باشد، Overhead موازی‌سازی سود نخواهد داشت.
//
//  سیگنیچر اصلی:
//   Parallel.For(fromInclusive, toExclusive, body)
//   Parallel.For(fromInclusive, toExclusive, options, body)
//   Parallel.For(fromInclusive, toExclusive, localInit, body, localFinally)
//
//  ParallelLoopState: برای Stop (توقف سریع) یا Break (توقف پس از گام فعلی).
// ============================================================================

namespace ConcurrentProgrammingDemo.ParallelProcessing;

public static class ParallelFor
{
    // --------------------------------------------------------------------------
    //  Run: نمایش چند سیگنیچر Parallel.For
    // --------------------------------------------------------------------------
    public static void Run()
    {
        BasicExample();
        WithOptionsExample();
        WithStopExample();
        WithThreadLocalExample();
    }

    // ==========================================================================
    //  سناریو ۱: فرم پایه - شبیه‌سازی محاسبه مستقل
    // ==========================================================================
    private static void BasicExample()
    {
        long sum = 0;

        //  Parallel.For(fromInclusive=0, toExclusive=100, body)
        //  body به‌صورت delegate به ازای هر i فراخوانی می‌شود.
        Parallel.For(0, 100, i =>
        {
            //  نکته: body در رشته‌های مختلف اجرا می‌شود؛ بنابراین
            //  هر دسترسی به منبع مشترک (sum) باید ایمن باشد.
            Interlocked.Add(ref sum, i); // <--- اتمیک
        });

        Console.WriteLine($"  [ParallelFor] مجموع 0..99 = {sum} (انتظار: 4950) ✓");
    }

    // ==========================================================================
    //  سناریو ۲: با ParallelOptions (درجه موازی‌سازی، CancellationToken)
    // ==========================================================================
    private static void WithOptionsExample()
    {
        var opts = new ParallelOptions
        {
            MaxDegreeOfParallelism = 2, // <--- حداکثر ۲ رشته موازی
            CancellationToken = CancellationToken.None
        };

        Parallel.For(0, 10, opts, i =>
        {
            Console.WriteLine($"  [ParallelFor] i={i} روی رشته {Environment.CurrentManagedThreadId}");
            Thread.Sleep(100);
        });
    }

    // ==========================================================================
    //  سناریو ۳: توقف زودهنگام با ParallelLoopState.Stop
    // ==========================================================================
    //  Stop: همه تکرارهای آینده را لغو می‌کند (به محض ممکن شدن).
    //  Break: تکرارهای با اندیس بزرگ‌تر از فعلی را لغو می‌کند.
    // ==========================================================================
    private static void WithStopExample()
    {
        var result = Parallel.For(0, 1000, (i, loopState) =>
        {
            if (i == 50)
            {
                Console.WriteLine($"  [ParallelFor] توقف در i={i}");
                loopState.Stop(); // <--- درخواست توقف
                return;
            }
        });

        Console.WriteLine($"  [ParallelFor] IsCompleted={result.IsCompleted}, LowestBreakIteration={result.LowestBreakIteration}");
    }

    // ==========================================================================
    //  سناریو ۴: الگوی Thread-Local برای عملکرد بهتر
    // ==========================================================================
    //  وقتی body به یک متغیر مشترک (sum) می‌نویسد، Interlocked.Add گران است.
    //  الگوی «Thread-Local Aggregation»: هر رشته مجموعه محلی خود را نگه
    //  می‌دارد، در پایان localFinally مقادیر محلی را به نتیجه نهایی می‌افزاید.
    //  این الگو 10x سریع‌تر از Interlocked برای کارهای سنگین است.
    // ==========================================================================
    private static void WithThreadLocalExample()
    {
        long grandTotal = 0;

        Parallel.For(
            fromInclusive: 0,
            toExclusive: 100000,
            localInit: () => 0L, // مقدار اولیه محلی هر رشته
            body: (i, loopState, localSum) =>
            {
                localSum += i; // بدون نیاز به قفل - محلی است
                return localSum; // بازگرداندن مقدار جدید
            },
            localFinally: localSum =>
            {
                //  این بخش برای هر رشته یک‌بار اجرا می‌شود
                //  تنها اینجا باید همگام‌سازی کنیم
                Interlocked.Add(ref grandTotal, localSum);
            }
        );

        long expected = (long)(100000 - 1) * 100000 / 2;
        Console.WriteLine($"  [ParallelFor] Thread-Local مجموع = {grandTotal} (انتظار: {expected}) ✓");
    }
}

// ============================================================================
//  ParallelInvoke.cs - فراخوانی موازی چند عملیات مستقل
// ============================================================================
//  Parallel.Invoke چند Action را همزمان اجرا می‌کند و منتظر می‌ماند تا
//  همگی پایان یابند. ساده‌ترین شکل موازی‌سازی: فقط Actionها را لیست کن.
//
//  کاربردها:
//   - اجرای همزمان چند کار مستقل (مثلاً: خواندن ۳ فایل کانفیگ مختلف)
//   - بارگذاری داده‌های اولیه موازی هنگام راه‌اندازی برنامه
//
//  سیگنیچر:
//   Parallel.Invoke(params Action[] actions)
//   Parallel.Invoke(ParallelOptions, params Action[] actions)
// ============================================================================

namespace ConcurrentProgrammingDemo.ParallelProcessing;

public static class ParallelInvoke
{
    // --------------------------------------------------------------------------
    //  Run: چند مثال از Parallel.Invoke
    // --------------------------------------------------------------------------
    public static void Run()
    {
        BasicExample();
        WithOptionsExample();
        ExceptionHandlingExample();
    }

    // ==========================================================================
    //  سناریو ۱: ۳ کار مستقل به‌صورت همزمان
    // ==========================================================================
    private static void BasicExample()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        //  هر Action به‌طور مستقل روی یک رشته موازی اجرا می‌شود.
        //  Invoke تا تکمیل همه ۳ مسدود می‌ماند (Synchronous).
        Parallel.Invoke(
            () => DoWork("کار الف", 300),
            () => DoWork("کار ب", 200),
            () => DoWork("کار ج", 400)
        );

        sw.Stop();
        Console.WriteLine($"  [ParallelInvoke] کل زمان: {sw.ElapsedMilliseconds}ms (باید نزدیک 400ms باشد) ✓");
    }

    // ==========================================================================
    //  سناریو ۲: با ParallelOptions - محدودسازی درجه موازی‌سازی
    // ==========================================================================
    private static void WithOptionsExample()
    {
        var opts = new ParallelOptions
        {
            MaxDegreeOfParallelism = 2 // <--- نهایتاً ۲ کار همزمان
        };

        var actions = new Action[]
        {
            () => DoWork("کار 1", 200),
            () => DoWork("کار 2", 200),
            () => DoWork("کار 3", 200),
            () => DoWork("کار 4", 200)
        };

        var sw = System.Diagnostics.Stopwatch.StartNew();
        Parallel.Invoke(opts, actions);
        sw.Stop();

        //  با ۲ رشته و ۴ کار هرکدام 200ms، کل باید حدود 400ms باشد
        Console.WriteLine($"  [ParallelInvoke] زمان با MaxDegree=2: {sw.ElapsedMilliseconds}ms (انتظار ~400ms)");
    }

    // ==========================================================================
    //  سناریو ۳: مدیریت استثنا
    // ==========================================================================
    //  اگر چند Action استثنا پرتاب کنند، Invoke یک AggregateException با همه
    //  آن‌ها برمی‌گرداند (درست مثل Parallel.For).
    // ==========================================================================
    private static void ExceptionHandlingExample()
    {
        try
        {
            Parallel.Invoke(
                () => throw new InvalidOperationException("خطای ۱"),
                () => throw new FormatException("خطای ۲"),
                () => Console.WriteLine("  [ParallelInvoke] کار ۳ موفق بود")
            );
        }
        catch (AggregateException ae)
        {
            Console.WriteLine($"  [ParallelInvoke] {ae.InnerExceptions.Count} استثنا جمع‌آوری شد:");
            foreach (var ex in ae.InnerExceptions)
                Console.WriteLine($"    - {ex.GetType().Name}: {ex.Message}");
        }
    }

    // --------------------------------------------------------------------------
    //  DoWork: شبیه‌سازی کار مستقل
    // --------------------------------------------------------------------------
    private static void DoWork(string label, int durationMs)
    {
        Console.WriteLine($"  [ParallelInvoke] {label} شروع (رشته {Environment.CurrentManagedThreadId})");
        Thread.Sleep(durationMs);
        Console.WriteLine($"  [ParallelInvoke] {label} پایان");
    }
}

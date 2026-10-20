// ============================================================================
//  ParallelForEach.cs - پیمایش موازی مجموعه با Parallel.ForEach
// ============================================================================
//  Parallel.ForEach معادل foreach موازی است. برای هر عنصر مجموعه،
//  body روی رشته ThreadPool اجرا می‌شود.
//
//  مجموعه مناسب: IEnumerable<T> (با Count شناخته‌شده بهتر است؛ اگر
//  شمارشگر کلی پیمایش شود، Partitioner.Create خودش چند بخش می‌سازد).
//
//  سناریوهای رایج:
//   - پردازش فایل‌های پوشه (هر فایل در یک رشته)
//   - پردازش رکوردهای پایگاه داده (پیمایش IDataReader)
//   - دانلود موازی چند URL
// ============================================================================

using System.Collections.Concurrent;

namespace ConcurrentProgrammingDemo.ParallelProcessing;

public static class ParallelForEach
{
    // --------------------------------------------------------------------------
    //  Run: نمایش چند سیگنیچر Parallel.ForEach
    // --------------------------------------------------------------------------
    public static void Run()
    {
        BasicExample();
        WithOptionsExample();
        WithPartitionerExample();
    }

    // ==========================================================================
    //  سناریو ۱: پیمایش موازی یک List
    // ==========================================================================
    private static void BasicExample()
    {
        var items = Enumerable.Range(1, 10).ToList();

        //  Parallel.ForEach(IEnumerable, body)
        //  body: (item, state, index) - index اختیاری و nullable است
        Parallel.ForEach(items, (item, state, index) =>
        {
            Console.WriteLine($"  [ParallelForEach] item={item}, index={index}, thread={Environment.CurrentManagedThreadId}");
            Thread.Sleep(50);
        });
    }

    // ==========================================================================
    //  سناریو ۲: با MaxDegreeOfParallelism و CancellationToken
    // ==========================================================================
    private static void WithOptionsExample()
    {
        var urls = new[] { "url1", "url2", "url3", "url4", "url5" };

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        var opts = new ParallelOptions
        {
            MaxDegreeOfParallelism = 2,
            CancellationToken = cts.Token
        };

        try
        {
            Parallel.ForEach(urls, opts, url =>
            {
                opts.CancellationToken.ThrowIfCancellationRequested();
                Console.WriteLine($"  [ParallelForEach] پردازش {url}");
                Thread.Sleep(300);
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("  [ParallelForEach] لغو شد ✓");
        }
    }

    // ==========================================================================
    //  سناریو ۳: استفاده از Partitioner برای کنترل دستی چند بخشی
    // ==========================================================================
    //  Partitioner.Create می‌تواند دامنه‌های (Range) دلخواه بسازد؛
    //  برای بدنه‌های سبک که در آن‌ها Overhead partition پیش‌فرض زیاد است.
    // ==========================================================================
    private static void WithPartitionerExample()
    {
        //  ایجاد Rangeهایی به طول 1000 - هر رشته 1000 عدد را یکجا پردازش می‌کند
        var ranges = Partitioner.Create(0, 10000, rangeSize: 1000);

        long total = 0;
        Parallel.ForEach(ranges, range =>
        {
            //  range یک Tuple<int,int> است: (from, to)
            long localSum = 0;
            for (int i = range.Item1; i < range.Item2; i++)
                localSum += i;

            Interlocked.Add(ref total, localSum);
            Console.WriteLine($"  [ParallelForEach] بخش [{range.Item1},{range.Item2}) → {localSum}");
        });

        Console.WriteLine($"  [ParallelForEach] مجموع = {total}");
    }
}

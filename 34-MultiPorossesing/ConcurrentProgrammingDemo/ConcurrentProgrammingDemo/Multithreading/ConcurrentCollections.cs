// ============================================================================
//  ConcurrentCollections.cs - مجموعه‌های امن برای چندرشته‌ای
// ============================================================================
//  مجموعه‌های استاندارد (List<T>، Dictionary<K,V>، Queue<T>) در دسترسی
//  همزمان ناامن هستند. دات نت نسخه‌های همگام‌سازی‌شده ارائه می‌دهد:
//
//   1) ConcurrentDictionary<TKey, TValue>   - دیکشنری امن
//   2) ConcurrentQueue<T>                     - صف FIFO امن (lock-free)
//   3) ConcurrentStack<T>                    - پشته LIFO امن
//   4) ConcurrentBag<T>                      - کیسه بدون ترتیب (سریع‌تر برای همان رشته)
//   5) BlockingCollection<T>                 - نسخه با مسدود کردن (Producer/Consumer)
//
//  الگو: گاهی ترجیح می‌دهیم از immutable collections (مثل ImmutableList)
//  استفاده کنیم که اصلاً قابل تغییر نیستند - آن‌ها هم امن هستند.
// ============================================================================

using System.Collections.Concurrent;

namespace ConcurrentProgrammingDemo.Multithreading;

public static class ConcurrentCollections
{
    // --------------------------------------------------------------------------
    //  Run: نمایش هر مجموعه با یک مثال
    // --------------------------------------------------------------------------
    public static void Run()
    {
        ConcurrentDictionaryExample();
        ConcurrentQueueExample();
        BlockingCollectionExample();
    }

    // ==========================================================================
    //  سناریو ۱: ConcurrentDictionary - دیکشنری امن
    // ==========================================================================
    //  متدهای کلیدی: TryAdd، AddOrUpdate، GetOrAdd، TryGetValue.
    //  این متدها اتمیک هستند: یعنی «بررسی و سپس افزودن» در یک عملیات
    //  انجام می‌شود، نه دو عملیات جدا.
    // ==========================================================================
    private static void RunConcurrentDictionary()
    {
        var dict = new ConcurrentDictionary<string, int>();

        // AddOrUpdate: اگر کلید نیست، اضافه کن؛ اگر هست، مقدار را با تابع به‌روز کن
        int result = dict.AddOrUpdate("apple", 1, (key, oldVal) => oldVal + 1);
        Console.WriteLine($"  [ConcDict] apple = {result}");

        // GetOrAdd: اگر هست برگردان، اگر نیست با تابع بساز
        int v = dict.GetOrAdd("banana", _ => 5);
        Console.WriteLine($"  [ConcDict] banana = {v}");

        // TryAdd: اضافه کن اگر نیست؛ برمی‌گرداند true/false
        bool ok = dict.TryAdd("apple", 100);
        Console.WriteLine($"  [ConcDict] TryAdd apple = {ok}");

        Parallel.ForEach(Enumerable.Range(0, 1000), _ =>
        {
            //  با AddOrUpdate ایمن می‌توان از چندین رشته همزمان شمارش کرد
            dict.AddOrUpdate("counter", 1, (_, old) => old + 1);
        });

        Console.WriteLine($"  [ConcDict] counter بعد از 1000 افزایش همزمان: {dict["counter"]}");
    }

    //  نسخه اصلی که از Program.cs فراخوانی می‌شود:
    private static void ConcurrentDictionaryExample() => RunConcurrentDictionary();

    // ==========================================================================
    //  سناریو ۲: ConcurrentQueue - صف FIFO بدون قفل
    // ==========================================================================
    //  ساخته‌شده برای حالت «چند تولیدکننده، چند مصرف‌کننده». enqueue و
    //  dequeue عملیات lock-free دارند (ازCAS سخت‌افزاری استفاده می‌کنند).
    // ==========================================================================
    private static void ConcurrentQueueExample()
    {
        var queue = new ConcurrentQueue<int>();

        // تولید همزمان
        Parallel.For(0, 100, i => queue.Enqueue(i));

        // مصرف: TryDequeue برمی‌گرداند bool و مقدار
        int sum = 0;
        while (queue.TryDequeue(out int item))
        {
            sum += item;
        }

        Console.WriteLine($"  [ConcQueue] مجموع 100 عنصر: {sum} (انتظار: {4950})");

        //  اگر فقط تعداد باقی‌مانده را بخواهید: queue.Count
        //  اگر فقط به اولین بدون حذف نگاه کنید: queue.TryPeek(out var head)
    }

    // ==========================================================================
    //  سناریو ۳: BlockingCollection - مسدودکننده برای Producer/Consumer
    // ==========================================================================
    //  وقتی مصرف‌کننده زودتر از تولیدکننده می‌پرسد، بدون چیزی می‌ماند.
    //  BlockingCollection خودش مسدود می‌شود تا چیزی تولید شود:
    //   - Add: اگر کامل باشد، مسدود می‌شود
    //   - Take: اگر خالی باشد، مسدود می‌شود
    //  GetConsumingEnumerable: پیمایش‌گر ناهمگام (خوب برای foreach در مصرف‌کننده)
    // ==========================================================================
    private static void BlockingCollectionExample()
    {
        //  BoundedCapacity=10 یعنی تولیدکننده حداکثر 10 را در صف بگذارد تا
        //  مصرف‌کننده چیزی بردارد (Back-pressure).
        using var bc = new BlockingCollection<int>(boundedCapacity: 10);

        // تولیدکننده
        var producer = Task.Run(() =>
        {
            for (int i = 0; i < 20; i++)
            {
                bc.Add(i);
                Console.WriteLine($"  [Blocking] تولید: {i}");
                Thread.Sleep(50);
            }
            bc.CompleteAdding(); // <--- سیگنال: دیگر چیزی اضافه نمی‌شود
        });

        // مصرف‌کننده
        var consumer = Task.Run(() =>
        {
            //  GetConsumingEnumerable: مسدود می‌شود تا Add جدید یا CompleteAdding
            foreach (var item in bc.GetConsumingEnumerable())
            {
                Console.WriteLine($"  [Blocking] مصرف: {item}");
                Thread.Sleep(80);
            }
        });

        Task.WaitAll(producer, consumer);
        Console.WriteLine("  [Blocking] تولید/مصرف پایان یافت ✓");
    }
}

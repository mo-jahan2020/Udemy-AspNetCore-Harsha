// ============================================================================
//  ThreadSynchronization.cs - همگام‌سازی دسترسی همزمان به منابع مشترک
// ============================================================================
//  وقتی چند رشته به یک منبع مشترک (متغیر، فایل، پایگاه داده) دسترسی
//  دارند، بدون همگام‌سازی، «Race Condition» (وضعیت مسابقه) رخ می‌دهد:
//  خروجی به ترتیب اجرا وابسته می‌شود و نتایج غیرقابل پیش‌بینی به دست
//  می‌آید.
//
//  ابزارهای همگام‌سازی دات نت:
//   1) lock / Monitor            - انحصار متنی، سبک، فقط در همان پروسه
//   2) Mutex                     - انحصار بین پروسه‌ها (Inter-process)
//   3) Semaphore / SemaphoreSlim - اجازه به N رشته به‌طور همزمان
//   4) ReaderWriterLockSlim      - جداسازی خواندن/نوشتن
//   5) Interlocked               - عملیات اتمیک برای انواع اولیه
// ============================================================================

namespace ConcurrentProgrammingDemo.Multithreading;

public static class ThreadSynchronization
{
    // --------------------------------------------------------------------------
    //  Run: نمایش مقایسه ناامن در برابر امن با lock
    // --------------------------------------------------------------------------
    public static void Run()
    {
        Console.WriteLine("  [Sync] سناریو ۱: شمارنده ناامن (Race Condition)");
        RunUnsafeCounter();

        Console.WriteLine("  [Sync] سناریو ۲: شمارنده امن با lock");
        RunLockedCounter();

        Console.WriteLine("  [Sync] سناریو ۳: شمارنده امن با Interlocked");
        RunInterlockedCounter();

        Console.WriteLine("  [Sync] سناریو ۴: SemaphoreSlim محدودکننده N رشته");
        RunSemaphoreExample().GetAwaiter().GetResult();
    }

    // ==========================================================================
    //  سناریو ۱: شمارنده ناامن - نمایش Race Condition
    // ==========================================================================
    private static int _unsafeCounter = 0;

    private static void RunUnsafeCounter()
    {
        _unsafeCounter = 0;
        var threads = new Thread[10];

        for (int i = 0; i < 10; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 10_000; j++)
                {
                    //  x++ در ظاهر یک عملیات است، اما در IL چند دستور است:
                    //  read, increment, store - میان این، رشته دیگری می‌تواند
                    //  همان مقدار قدیمی را بخواند و سپس هر دو همان مقدار را
                    //  بنویسند → یک واحد گم می‌شود.
                    _unsafeCounter++;
                }
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();

        //  انتظار: 100,000 - واقعیت: معمولاً کمتر به‌خاطر Race Condition
        Console.WriteLine($"  [Sync] مقدار نهایی (ناامن): {_unsafeCounter} (انتظار: 100000)");
    }

    // ==========================================================================
    //  سناریو ۲: شمارنده امن با lock
    // ==========================================================================
    private static int _lockedCounter = 0;
    private static readonly object _lockObj = new(); // <--- شیء همگام‌سازی

    private static void RunLockedCounter()
    {
        _lockedCounter = 0;
        var threads = new Thread[10];

        for (int i = 0; i < 10; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 10_000; j++)
                {
                    //  lock در عمل یک wrapper برای Monitor.Enter/Exit است.
                    //  از آن به‌صورت `lock (object)` استفاده کنید و همیشه یک
                    //  شیء خصوصی به‌عنوان قفل بگیرید (نه `this` یا `typeof(T)`).
                    lock (_lockObj)
                    {
                        _lockedCounter++;
                    }
                }
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();

        Console.WriteLine($"  [Sync] مقدار نهایی (قفل‌شده): {_lockedCounter} (انتظار: 100000) ✓");
    }

    // ==========================================================================
    //  سناریو ۳: شمارنده با Interlocked (بدون قفل)
    // ==========================================================================
    //  Interlocked عملیات اتمیک سخت‌افزاری ارائه می‌دهد: بسیار سریع‌تر از lock
    //  برای انواع اولیه (int, long, double) و کارهای تک‌مرحله‌ای.
    // ==========================================================================
    private static int _interlockedCounter = 0;

    private static void RunInterlockedCounter()
    {
        _interlockedCounter = 0;
        var threads = new Thread[10];

        for (int i = 0; i < 10; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < 10_000; j++)
                {
                    //  Interlocked.Increment: افزایش اتمیک، بدون نیاز به lock
                    //  بهتر از `lock(x) { x++; }` برای شمارنده ساده.
                    System.Threading.Interlocked.Increment(ref _interlockedCounter);
                }
            });
            threads[i].Start();
        }

        foreach (var t in threads) t.Join();

        Console.WriteLine($"  [Sync] مقدار نهایی (Interlocked): {_interlockedCounter} (انتظار: 100000) ✓");
    }

    // ==========================================================================
    //  سناریو ۴: SemaphoreSlim - اجازه به N رشته به‌طور همزمان
    // ==========================================================================
    //  Semaphore = گاز دادن. به جای انحصار کامل (مثل lock)، به N رشته
    //  همزمان اجازه ورود می‌دهد. ایده‌آل برای محدودسازی بارگیری به یک
    //  سرویس (مثلاً ۳ اتصال همزمان).
    //  SemaphoreSlim نسخه سبک و مبتنی بر Task است.
    // ==========================================================================
    private static async Task RunSemaphoreExample()
    {
        Console.WriteLine("  [Sync] فقط ۳ رشته می‌توانند همزمان وارد بخش بحرانی شوند");

        using var sem = new SemaphoreSlim(3, 3); // initialCount=3, max=3

        var tasks = new Task[10];
        for (int i = 0; i < 10; i++)
        {
            int id = i;
            tasks[i] = Task.Run(async () =>
            {
                Console.WriteLine($"  [Sync] Task {id} در انتظار...");
                await sem.WaitAsync(); // گرفتن یک جایگاه

                try
                {
                    Console.WriteLine($"  [Sync] Task {id} وارد شد - رشته {Environment.CurrentManagedThreadId}");
                    await Task.Delay(200);
                }
                finally
                {
                    sem.Release(); // <--- حتماً در finally آزاد کنید
                }
            });
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("  [Sync] همه Taskها از Semaphore عبور کردند");
    }
}

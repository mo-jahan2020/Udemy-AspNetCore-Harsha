// ============================================================================
//  ThreadPoolDemo.cs - استفاده از ThreadPool (مجموعه رشته‌ها)
// ============================================================================
//  ThreadPool مجموعه‌ای از رشته‌های از پیش‌ساخته‌شده است که برای کارهای
//  کوتاه (به‌طور کلی < 250ms) استفاده می‌شود.
//
//  مزایا:
//   - بدون هزینه ساخت و تخریب Thread (رشته‌ها قابل استفاده مجدد هستند).
//   - تعداد رشته‌ها به‌صورت پویا مدیریت می‌شود (GetMinThreads/SetMinThreads).
//   - پایه‌ی Task Parallel Library و async/await.
//
//  نقص:
//   - کارهای طولانی (LongRunning) باید با Thread دستی یا Task با گزینه LongRunning.
//   - نام‌گذاری Thread سخت است (همه رشته‌های ThreadPool بی‌نام هستند).
//   - اولویت‌بندی پیچیده.
//
//  الگوی مدرن: از Task.Run استفاده کنید که در عمل روی ThreadPool اجرا می‌شود.
// ============================================================================

namespace ConcurrentProgrammingDemo.Multithreading;

public static class ThreadPoolDemo
{
    // --------------------------------------------------------------------------
    //  Run: نمونه اصلی - صف کار در ThreadPool
    // --------------------------------------------------------------------------
    public static void Run()
    {
        Console.WriteLine("  [ThreadPool] آغاز - آیا رشته فعلی ThreadPool است؟ "
            + Thread.CurrentThread.IsThreadPoolThread);

        //  QueueUserWorkItem: روش پایه برای صف کردن کار در ThreadPool.
        //  نسخه با delegate و state: کار با لاگ محلی.
        for (int i = 0; i < 5; i++)
        {
            int captured = i; // <--- مهم: i را لوکال کپی کنید تا در بسته‌بندی درست باشد
            ThreadPool.QueueUserWorkItem(_ => DoWork(captured));
        }

        //  ThreadPoolcompletion و همگام‌سازی: فعلاً با ManualResetEventSlim منتظر می‌مانیم.
        using var done = new ManualResetEventSlim(false);
        ThreadPool.QueueUserWorkItem(_ =>
        {
            DoWork(99);
            done.Set();
        });

        //  منتظر می‌مانیم تا آخرین کار هم تمام شود
        done.Wait();
        Console.WriteLine("  [ThreadPool] همه کارها ارسال شدند");
    }

    // --------------------------------------------------------------------------
    //  DoWork: کار نمونه - شبیه‌سازی تأخیر
    // --------------------------------------------------------------------------
    private static void DoWork(int id)
    {
        int threadId = Environment.CurrentManagedThreadId;
        Console.WriteLine($"  [ThreadPool] کار {id} روی رشته {threadId} شروع شد");

        //  شبیه‌سازی کار کوتاه (CPU-bound یا I/O)
        Thread.SpinWait(1_000_000);
        Thread.Sleep(100);

        Console.WriteLine($"  [ThreadPool] کار {id} روی رشته {threadId} پایان یافت");
    }

    // --------------------------------------------------------------------------
    //  GetSetBounds: نمایش و تنظیم حداقل/حداکثر رشته‌های ThreadPool
    // --------------------------------------------------------------------------
    public static void GetSetBounds()
    {
        //  GetMinThreads: حداقل تعداد رشته‌های همیشگی
        ThreadPool.GetMinThreads(out int workerMin, out int ioMin);

        //  GetMaxThreads: حداکثر تعداد رشته‌های اجازه‌داده‌شده
        ThreadPool.GetMaxThreads(out int workerMax, out int ioMax);

        Console.WriteLine($"  [ThreadPool] Worker Min/Max: {workerMin}/{workerMax}");
        Console.WriteLine($"  [ThreadPool] I/O Min/Max: {ioMin}/{ioMax}");

        //  SetMinThreads: افزایش تعداد کمینه (برای بار ناگهانی)
        //  نکته: فقط در موارد خاص؛ تنظیم نادرست می‌تواند به عملکرد آسیب بزند.
        bool ok = ThreadPool.SetMinThreads(8, ioMin);
        Console.WriteLine($"  [ThreadPool] SetMinThreads موفق بود؟ {ok}");
    }

    // --------------------------------------------------------------------------
    //  ThreadPoolWithCancellation: لغو کار در صف
    // --------------------------------------------------------------------------
    //  ThreadPool.QueueUserWorkItem یک ورودی UserWorkItemCallback withObjectState
    //  را می‌پذیرد که می‌تواند حاوی CancellationToken باشد.
    // --------------------------------------------------------------------------
    public static void ThreadPoolWithCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(200));

        ThreadPool.QueueUserWorkItem(state =>
        {
            var token = (CancellationToken)state!;
            for (int i = 0; i < 1000; i++)
            {
                if (token.IsCancellationRequested)
                {
                    Console.WriteLine("  [ThreadPool] کار لغو شد");
                    return;
                }
                Thread.Sleep(20);
            }
        }, cts.Token);

        Thread.Sleep(500); // فرصت برای لغو
    }
}

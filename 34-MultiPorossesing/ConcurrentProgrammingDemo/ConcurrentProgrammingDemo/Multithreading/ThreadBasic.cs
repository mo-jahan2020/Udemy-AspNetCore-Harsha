// ============================================================================
//  ThreadBasic.cs - مفاهیم پایه چندرشته‌ای (System.Threading.Thread)
// ============================================================================
//  Thread یک رشته ریشه‌ای سیستم‌عاملی است. هر Thread حدود 1MB حافظه
//  پشته اشغال می‌کند، بنابراین تعداد Threadهای دستی محدود است.
//
//  تفاوت با async/await:
//   - Thread: یک رشته واقعی است که سیستم‌عامل آن را زمان‌بندی می‌کند.
//   - async/await: یک رشته را بین چند کار I/O به اشتراک می‌گذارد.
//
//  استفاده‌ی مناسب از Thread دستی: کارهای طولانی‌مدت CPU-bound که
//  نمی‌خواهیم ThreadPool را اشغال کنند (که برای کارهای کوتاه است).
// ============================================================================

namespace ConcurrentProgrammingDemo.Multithreading;

public static class ThreadBasic
{
    // --------------------------------------------------------------------------
    //  Run: نمونه اصلی این فایل
    // --------------------------------------------------------------------------
    public static void Run()
    {
        Console.WriteLine("  [ThreadBasic] رشته اصلی: " + Environment.CurrentManagedThreadId);

        //  ThreadStart: delegate (تابع بدون پارامتر) که در رشته جدید اجرا می‌شود.
        Thread t1 = new Thread(new ThreadStart(SimpleWorker));
        t1.Name = "Worker-1"; // نام‌گذاری برای دیباگ راحت‌تر
        t1.IsBackground = true; // رشته پس‌زمینه: جلوی خروج پروسه را نمی‌گیرد

        //  ParameterizedThreadStart: ورودی را به‌صورت object می‌پذیرد.
        Thread t2 = new Thread(new ParameterizedThreadStart(ParamWorker));
        t2.Name = "Worker-2";

        //  Start: رشته جدید را راه‌اندازی می‌کند (تخصیص پشته).
        t1.Start();
        t2.Start("Hello from main!");

        //  Join: منتظر می‌ماند تا رشته تمام شود.
        //  معادل await برای Thread دستی.
        t1.Join();
        t2.Join();

        Console.WriteLine("  [ThreadBasic] همه رشته‌ها پایان یافتند");
    }

    // --------------------------------------------------------------------------
    //  SimpleWorker: تابع بدون پارامتر (ThreadStart)
    // --------------------------------------------------------------------------
    private static void SimpleWorker()
    {
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"  [ThreadBasic] {Thread.CurrentThread.Name} در حال کار - گام {i}");
            Thread.Sleep(100); // مسدود کردن رشته (به‌جای Task.Delay)
        }
    }

    // --------------------------------------------------------------------------
    //  ParamWorker: تابع با یک پارامتر object (ParameterizedThreadStart)
    // --------------------------------------------------------------------------
    //  نکته: ParameterizedThreadStart فقط object می‌پذیرد؛ در تابع باید cast کنید.
    //  برای تایپ‌سیفتی، توصیه می‌شود از Task.Run یا الگوهای دیگر استفاده کنید.
    // --------------------------------------------------------------------------
    private static void ParamWorker(object? state)
    {
        string? msg = state as string;
        Console.WriteLine($"  [ThreadBasic] {Thread.CurrentThread.Name} دریافت: {msg}");
        Thread.Sleep(150);
    }

    // --------------------------------------------------------------------------
    //  ThreadProperties: نمایش خواص مهم Thread
    // --------------------------------------------------------------------------
    public static void ThreadProperties()
    {
        Thread t = Thread.CurrentThread;
        Console.WriteLine($"  [ThreadBasic] Name: {t.Name}");
        Console.WriteLine($"  [ThreadBasic] ManagedThreadId: {t.ManagedThreadId}");
        Console.WriteLine($"  [ThreadBasic] IsBackground: {t.IsBackground}");
        Console.WriteLine($"  [ThreadBasic] IsThreadPoolThread: {t.IsThreadPoolThread}");
        Console.WriteLine($"  [ThreadBasic] Priority: {t.Priority}");
        Console.WriteLine($"  [ThreadBasic] ThreadState: {t.ThreadState}");
    }
}

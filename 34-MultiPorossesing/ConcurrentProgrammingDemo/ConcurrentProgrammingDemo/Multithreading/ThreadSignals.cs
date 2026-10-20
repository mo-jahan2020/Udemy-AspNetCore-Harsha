// ============================================================================
//  ThreadSignals.cs - سیگنال‌دهی بین رشته‌ها (WaitHandle)
// ============================================================================
//  سیگنال‌دهی (Signaling) الگویی است که در آن یک رشته منتظر می‌ماند تا
//  رشته دیگر به آن علامت بدهد که «اکنون ادامه بده».
//
//  ابزارهای سیگنال‌دهی:
//   1) AutoResetEvent    - بعد از هر Receive، به‌صورت خودکار Reset می‌شود.
//                          شبیه «با sensitivity به یک ورودی پالس».
//   2) ManualResetEventSlim - دستی Reset می‌شود؛ تا Reset، همه منتظران آزاد می‌شوند.
//                          نسخه Slim سبک‌تر و سریع‌تر برای مدت کوتاه است.
//   3) CountdownEvent   - منتظر می‌ماند تا N رشته سیگنال بدهند.
//   4) Barrier          - همه رشته‌ها در یک نقطه جمع شوند (هم‌زمان).
//
//  نسخه‌های Slim (AutoResetEventSlim ندارد ولی ManualResetEventSlim و
//  CountdownEvent و Barrier) سبک‌تر و درون پروسه‌ای هستند.
// ============================================================================

namespace ConcurrentProgrammingDemo.Multithreading;

public static class ThreadSignals
{
    // --------------------------------------------------------------------------
    //  Run: چهار سناریو را به‌نوبت اجرا می‌کند
    // --------------------------------------------------------------------------
    public static void Run()
    {
        AutoResetEventExample();
        ManualResetEventSlimExample();
        CountdownEventExample();
        BarrierExample();
    }

    // ==========================================================================
    //  سناریو ۱: AutoResetEvent - یک رشته در انتظار، دیگری علامت می‌دهد
    // ==========================================================================
    //  شبیه گاراژ پارکینگ: هر بار یک ماشین وارد یا خارج می‌شود (دقیقاً یکی).
    //  initialState=false یعنی شروعاً در حالت «مسدود» (non-signaled) است.
    // ==========================================================================
    private static void AutoResetEventExample()
    {
        var are = new AutoResetEvent(false);

        Thread consumer = new(() =>
        {
            Console.WriteLine("  [Signal/AutoReset] مصرف‌کننده منتظر علامت است...");
            are.WaitOne(); // <--- مسدود تا Set فراخوانی شود
            Console.WriteLine("  [Signal/AutoReset] دریافت شد! ادامه");

            are.WaitOne(); // منتظر علامت دوم
            Console.WriteLine("  [Signal/AutoReset] علامت دوم دریافت شد ✓");
        })
        { Name = "Consumer" };

        consumer.Start();

        Thread.Sleep(300);
        Console.WriteLine("  [Signal/AutoReset] تولیدکننده علامت اول را می‌فرستد");
        are.Set(); // <--- ارسال علامت (تک‌بار مصرف می‌شود)

        Thread.Sleep(300);
        Console.WriteLine("  [Signal/AutoReset] تولیدکننده علامت دوم را می‌فرستد");
        are.Set();

        consumer.Join();
    }

    // ==========================================================================
    //  سناریو ۲: ManualResetEventSlim - گازی که باز می‌ماند تا بسته شود
    // ==========================================================================
    //  پس از Set، تمام WaitOneها فوراً برمی‌گردند تا زمانی که Reset فراخوانی
    //  شود. ایده‌آل برای «درخت‌شکن» که فقط یک‌بار باز می‌شود.
    // ==========================================================================
    private static void ManualResetEventSlimExample()
    {
        var mres = new ManualResetEventSlim(false);

        var waiters = new Thread[3];
        for (int i = 0; i < 3; i++)
        {
            int id = i;
            waiters[i] = new Thread(() =>
            {
                Console.WriteLine($"  [Signal/ManualReset] رشته {id} منتظر است");
                mres.Wait();
                Console.WriteLine($"  [Signal/ManualReset] رشته {id} عبور کرد");
            });
            waiters[i].Start();
        }

        Thread.Sleep(300);
        Console.WriteLine("  [Signal/ManualReset] دروازه باز می‌شود (Set)");
        mres.Set(); // <--- همه منتظران فوراً عبور می‌کنند

        //  دروازه باز می‌ماند - منتظر جدید بدون انتظار عبور می‌کند.
        //  برای مسدود دوباره: mres.Reset();

        foreach (var t in waiters) t.Join();
    }

    // ==========================================================================
    //  سناریو ۳: CountdownEvent - منتظر تکمیل N سیگنال
    // ==========================================================================
    //  عکس CountdownEvent: به جای «اجازه از N رشته در همزمان»، «منتظر
    //  تا N رشته کارشان تمام شود». ایده‌آل برای Fan-Out/Fan-In ساده.
    // ==========================================================================
    private static void CountdownEventExample()
    {
        using var cde = new CountdownEvent(5); // 5 سیگنال مورد نیاز

        for (int i = 0; i < 5; i++)
        {
            int id = i;
            Task.Run(() =>
            {
                Thread.Sleep(new Random(id).Next(100, 500));
                Console.WriteLine($"  [Signal/Countdown] کار {id} تمام شد");
                cde.Signal(); // <--- کاهش شمارش
            });
        }

        Console.WriteLine("  [Signal/Countdown] رشته اصلی منتظر همه کارهاست");
        cde.Wait(); // <--- مسدود تا شمارش به 0 برسد
        Console.WriteLine("  [Signal/Countdown] همه کارها تمام شدند ✓");
    }

    // ==========================================================================
    //  سناریو ۴: Barrier - همه در یک نقطه هم‌زمان جمع شوند
    // ==========================================================================
    //  Barrier ایده‌آل برای فازبندی کار: N رشته کار فاز 1 را می‌کنند، سپس
    //  همه منتظر می‌مانند تا بقیه هم تمام کنند، سپس فاز 2.
    //  کاربرد: پردازش‌های تکراری فازی، شبیه‌سازی‌های قدم‌به‌قدم.
    // ==========================================================================
    private static void BarrierExample()
    {
        const int PARTICIPANTS = 3;
        var barrier = new Barrier(PARTICIPANTS, b =>
        {
            // این callback پس از هر فاز، در آخرین رشته‌ای که Signal داده اجرا می‌شود
            Console.WriteLine($"  [Signal/Barrier] === فاز {b.CurrentPhaseNumber + 1} تکمیل شد ===");
        });

        var threads = new Thread[PARTICIPANTS];
        for (int i = 0; i < PARTICIPANTS; i++)
        {
            int id = i;
            threads[i] = new Thread(() =>
            {
                Console.WriteLine($"  [Signal/Barrier] کار {id} فاز 1 شروع");
                Thread.Sleep(new Random(id).Next(100, 300));
                Console.WriteLine($"  [Signal/Barrier] کار {id} فاز 1 پایان");
                barrier.SignalAndWait(); // <--- منتظر بقیه

                Console.WriteLine($"  [Signal/Barrier] کار {id} فاز 2 شروع");
                Thread.Sleep(100);
                barrier.SignalAndWait();
            });
        }

        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();
    }
}

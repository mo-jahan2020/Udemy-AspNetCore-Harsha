// ============================================================================
//  ChannelsDemo.cs - System.Threading.Channels (Producer/Consumer مدرن)
// ============================================================================
//  Channel معرفی‌شده در دات نت کور 2.0 یک ساختار داده صف مدرن است:
//   - ناهمگام و با عملکرد بالا (بهینه‌شده برای Producer/Consumer)
//   - پشتیبانی از یک یا چند تولیدکننده و مصرف‌کننده
//   - گزینه‌های bounded (با ظرفیت) و unbounded (بدون ظرفیت)
//
//  مزایا نسبت به BlockingCollection:
//   - API ناهمگام (WriteAsync/WaitToReadAsync) به جای مسدودسازی
//   - بهینه‌تر برای سناریوهای پر فشار
//   - Back-pressure داخلی (با bounded channel)
//
//  دو نوع Channel:
//   - Channel.CreateUnbounded<T>() - بدون محدودیت اندازه
//   - Channel.CreateBounded<T>(capacity) - با محدودیت
// ============================================================================

using System.Threading.Channels;

namespace ConcurrentProgrammingDemo.AdvancedPatterns;

public static class ChannelsDemo
{
    // --------------------------------------------------------------------------
    //  RunAsync: یک تولیدکننده + چند مصرف‌کننده
    // --------------------------------------------------------------------------
    public static async Task RunAsync()
    {
        //  کانال محدودشده با ظرفیت ۵: تولیدکننده اگر پر باشد، مسدود می‌شود
        //  (یا با `await` آن Back-pressure را اعمال می‌کند).
        var channel = Channel.CreateBounded<int>(5);

        var producer = Task.Run(async () =>
        {
            for (int i = 0; i < 20; i++)
            {
                //  WriteAsync: اگر پر باشد، منتظر می‌ماند تا جا باز شود
                await channel.Writer.WriteAsync(i);
                Console.WriteLine($"  [Channel] تولید: {i}");
                await Task.Delay(50);
            }

            //  Complete: به مصرف‌کننده سیگنال می‌دهد که دیگر داده‌ای نخواهد بود
            channel.Writer.Complete();
            Console.WriteLine("  [Channel] تولید تمام شد");
        });

        //  دو مصرف‌کننده همزمان
        var consumer1 = Task.Run(() => ConsumeAsync(channel.Reader, "C1"));
        var consumer2 = Task.Run(() => ConsumeAsync(channel.Reader, "C2"));

        await Task.WhenAll(producer, consumer1, consumer2);
        Console.WriteLine("  [Channel] همه تولید/مصرف پایان یافت ✓");
    }

    // --------------------------------------------------------------------------
    //  ConsumeAsync: تابع مصرف‌کننده استاندارد
    // --------------------------------------------------------------------------
    private static async Task ConsumeAsync(ChannelReader<int> reader, string id)
    {
        try
        {
            //  WaitToReadAsync: مسدود تا چیزی برای خواندن برسد یا Writer بسته شود
            while (await reader.WaitToReadAsync())
            {
                //  TryRead: اگر چیزی هست، آن را برمی‌گرداند (غیر مسدود)
                while (reader.TryRead(out int item))
                {
                    Console.WriteLine($"  [Channel] {id} دریافت: {item}");
                    await Task.Delay(80); // شبیه‌سازی کار
                }
            }
        }
        catch (ChannelClosedException)
        {
            //  کانال بسته شد (طبیعی است پس از Complete)
        }
    }

    // --------------------------------------------------------------------------
    //  BackpressureExample: نمایش Back-pressure با کانال محدود
    // --------------------------------------------------------------------------
    public static async Task BackpressureExample()
    {
        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(3)
        {
            FullMode = BoundedChannelFullMode.Wait // <--- وقتی پر شد، منتظر بمان
        });

        var sw = System.Diagnostics.Stopwatch.StartNew();

        // تولیدکننده سریع: 100 عنصر
        var producer = Task.Run(async () =>
        {
            for (int i = 0; i < 100; i++)
                await channel.Writer.WriteAsync(i);
            channel.Writer.Complete();
        });

        // مصرف‌کننده آهسته: 100ms هر عنصر
        await foreach (var item in channel.Reader.ReadAllAsync())
        {
            await Task.Delay(100);
        }

        sw.Stop();
        Console.WriteLine($"  [Channel] زمان Back-pressure: {sw.ElapsedMilliseconds}ms (حداقل 10s بدون Back-pressure)");
    }
}

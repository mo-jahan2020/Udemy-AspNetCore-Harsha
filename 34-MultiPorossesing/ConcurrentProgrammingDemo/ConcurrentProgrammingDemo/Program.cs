// ============================================================================
//  Program.cs - نقطه ورود اصلی برنامه آموزشی اجرای همزمان در C# 10
// ============================================================================
//  این برنامه سه روش اصلی اجرای همزمان در دات نت را نشان می‌دهد:
//
//  1) Asynchronous Programming  (برنامه‌نویسی ناهمگام - async/await)
//  2) Multithreading             (چندرشته‌ای - Thread / ThreadPool)
//  3) Parallel Processing        (پردازش موازی - Parallel.For / PLINQ)
//
//  علاوه بر این، الگوهای پیشرفته‌تری مانند Channels و ValueTask و TPL
//  Dataflow نیز پوشش داده شده‌اند.
//
//  هر مثال در فایل .cs جداگانه‌ای قرار دارد تا خوانایی افزایش یابد.
//  برای اجرای هر بخش، شماره آن را در منو وارد کنید.
// ============================================================================

using ConcurrentProgrammingDemo.AsynchronousProgramming;
using ConcurrentProgrammingDemo.Multithreading;
using ConcurrentProgrammingDemo.ParallelProcessing;
using ConcurrentProgrammingDemo.AdvancedPatterns;

//فعال‌سازی رنگ برای خروجی کنسول
Console.OutputEncoding = System.Text.Encoding.UTF8;

while (true)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 70));
    Console.WriteLine("   آموزش اجرای همزمان در C# .NET 10  - Concurrent Execution Demo");
    Console.WriteLine(new string('=', 70));
    Console.WriteLine(" 1) Asynchronous Programming  (ناهمگام - async/await)");
    Console.WriteLine(" 2) Multithreading             (چندرشته‌ای)");
    Console.WriteLine(" 3) Parallel Processing        (پردازش موازی)");
    Console.WriteLine(" 4) Advanced Patterns          (الگوهای پیشرفته: Channels / ValueTask / TPL Dataflow)");
    Console.WriteLine(" 5) اجرای همه مثال‌ها به ترتیب");
    Console.WriteLine(" 0) خروج");
    Console.WriteLine(new string('-', 70));
    Console.Write("انتخاب شما: ");

    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) continue;

    if (input == "0") break;

    try
    {
        switch (input)
        {
            case "1":
                await RunAsynchronousExamples();
                break;
            case "2":
                RunMultithreadingExamples();
                break;
            case "3":
                RunParallelProcessingExamples();
                break;
            case "4":
                await RunAdvancedPatterns();
                break;
            case "5":
                await RunAsynchronousExamples();
                RunMultithreadingExamples();
                RunParallelProcessingExamples();
                await RunAdvancedPatterns();
                break;
            default:
                Console.WriteLine("ورودی نامعتبر است.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"خطا هنگام اجرا: {ex.Message}");
    }
}

// ----------------------------------------------------------------------------
//  بخش ۱: مثال‌های برنامه‌نویسی ناهمگام (Asynchronous Programming)
// ----------------------------------------------------------------------------
async Task RunAsynchronousExamples()
{
    PrintHeader("بخش ۱: برنامه‌نویسی ناهمگام (Asynchronous Programming)");

    Console.WriteLine("\n>>> 1.1: مفاهیم پایه async/await");
    await AsyncBasic.RunAsync();

    Console.WriteLine("\n>>> 1.2: اجرای همزمان چند Task با Task.WhenAll");
    await AsyncWhenAll.RunAsync();

    Console.WriteLine("\n>>> 1.3: نخستین Task تکمیل‌شده با Task.WhenAny");
    await AsyncWhenAny.RunAsync();

    Console.WriteLine("\n>>> 1.4: جریان‌های ناهمگام با IAsyncEnumerable");
    await AsyncStreams.RunAsync();

    Console.WriteLine("\n>>> 1.5: لغو عملیات ناهمگام با CancellationToken");
    await AsyncCancellation.RunAsync();
}

// ----------------------------------------------------------------------------
//  بخش ۲: مثال‌های چندرشته‌ای (Multithreading)
// ----------------------------------------------------------------------------
void RunMultithreadingExamples()
{
    PrintHeader("بخش ۲: چندرشته‌ای (Multithreading)");

    Console.WriteLine("\n>>> 2.1: ایجاد و مدیریت Thread");
    ThreadBasic.Run();

    Console.WriteLine("\n>>> 2.2: استفاده از ThreadPool");
    ThreadPoolDemo.Run();

    Console.WriteLine("\n>>> 2.3: همگام‌سازی با lock / Monitor / Mutex");
    ThreadSynchronization.Run();

    Console.WriteLine("\n>>> 2.4: سیگنال‌دهی با AutoResetEvent و ManualResetEventSlim");
    ThreadSignals.Run();

    Console.WriteLine("\n>>> 2.5: مجموعه‌های همزمان ConcurrentDictionary / ConcurrentQueue");
    ConcurrentCollections.Run();
}

// ----------------------------------------------------------------------------
//  بخش ۳: مثال‌های پردازش موازی (Parallel Processing)
// ----------------------------------------------------------------------------
void RunParallelProcessingExamples()
{
    PrintHeader("بخش ۳: پردازش موازی (Parallel Processing)");

    Console.WriteLine("\n>>> 3.1: Parallel.For - حلقه موازی شمارشی");
    ParallelFor.Run();

    Console.WriteLine("\n>>> 3.2: Parallel.ForEach - پیمایش موازی مجموعه");
    ParallelForEach.Run();

    Console.WriteLine("\n>>> 3.3: Parallel.Invoke - فراخوانی موازی چند عملیات");
    ParallelInvoke.Run();

    Console.WriteLine("\n>>> 3.4: PLINQ - LINQ موازی با AsParallel");
    PLINQ.Run();
}

// ----------------------------------------------------------------------------
//  بخش ۴: الگوهای پیشرفته
// ----------------------------------------------------------------------------
async Task RunAdvancedPatterns()
{
    PrintHeader("بخش ۴: الگوهای پیشرفته (Advanced Patterns)");

    Console.WriteLine("\n>>> 4.1: Channels - ارتباط Producer/Consumer");
    await ChannelsDemo.RunAsync();

    Console.WriteLine("\n>>> 4.2: ValueTask - بهینه‌سازی عملکرد async");
    await ValueTaskDemo.RunAsync();

    Console.WriteLine("\n>>> 4.3: TPL Dataflow - خط لوله پردازشی");
    await TplDataflowDemo.RunAsync();
}

// ----------------------------------------------------------------------------
//  تابع کمکی: چاپ سرآیند بخش‌ها
// ----------------------------------------------------------------------------
void PrintHeader(string title)
{
    Console.WriteLine();
    Console.BackgroundColor = ConsoleColor.DarkCyan;
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"  {title}  ");
    Console.ResetColor();
}

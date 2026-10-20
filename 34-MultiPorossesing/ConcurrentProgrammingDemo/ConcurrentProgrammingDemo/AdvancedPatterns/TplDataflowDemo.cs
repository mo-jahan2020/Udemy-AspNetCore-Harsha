// ============================================================================
//  TplDataflowDemo.cs - TPL Dataflow: خط لوله پردازشی
// ============================================================================
//  TPL Dataflow (System.Threading.Tasks.Dataflow) کتابخانه‌ای برای ساخت
//  خطوط لوله (Pipelines) پردازشی است. هر بلوک یک گام است:
//
//   - TransformBlock<TInput, TOutput>  : ورودی → خروجی (تبدیل)
//   - ActionBlock<TInput>              : ورودی → اثر جانبی
//   - BatchBlock<T>                     : چند ورودی → یک آرایه
//   - BroadcastBlock<T>                 : کپی ورودی به همه Linkها
//   - JoinBlock<T1, T2>                 : ترکیب دو جریان
//
//  ویژگی‌ها:
//   - LinkTo بلوک‌ها را به هم وصل می‌کند
//   - MaxDegreeOfParallelism هر بلوک جدا قابل تنظیم است
//   - BoundedCapacity برای Back-pressure
//   - هر بلوک به‌طور داخلی از ThreadPool استفاده می‌کند
//
//  کاربرد: پردازش جریان داده، مانند چند-مرحله‌ای ETL.
// ============================================================================

using System.Threading.Tasks.Dataflow;

namespace ConcurrentProgrammingDemo.AdvancedPatterns;

public static class TplDataflowDemo
{
    // --------------------------------------------------------------------------
    //  RunAsync: یک خط لوله ۳ مرحله‌ای - Download → Process → Save
    // --------------------------------------------------------------------------
    public static async Task RunAsync()
    {
        //  گام ۱: دانلود (تبدیل URL به محتوا)
        var downloadBlock = new TransformBlock<string, string>(async url =>
        {
            await Task.Delay(100); // شبیه‌سازی دانلود
            Console.WriteLine($"  [Dataflow] دانلود شد: {url}");
            return $"محتوای {url}";
        },
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 3, // همزمان ۳ دانلود
            BoundedCapacity = 5
        });

        //  گام ۲: پردازش (تبدیل محتوا به طول)
        var processBlock = new TransformBlock<string, int>(content =>
        {
            int length = content.Length;
            Console.WriteLine($"  [Dataflow] پردازش شد: طول={length}");
            return length;
        },
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 2,
            BoundedCapacity = 5
        });

        //  گام ۳: ذخیره (Action - فقط اثر جانبی)
        var saveBlock = new ActionBlock<int>(length =>
        {
            Console.WriteLine($"  [Dataflow] ذخیره شد: طول={length}");
        },
        new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 1 // ترتیبی
        });

        //  وصل بلوک‌ها با LinkTo
        //  DataflowLinkOptions.PropagateCompletion: وقتی گام اول تمام شد،
        //  گام بعدی هم به حالت Complete می‌رود.
        var linkOpts = new DataflowLinkOptions { PropagateCompletion = true };
        downloadBlock.LinkTo(processBlock, linkOpts);
        processBlock.LinkTo(saveBlock, linkOpts);

        //  ارسال داده به ابتدای خط لوله
        string[] urls = { "url1", "url2", "url3", "url4", "url5" };
        foreach (var url in urls)
        {
            //  SendAsync: اگر کانال پر باشد، مسدود می‌شود (Back-pressure)
            await downloadBlock.SendAsync(url);
        }

        //  اعلام اتمام ارسال
        downloadBlock.Complete();

        //  منتظر بمان تا آخرین گام پایان یابد
        saveBlock.Completion.Wait();
        Console.WriteLine("  [Dataflow] خط لوله پایان یافت ✓");
    }

    // --------------------------------------------------------------------------
    //  BatchBlockExample: تجمیع چند عنصر در یک آرایه
    // --------------------------------------------------------------------------
    public static async Task BatchBlockExample()
    {
        //  BatchBlock<int>(10) ده عنصر را در یک int[] قرار می‌دهد
        var batchBlock = new BatchBlock<int>(batchSize: 3);
        var printBlock = new ActionBlock<int[]>(batch =>
        {
            Console.WriteLine($"  [Dataflow] بسته: [{string.Join(", ", batch)}]");
        });

        batchBlock.LinkTo(printBlock, new DataflowLinkOptions { PropagateCompletion = true });

        for (int i = 0; i < 10; i++)
        {
            await batchBlock.SendAsync(i);
        }

        batchBlock.Complete();
        printBlock.Completion.Wait();
    }
}

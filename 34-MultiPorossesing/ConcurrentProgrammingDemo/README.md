# آموزش اجرای همزمان در C# .NET 10

یک پروژه آموزشی .NET 10 که سه روش اصلی اجرای همزمان (Concurrent Execution)
در C# را با مثال‌های مستقل و کامنت‌های آموزشی فارسی نشان می‌دهد.

## نحوه اجرا

```bash
# با منوی تعاملی
./run.sh

# اجرای همه مثال‌ها
./run.sh 5

# اجرای فقط یک بخش
./run.sh 1   # Asynchronous
./run.sh 2   # Multithreading
./run.sh 3   # Parallel Processing
./run.sh 4   # Advanced Patterns
```

اگر .NET SDK روی سیستم شما نبود، اسکریپت از نسخه محلی موجود در
`/home/z/my-project/dotnet` استفاده می‌کند.

## ساختار پروژه

پروژه شامل **۱۷ فایل .cs جداگانه** است تا خوانایی افزایش یابد:

```
ConcurrentProgrammingDemo/
├── ConcurrentProgrammingDemo.csproj
├── Program.cs                                # منوی اصلی
│
├── AsynchronousProgramming/                 # بخش ۱: ناهمگام (async/await)
│   ├── AsyncBasic.cs                        #   - مفاهیم پایه async/await، Task، Task<T>
│   ├── AsyncWhenAll.cs                      #   - Task.WhenAll و Fan-Out/Fan-In
│   ├── AsyncWhenAny.cs                      #   - Task.WhenAny و الگوی Timeout
│   ├── AsyncStreams.cs                      #   - IAsyncEnumerable<T> و await foreach
│   └── AsyncCancellation.cs                 #   - CancellationTokenSource و لغو
│
├── Multithreading/                          # بخش ۲: چندرشته‌ای
│   ├── ThreadBasic.cs                       #   - Thread، ThreadStart، ParameterizedThreadStart
│   ├── ThreadPoolDemo.cs                    #   - ThreadPool.QueueUserWorkItem
│   ├── ThreadSynchronization.cs             #   - lock، Monitor، Interlocked، SemaphoreSlim
│   ├── ThreadSignals.cs                    #   - AutoResetEvent، ManualResetEventSlim، CountdownEvent، Barrier
│   └── ConcurrentCollections.cs             #   - ConcurrentDictionary، ConcurrentQueue، BlockingCollection
│
├── ParallelProcessing/                      # بخش ۳: پردازش موازی
│   ├── ParallelFor.cs                       #   - Parallel.For با Thread-Local Aggregation
│   ├── ParallelForEach.cs                  #   - Parallel.ForEach و Partitioner
│   ├── ParallelInvoke.cs                   #   - Parallel.Invoke و مدیریت AggregateException
│   └── PLINQ.cs                             #   - AsParallel، AsOrdered، WithCancellation
│
└── AdvancedPatterns/                        # بخش ۴: الگوهای پیشرفته (افزوده‌شده)
    ├── ChannelsDemo.cs                      #   - System.Threading.Channels (Producer/Consumer مدرن)
    ├── ValueTaskDemo.cs                     #   - ValueTask<T> و بهینه‌سازی allocation
    └── TplDataflowDemo.cs                   #   - TPL Dataflow: خط لوله ۳-مرحله‌ای
```

## الگوهای اضافی که اضافه شدند

علاوه بر سه موضوع درخواست‌شده، این الگوهای پیشرفته نیز پوشش داده شدند:

1. **Channels** - Producer/Consumer مدرن ناهمگام با Back-pressure
2. **ValueTask** - بهینه‌سازی Hot Path با کاهش تخصیص حافظه
3. **TPL Dataflow** - ساخت خطوط لوله چندمرحله‌ای
4. **IAsyncEnumerable** - جریان‌های ناهمگام
5. **Partitioner** - کنترل دستی چند بخشی در Parallel.ForEach
6. **Thread-Local Aggregation** - الگوی عملکرد بالا در Parallel.For
7. **CountdownEvent / Barrier** - سیگنال‌دهی پیشرفته بین رشته‌ها

## تفاوت‌های کلیدی سه روش

| روش               | بهترین کاربرد                       | مفهوم                                  |
|-------------------|-------------------------------------|----------------------------------------|
| **async/await**   | I/O-bound (شبکه، فایل، پایگاه داده) | آزادسازی رشته در حین انتظار            |
| **Multithreading** | کارهای بلندمدت CPU-bound            | اجرای واقعی روی چند هسته CPU           |
| **Parallel**       | پردازش موازی داده‌های مستقل         | تقسیم حلقه/مجموعه به چند بخش همزمان   |

## پیش‌نیازها

- .NET 10 SDK (در پروژه گنجانده شده است)
- سیستم‌عامل: Linux / Windows / macOS

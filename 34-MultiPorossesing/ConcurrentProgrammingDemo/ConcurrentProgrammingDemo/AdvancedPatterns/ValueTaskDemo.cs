// ============================================================================
//  ValueTaskDemo.cs - بهینه‌سازی عملکرد با ValueTask
// ============================================================================
//  ValueTask<T> یک struct است که به جای Task<T> (که کلاس است) استفاده می‌شود
//  برای کاهش فشار بر GC (Garbage Collector).
//
//  مزایا:
//   - در سناریوهای هم‌زمان (هم‌زمان) که نتیجه هم‌اکنون آماده است، بدون
//     تخصیص حافظه (allocation) برمی‌گردد.
//   - برای مسیرهای داغ (Hot Path) که ۹۹٪ مواقع هم‌اکنون نتیجه دارند.
//
//  معایب:
//   - نمی‌توان چند بار await کرد (در نسل دوم ظریف است).
//   - ترکیب با Task.WhenAll پیچیده‌تر است (نیاز به AsTask()).
//   - برای مسیرهای سرد (Cold Path) که همیشه I/O دارند، سودی ندارد.
//
//  قانون: از ValueTask استفاده کنید فقط اگر کار بسیار سریع‌تر کامل می‌شود.
//          در غیر این صورت، Task پیش‌فرض است.
// ============================================================================

namespace ConcurrentProgrammingDemo.AdvancedPatterns;

public static class ValueTaskDemo
{
    // --------------------------------------------------------------------------
    //  RunAsync: مقایسه Task با ValueTask
    // --------------------------------------------------------------------------
    public static async Task RunAsync()
    {
        Console.WriteLine("  [ValueTask] سناریو ۱: مقدار کش‌شده (Cached Value)");
        await CachedValueExampleAsync();

        Console.WriteLine("  [ValueTask] سناریو ۲: مسیر سرد با تأخیر");
        await ColdPathExampleAsync();
    }

    // ==========================================================================
    //  سناریو ۱: مسیر داغ با مقدار کش‌شده - ValueTask بدون تخصیص
    // ==========================================================================
    //  این نمونه یک نمونه واقعی است: اگر در حافظه هستیم، هیچ Task تولید نمی‌شود.
    //  اگر نباشیم، ValueTask به‌صورت داخلی یک Task می‌سازد.
    // ==========================================================================
    private static int _cachedValue;
    private static Task<int>? _cacheLoader;

    public static async ValueTask<int> GetCachedValueAsync()
    {
        if (_cachedValue != 0)
        {
            //  مسیر داغ: هیچ تخصیصی نیست - ValueTask<int> مستقیماً مقدار را نگه می‌دارد
            return _cachedValue;
        }

        if (_cacheLoader == null)
        {
            //  مسیر سرد: Task می‌سازیم
            _cacheLoader = LoadFromSourceAsync();
        }

        //  در مسیر سرد، ValueTask به Task داخلی منتقل می‌شود
        _cachedValue = await _cacheLoader;
        _cacheLoader = null;
        return _cachedValue;
    }

    private static async Task<int> LoadFromSourceAsync()
    {
        Console.WriteLine("  [ValueTask] بارگیری مقدار از منبع...");
        await Task.Delay(300);
        return 42;
    }

    private static async Task CachedValueExampleAsync()
    {
        //  بار نخست: مسیر سرد - Task تولید می‌شود
        int v1 = await GetCachedValueAsync();
        Console.WriteLine($"  [ValueTask] فراخوانی اول: {v1}");

        //  بار دوم: مسیر داغ - هیچ تخصیصی نیست
        int v2 = await GetCachedValueAsync();
        Console.WriteLine($"  [ValueTask] فراخوانی دوم (داغ): {v2}");

        //  بار سوم: دوباره مسیر داغ
        int v3 = await GetCachedValueAsync();
        Console.WriteLine($"  [ValueTask] فراخوانی سوم (داغ): {v3}");
    }

    // ==========================================================================
    //  سناریو ۲: مسیر سرد - ValueTask با تأخیر شبیه‌سازی
    // ==========================================================================
    private static async ValueTask<DateTime> GetTimeAsync(bool fromCache)
    {
        if (fromCache)
        {
            //  مسیر داغ: مستقیماً برگردان، بدون ساخت Task
            return DateTime.Now;
        }

        //  مسیر سرد: تأخیر واقعی - ValueTask به Task داخلی تبدیل می‌شود
        await Task.Delay(100);
        return DateTime.Now;
    }

    private static async Task ColdPathExampleAsync()
    {
        //  مسیر سرد: تأخیر واقعی
        DateTime t1 = await GetTimeAsync(fromCache: false);
        Console.WriteLine($"  [ValueTask] مسیر سرد: {t1:O}");

        //  مسیر داغ
        DateTime t2 = await GetTimeAsync(fromCache: true);
        Console.WriteLine($"  [ValueTask] مسیر داغ: {t2:O}");
    }
}

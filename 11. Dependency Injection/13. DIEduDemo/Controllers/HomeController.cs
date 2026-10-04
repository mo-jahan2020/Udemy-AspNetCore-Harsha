// ============================================================================
//  HomeController.cs — کنترلر اصلی (نسخه کامل با Keyed Services و DbContext)
// ----------------------------------------------------------------------------
//  در این کنترلر:
//   - Constructor Injection: چندین سرویس به Constructor تزریق می‌شوند.
//   - Method Injection: با [FromServices] در اکشن متدها استفاده می‌کنیم.
//   - Keyed Services: با [FromKeyedServices("key")] به سرویس‌های Keyed دسترسی داریم.
//   - DbContext: از IProductRepository برای کار با دیتابیس استفاده می‌کنیم.
//   - مقایسه Lifetime: دو بار از هر سرویس نمونه می‌گیریم و در خروجی مقایسه می‌کنیم.
// ============================================================================

using DIEduDemo.Data;
using DIEduDemo.Models;
using DIEduDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DIEduDemo.Controllers;

public class HomeController : Controller
{
    // فیلدهای readonly برای نگهداری وابستگی‌های تزریق‌شده در Constructor
    private readonly ConstructorInjectionDemo _ctorDemo;
    private readonly MethodInjectionDemo _methodDemo;
    private readonly PropertyInjectionDemo _propertyDemo;
    private readonly IOrderProcessor _orderProcessor;
    private readonly IProductRepository _productRepo;
    private readonly AppDbContext _dbContext;

    // سرویس داخلی ASP.NET Core: IWebHostEnvironment برای دسترسی به اطلاعات محیط
    private readonly IWebHostEnvironment _env;

    // سرویس داخلی ASP.NET Core: ILogger<T> برای لاگ‌گیری
    private readonly ILogger<HomeController> _logger;

    /// <summary>
    /// Constructor Injection — چندین وابستگی به‌صورت همزمان تزریق می‌شوند.
    /// این رایج‌ترین روش تزریق در ASP.NET Core است.
    /// </summary>
    public HomeController(
        ConstructorInjectionDemo ctorDemo,
        MethodInjectionDemo methodDemo,
        PropertyInjectionDemo propertyDemo,
        IOrderProcessor orderProcessor,
        IProductRepository productRepo,
        AppDbContext dbContext,
        IWebHostEnvironment env,
        ILogger<HomeController> logger)
    {
        _ctorDemo = ctorDemo;
        _methodDemo = methodDemo;
        _propertyDemo = propertyDemo;
        _orderProcessor = orderProcessor;
        _productRepo = productRepo;
        _dbContext = dbContext;
        _env = env;
        _logger = logger;

        _logger.LogInformation("HomeController ساخته شد. Environment={Env}", _env.EnvironmentName);
    }

    /// <summary>
    /// اکشن اصلی: مقایسه Lifetimeها، Keyed Services، DbContext و انواع تزریق
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var sp = HttpContext.RequestServices;

        // ----- بخش ۱: مقایسه Lifetime -----
        var t1 = sp.GetRequiredService<IOperationTransient>();
        var t2 = sp.GetRequiredService<IOperationTransient>();
        var s1 = sp.GetRequiredService<IOperationScoped>();
        var s2 = sp.GetRequiredService<IOperationScoped>();
        var singleton = sp.GetRequiredService<IOperationSingleton>();

        // ----- بخش ۲: Keyed Services -----
        // گرفتن هر سه سرویس Keyed با کلیدهایشان
        var emailSvc = sp.GetRequiredKeyedService<INotificationService>("email");
        var smsSvc = sp.GetRequiredKeyedService<INotificationService>("sms");
        var pushSvc = sp.GetRequiredKeyedService<INotificationService>("push");

        var keyedServices = new List<KeyedServiceInfo>
        {
            new()
            {
                Key = "email",
                ChannelName = emailSvc.ChannelName,
                ImplementationName = emailSvc.GetType().Name,
                SendResult = emailSvc.Send("user@example.com", "سلام از Keyed Service ایمیل!")
            },
            new()
            {
                Key = "sms",
                ChannelName = smsSvc.ChannelName,
                ImplementationName = smsSvc.GetType().Name,
                SendResult = smsSvc.Send("09120000000", "کد تأیید: 1234")
            },
            new()
            {
                Key = "push",
                ChannelName = pushSvc.ChannelName,
                ImplementationName = pushSvc.GetType().Name,
                SendResult = pushSvc.Send("device-001", "شما یک اعلان جدید دارید")
            }
        };

        // ----- بخش ۳: DbContext (خواندن محصولات) -----
        var products = await _productRepo.GetAllAsync();
        var productCount = await _productRepo.CountAsync();

        var model = new LifetimeComparisonModel
        {
            EnvironmentName = _env.EnvironmentName,
            RequestTime = DateTime.Now,
            Rows = new List<LifetimeRow>
            {
                new()
                {
                    Lifetime = "Transient",
                    FirstCallGuid = t1.OperationId,
                    SecondCallGuid = t2.OperationId,
                    Description = "هر بار درخواست → یک نمونه جدید. مناسب سرویس‌های سبک و Stateless."
                },
                new()
                {
                    Lifetime = "Scoped",
                    FirstCallGuid = s1.OperationId,
                    SecondCallGuid = s2.OperationId,
                    Description = "در یک Request ثابت، در Request بعدی نمونه جدید. مناسب DbContext."
                },
                new()
                {
                    Lifetime = "Singleton",
                    FirstCallGuid = singleton.OperationId,
                    SecondCallGuid = singleton.OperationId,
                    Description = "در کل عمر برنامه فقط یک نمونه. مناسب Cache و سرویس‌های گران. باید Thread-safe باشد."
                }
            },

            // Keyed Services
            KeyedServices = keyedServices,

            // DbContext
            Products = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CreatedAt = p.CreatedAt
            }).ToList(),
            ProductCount = productCount,
            DatabaseName = "DIEduDemoDb",
            DatabaseProvider = "EF Core In-Memory"
        };

        // ----- بخش ۴: انواع تزریق -----
        var ids = _ctorDemo.GetCurrentIds();
        model.ConstructorInjectionResult =
            $"Transient:  {ids.Transient}\n" +
            $"Scoped:     {ids.Scoped}\n" +
            $"Singleton:  {ids.Singleton}";

        model.PropertyInjectionResult = _propertyDemo.Describe();

        var scopedService = sp.GetRequiredService<IOperationScoped>();
        model.MethodInjectionResult = _methodDemo.ProcessIt(scopedService);

        model.OrderProcessorResult = _orderProcessor.ProcessOrder(orderId: 1001);

        return View(model);
    }

    // =====================================================================
    // endpointهای AJAX برای Keyed Services
    // =====================================================================

    /// <summary>
    /// اکشن AJAX برای فراخوانی سرویس Keyed بر اساس کلید دریافتی.
    /// ----------------------------------------------------------------------------
    /// نکته آموزشی مهم:
    /// ویژگی [FromKeyedServices("key")] کلید را به‌صورت ایستا (Literal) می‌گیرد،
    /// یعنی نمی‌توان کلید را به‌صورت داینامیک از query string پاس داد.
    /// برای حالت داینامیک، باید از IServiceProvider.GetRequiredKeyedService<T>(key)
    /// استفاده کنیم — همان‌طور که در این اکشن انجام داده‌ایم.
    ///
    /// مثال حالت ایستا (در Constructor یا پارامتر اکشن):
    ///   public IActionResult Sample([FromKeyedServices("email")] INotificationService svc)
    ///
    /// مثال حالت داینامیک (در بدنه‌ی متد):
    ///   var svc = sp.GetRequiredKeyedService<INotificationService>("email");
    /// </summary>
    [HttpGet]
    public IActionResult SendKeyedNotification(string channel, string recipient, string message)
    {
        // گرفتن سرویس Keyed به‌صورت داینامیک با کلید از query string
        var sp = HttpContext.RequestServices;

        if (string.IsNullOrWhiteSpace(channel))
        {
            return Ok(new { success = false, error = "پارامتر channel الزامی است (email/sms/push)." });
        }

        // تلاش برای گرفتن سرویس با کلید درخواستی
        var keyedService = sp.GetKeyedService<INotificationService>(channel);
        if (keyedService is null)
        {
            return Ok(new
            {
                success = false,
                error = $"کلید '{channel}' ثبت نشده است. کلیدهای معتبر: email, sms, push"
            });
        }

        if (string.IsNullOrWhiteSpace(recipient))
            recipient = "(گیرنده نامشخص)";

        if (string.IsNullOrWhiteSpace(message))
            message = "(بدون متن)";

        var result = keyedService.Send(recipient, message);

        return Ok(new
        {
            success = true,
            channel = keyedService.ChannelName,
            implementation = keyedService.GetType().Name,
            recipient = recipient,
            message = message,
            result = result,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });
    }

    /// <summary>
    /// اکشن نمونه برای نمایش [FromKeyedServices] به‌صورت ایستا در پارامتر اکشن متد.
    /// این روش زمانی استفاده می‌شود که کلید از قبل مشخص باشد.
    /// </summary>
    [HttpGet]
    public IActionResult StaticKeyedExample([FromKeyedServices("email")] INotificationService emailSvc)
    {
        var result = emailSvc.Send("static@example.com", "این فراخوانی با [FromKeyedServices(\"email\")] است");
        return Ok(new
        {
            success = true,
            channel = emailSvc.ChannelName,
            implementation = emailSvc.GetType().Name,
            result = result,
            note = "این سرویس با [FromKeyedServices(\"email\")] به‌صورت ایستا در پارامتر اکشن تزریق شده است.",
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });
    }

    /// <summary>
    /// اکشن AJAX که همه‌ی کلیدهای ثبت‌شده‌ی Keyed را برمی‌گرداند.
    /// از این برای ساخت دکمه‌های پویا در View استفاده می‌کنیم.
    /// </summary>
    [HttpGet]
    public IActionResult ListKeyedServices()
    {
        // دریافت همه‌ی کلیدهای ثبت‌شده برای INotificationService
        // این کار با استفاده از IServiceCollection در زمان build انجام می‌شود،
        // اما برای نمونه، کلیدها را به‌صورت hard-coded برمی‌گردانیم.
        return Ok(new
        {
            success = true,
            keys = new[] { "email", "sms", "push" }
        });
    }

    // =====================================================================
    // endpointهای AJAX برای DbContext
    // =====================================================================

    /// <summary>
    /// اکشن AJAX برای افزودن محصول جدید به دیتابیس.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddProduct([FromForm] string name, [FromForm] decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Ok(new { success = false, error = "نام محصول الزامی است." });
        }

        if (price <= 0)
        {
            return Ok(new { success = false, error = "قیمت باید بزرگتر از صفر باشد." });
        }

        var product = await _productRepo.AddAsync(name, price);
        var newCount = await _productRepo.CountAsync();

        return Ok(new
        {
            success = true,
            message = $"محصول '{product.Name}' با موفقیت اضافه شد.",
            product = new
            {
                id = product.Id,
                name = product.Name,
                price = product.Price,
                createdAt = product.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            },
            totalCount = newCount
        });
    }

    /// <summary>
    /// اکشن AJAX برای گرفتن لیست به‌روزشده‌ی محصولات.
    /// این برای refresh جدول محصولات بعد از افزودن محصول جدید استفاده می‌شود.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListProducts()
    {
        var products = await _productRepo.GetAllAsync();
        var count = await _productRepo.CountAsync();

        return Ok(new
        {
            success = true,
            count = count,
            products = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                price = p.Price,
                createdAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            })
        });
    }

    // =====================================================================
    // endpointهای AJAX قدیمی (Lifetime)
    // =====================================================================

    /// <summary>
    /// اکشن AJAX برای نمایش Method Injection با [FromServices]
    /// </summary>
    [HttpGet]
    public IActionResult MethodInjectionAjax([FromServices] IOperationSingleton singletonOp)
    {
        var envName = _env.EnvironmentName;
        return Ok(new
        {
            success = true,
            message = "Method Injection با [FromServices] با موفقیت اجرا شد!",
            singletonGuid = singletonOp.OperationId,
            lifetimeName = singletonOp.LifetimeName,
            environment = envName,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            note = "این سرویس Singleton است، پس Guid در تمام درخواست‌ها یکسان خواهد بود."
        });
    }

    /// <summary>
    /// اکشن AJAX برای نمایش تفاوت Transient با هر بار فراخوانی
    /// </summary>
    [HttpGet]
    public IActionResult TransientDemoAjax([FromServices] IOperationTransient transientOp)
    {
        return Ok(new
        {
            success = true,
            transientGuid = transientOp.OperationId,
            message = "هر بار که این دکمه را بزنید، یک Guid جدید می‌بینید — چون Transient هر بار یک نمونه جدید می‌سازد.",
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
        });
    }

    /// <summary>
    /// اکشن نمونه برای Method Injection با [FromServices] (نسخه غیر AJAX)
    /// </summary>
    public IActionResult MethodInjectionExample([FromServices] IOperationSingleton singletonOp)
    {
        ViewData["MethodInjectionExampleMessage"] =
            $"✅ Method Injection با [FromServices] اجرا شد. Guid Singleton = {singletonOp.OperationId}";
        return View("Index", BuildDefaultModel().Result);
    }

    /// <summary>
    /// ساختن مدل پیش‌فرض برای اکشن متدهای دیگر
    /// </summary>
    private async Task<LifetimeComparisonModel> BuildDefaultModel()
    {
        var sp = HttpContext.RequestServices;
        var t1 = sp.GetRequiredService<IOperationTransient>();
        var t2 = sp.GetRequiredService<IOperationTransient>();
        var s1 = sp.GetRequiredService<IOperationScoped>();
        var s2 = sp.GetRequiredService<IOperationScoped>();
        var singleton = sp.GetRequiredService<IOperationSingleton>();
        var scopedService = sp.GetRequiredService<IOperationScoped>();

        // Keyed Services
        var emailSvc = sp.GetRequiredKeyedService<INotificationService>("email");
        var smsSvc = sp.GetRequiredKeyedService<INotificationService>("sms");
        var pushSvc = sp.GetRequiredKeyedService<INotificationService>("push");

        // DbContext
        var products = await _productRepo.GetAllAsync();
        var productCount = await _productRepo.CountAsync();

        var ids = _ctorDemo.GetCurrentIds();

        return new LifetimeComparisonModel
        {
            EnvironmentName = _env.EnvironmentName,
            RequestTime = DateTime.Now,
            Rows = new List<LifetimeRow>
            {
                new()
                {
                    Lifetime = "Transient",
                    FirstCallGuid = t1.OperationId,
                    SecondCallGuid = t2.OperationId,
                    Description = "هر بار درخواست → یک نمونه جدید. مناسب سرویس‌های سبک و Stateless."
                },
                new()
                {
                    Lifetime = "Scoped",
                    FirstCallGuid = s1.OperationId,
                    SecondCallGuid = s2.OperationId,
                    Description = "در یک Request ثابت، در Request بعدی نمونه جدید. مناسب DbContext."
                },
                new()
                {
                    Lifetime = "Singleton",
                    FirstCallGuid = singleton.OperationId,
                    SecondCallGuid = singleton.OperationId,
                    Description = "در کل عمر برنامه فقط یک نمونه. مناسب Cache و سرویس‌های گران. باید Thread-safe باشد."
                }
            },
            KeyedServices = new List<KeyedServiceInfo>
            {
                new()
                {
                    Key = "email",
                    ChannelName = emailSvc.ChannelName,
                    ImplementationName = emailSvc.GetType().Name,
                    SendResult = emailSvc.Send("user@example.com", "سلام از Keyed Service ایمیل!")
                },
                new()
                {
                    Key = "sms",
                    ChannelName = smsSvc.ChannelName,
                    ImplementationName = smsSvc.GetType().Name,
                    SendResult = smsSvc.Send("09120000000", "کد تأیید: 1234")
                },
                new()
                {
                    Key = "push",
                    ChannelName = pushSvc.ChannelName,
                    ImplementationName = pushSvc.GetType().Name,
                    SendResult = pushSvc.Send("device-001", "شما یک اعلان جدید دارید")
                }
            },
            Products = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CreatedAt = p.CreatedAt
            }).ToList(),
            ProductCount = productCount,
            DatabaseName = "DIEduDemoDb",
            DatabaseProvider = "EF Core In-Memory",
            ConstructorInjectionResult =
                $"Transient:  {ids.Transient}\n" +
                $"Scoped:     {ids.Scoped}\n" +
                $"Singleton:  {ids.Singleton}",
            MethodInjectionResult = _methodDemo.ProcessIt(scopedService),
            PropertyInjectionResult = _propertyDemo.Describe(),
            OrderProcessorResult = _orderProcessor.ProcessOrder(orderId: 2002)
        };
    }

    /// <summary>
    /// صفحه خطا
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View("Error");
    }
}

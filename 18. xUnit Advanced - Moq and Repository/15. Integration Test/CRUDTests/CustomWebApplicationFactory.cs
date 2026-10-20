using Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.DependencyInjection;
/*
 * کارکرد این فایل:
 - یک نسخه Test از برنامه وب را بالا می‌آورد                
 - DbContext واقعی (SQL Server) را حذف می‌کند              
 - به‌جای آن In-Memory Database نصب می‌کند
 */

namespace CRUDTests
{
    // WebApplicationFactory<Program> یک کلاس از Microsoft.AspNetCore.Mvc.Testing است که:
    //   1) Program.cs برنامه‌ی وب ما را پیدا می‌کند (با reflection روی partial Program در .NET 6+)
    //   2) Host برنامه را در حافظه بالا می‌آورد (بدون باز کردن پورت شبکه‌ای واقعی)
    //   3) به ما اجازه می‌دهد قبل از بالا آمدن کامل، تنظیمات DI را تغییر دهیم
    //
    // چرا <Program>؟
    // در .NET 6+ برنامه از top-level statements استفاده می‌کند و کلاس Program
    // به‌طور ضمنی تعریف می‌شود. WebApplicationFactory آن را پیدا می‌کند.
    // (در نسخه‌های قدیمی‌تر از Startup استفاده می‌شد.)
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        // ConfigureWebHost را override می‌کنیم تا بتوانیم تنظیمات Host را شخصی‌سازی کنیم.
        // builder همان IWebHostBuilder است که قبل از Build شدن، در دسترس ماست.
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // ابتدا base.ConfigureWebHost را صدا می‌زنیم تا پیکربندی پیش‌فرض انجام شود.
            // اگر این را فراموش کنیم، برنامه‌ی وب واقعاً بالا نمی‌آید.
            base.ConfigureWebHost(builder);

            // محیط را روی "Test" تنظیم می‌کنیم.
            // این کار باعث می‌شود:
            //   - برنامه فایل appsettings.Test.json (اگر باشد) را بخواند
            //   - در کد می‌توانیم با builder.Environment.IsTest() رفتار خاص تست داشته باشیم
            // معمولاً سه محیط رایج داریم: Development, Staging, Production. "Test" یک مورد اضافی است.
            builder.UseEnvironment("Test");

            // ConfigureServices به ما اجازه می‌دهد سرویس‌های ثبت‌شده در DI Container را
            // قبل از Build شدن Host تغییر دهیم. اینجا می‌خواهیم DbContext واقعی را عوض کنیم.
            builder.ConfigureServices(services => {

                // هدف: پیدا کردن ثبت DbContextOptions<ApplicationDbContext> که برنامه برای SQL Server انجام داده.
                //
                // هر سرویس در DI Container به‌صورت یک ServiceDescriptor ثبت می‌شود که شامل:
                //   - ServiceType (اینترفیس یا کلاسی که با آن resolve می‌شود)
                //   - ImplementationType یا ImplementationFactory
                //   - Lifetime (Singleton, Scoped, Transient)
                //
                // SingleOrDefault: اگر دقیقاً یک مورد باشد برمی‌گرداند،
                //                  اگر هیچ مورد نباشد null برمی‌گرداند،
                //                  اگر چند مورد باشد exception می‌دهد.
                var descripter = services.SingleOrDefault(
                    temp => temp.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)
                );

                // اگر descriptor پیدا شد (یعنی Program.cs واقعاً DbContext ثبت کرده)، آن را حذف می‌کنیم.
                // این کار لازم است چون می‌خواهیم به‌جای آن یک DbContext با In-Memory Database ثبت کنیم.
                // اگر حذف نکنیم، دو ثبت همزمان داریم که ابهام ایجاد می‌کند.
                if (descripter != null)
                {
                    services.Remove(descripter);
                }

                // حالا یک DbContext جدید ثبت می‌کنیم که از In-Memory Database استفاده می‌کند.
                // AddDbContext در واقع DbContextOptions<ApplicationDbContext> را هم ثبت می‌کند.
                //
                // چرا In-Memory و نه SQL Server واقعی؟
                //   1) نیاز به SQL Server نصب‌شده ندارد
                //   2) هر بار که تست اجرا می‌شود، یک دیتابیس تازه در حافظه ساخته می‌شود
                //   3) تست‌ها سریع اجرا می‌شوند و قابل تکرارند
                //   4) داده‌ها بین تست‌ها به‌طور خودکار پاک می‌شوند (هر Test Class یک instance جدید دارد)
                //
                // نکته: نام "DatbaseForTesting" یک شناسه است. (اشتباه تایپی "Datbase" به‌جای "Database"
                //       است ولی از نظر فنی فرقی نمی‌کند.) اگر چند Factory با همین نام بسازید،
                //       همان دیتابیس را به‌اشتراک می‌گذارند.
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DatbaseForTesting");
                });
            });
        }
    }
}

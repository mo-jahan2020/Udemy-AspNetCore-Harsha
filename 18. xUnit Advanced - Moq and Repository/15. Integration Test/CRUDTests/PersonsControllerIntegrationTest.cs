using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
/*
 * این دو فایل با هم یک Integration Test کامل برای ASP.NET Core
 * می‌سازند. برخلاف Unit Test (که در فصل قبلی دیدیم و فقط یک کلاس را با Moq ایزوله تست می‌کرد)،
 * Integration Test کل برنامه را از بیرون یعنی از طریق HTTP
 * اجرا می‌کند و رفتار واقعی آن را از لایه Controller تا دیتابیس بررسی می‌کند.
 */
/*
 * کارکرد این فایل:
 * - یک HttpClient می‌سازد                                    
   - درخواست GET /Persons/Index می‌فرستد                      
   - وضعیت پاسخ را با FluentAssertions بررسی می‌کند
 */
namespace CRUDTests
{
    // چرا IClassFixture<CustomWebApplicationFactory>؟
    // xUnit به‌صورت پیش‌فرض برای هر [Fact] یک instance جدید از کلاس تست می‌سازد.
    // اگر بخواهیم یک منبع گران‌قیمت (مثل Host برنامه) را بین همه‌ی تست‌ها به‌اشتراک بگذاریم،
    // از IClassFixture<T> استفاده می‌کنیم. اینجا T همان CustomWebApplicationFactory است.
    // xUnit فقط یک بار factory را می‌سازد و به تمام متدهای تست تزریق می‌کند.
    public class PersonsControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        // HttpClient یک منبع است که بعد از استفاده باید Dispose شود.
        // CreateClient یک HttpClient می‌سازد که:
        //   1) به‌جای شبکه فیزیکی، مستقیم به Host درون‌حافظه‌ای وصل می‌شود
        //   2) به‌طور خودکار BaseAddress و هدرها را تنظیم می‌کند
        //   3) با Dispose شدن factory، خودش هم پاکسازی می‌شود
        private readonly HttpClient _client;

        // پارامتر factory توسط xUnit تزریق می‌شود (به‌خاطر IClassFixture).
        // این سازنده قبل از هر [Fact] اجرا می‌شود، ولی خود factory فقط یک بار ساخته شده.
        public PersonsControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }


        #region Index

        // [Fact] یعنی: این متد یک تست است.
        // [Fact] بدون پارامتر است؛ اگر ورودی‌های متفاوت خواستیم از [Theory] + [InlineData] استفاده می‌کنیم.
        [Fact]
        public async Task Index_ToReturnView()
        {
            // Arrange
            // در این تست چون فقط می‌خواهیم route کار کند، چیزی برای آماده‌سازی نیست.
            // در تست‌های کامل‌تر، ممکن است اینجا داده‌های اولیه داخل In-Memory DB وارد کنیم.


            // Act
            // یک درخواست GET واقعی به مسیر /Persons/Index می‌فرستیم.
            // نکته: چون از WebApplicationFactory استفاده می‌کنیم، درخواست واقعاً وارد
            //       pipeline برنامه می‌شود: Routing → Controller → View → ...
            //       اما بدون باز کردن پورت شبکه‌ای انجام می‌شود.
            HttpResponseMessage response = await _client.GetAsync("/Persons/Index");

            // Assert
            // IsSuccessStatusCode وقتی true می‌شود که کد وضعیت 2xx باشد (200, 201, 204, ...).
            //
            // FluentAssertions به‌جای Assert.True(...) از سبک روان‌تر استفاده می‌کند:
            //   "response.IsSuccessStatusCode باید true باشد."
            // مزایای FluentAssertions:
            //   1) خواناتر است
            //   2) پیام خطای بهتری در صورت شکست تست تولید می‌کند
            //   3) IntelliSense با Should شروع می‌شود و تمام assertionهای ممکن را پیشنهاد می‌دهد
            response.IsSuccessStatusCode.Should().BeTrue(); //2xx
        }

        #endregion
    }
}
 
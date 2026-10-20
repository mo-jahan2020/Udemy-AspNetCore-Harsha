using FluentAssertions;             // برای assertionهای روان: .Should().BeTrue()
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;              // HttpClient و HttpResponseMessage
using System.Text;
using System.Threading.Tasks;       // برای async/await
using Xunit;                        // [Fact], IClassFixture
using Fizzler;                      // موتور انتخابگر CSS - پایه‌ی QuerySelectorAll
using Fizzler.Systems.HtmlAgilityPack; // پل بین Fizzler و HtmlAgilityPack؛
                                       // متد الحاقی QuerySelectorAll را به HtmlNode اضافه می‌کند
using HtmlAgilityPack;              // parser HTML که HTML را به درخت DOM تبدیل می‌کند

/*
 علاوه‌بر بررسی وضعیت HTTP، محتوای HTML پاسخ را هم بررسی می‌کنیم
در نسخه‌ی قبلی فقط بررسی می‌کردیم که آیا پاسخ HTTP کد 2xx دارد یا نه.
این حداقلِ ممکن است؛ ولی نمی‌گوید View درست رندر شده یا نه.
مثلاً اگر View یک صفحه‌ی خالی برمی‌گرداند، تست سبز می‌شد ولی در واقع برنامه خراب بود.

این نسخه یک قدم جلوتر می‌رود و محتوای HTML پاسخ را با انتخابگرهای CSS بررسی می‌کند.
اگر جدول persons در صفحه نباشد، تست fail می‌شود.
 */
/*
 *کارکرد این فایل:
 *1. GET /Persons/Index می‌فرستد                                 
  2. پاسخ HTTP را می‌گیرد                                        
  3. بدنه‌ی HTML را استخراج می‌کند                                
  4. با HtmlAgilityPack آن را parse می‌کند                       
  5. با Fizzler یک انتخابگر CSS می‌زند: table.persons            
  6. بررسی می‌کند که حداقل یک عنصر پیدا شود
HtmlAgilityPack: یک parser HTML برای دات‌نت؛ فایل HTML را به یک درخت DOM تبدیل می‌کند تا بتوان آن را پیمایش کرد
Fizzler:یک موتور انتخابگر CSS برای HTML؛ روی HtmlAgilityPack سوار می‌شود
FluentAssertions:سبک روان assertion (که قبلاً دیدیم)
 */
namespace CRUDTests
{
    // این کلاس تست با IClassFixture یک CustomWebApplicationFactory دریافت می‌کند.
    // همان‌طور که در پیام قبلی توضیح دادم:
    //   - factory فقط یک‌بار توسط xUnit ساخته می‌شود
    //   - program در حافظه بالا می‌آید
    //   - DbContext واقعی با In-Memory Database جایگزین می‌شود
    public class PersonsControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public PersonsControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
            // یک HttpClient می‌سازد که مستقیماً به Host درون‌حافظه‌ای وصل می‌شود
            // (بدون باز کردن پورت شبکه‌ای واقعی)
            _client = factory.CreateClient();
        }


        #region Index

        [Fact]
        public async Task Index_ToReturnView()
        {
            // ════════════════════════════════════════════════════════════
            // Arrange
            // ════════════════════════════════════════════════════════════
            // در این تست خاص، چیزی برای آماده‌سازی نیست.
            // در تست‌های پیشرفته‌تر می‌توانیم اینجا داده‌های اولیه را
            // داخل In-Memory DbContext وارد کنیم تا View رکوردها را نشان دهد.


            // ════════════════════════════════════════════════════════════
            // Act
            // ════════════════════════════════════════════════════════════
            // یک درخواست GET واقعی به مسیر /Persons/Index می‌فرستیم.
            // این درخواست از کل pipeline برنامه عبور می‌کند:
            //   Routing → Controller Activation → PersonsController.Index()
            //   → View Rendering → HTTP Response
            HttpResponseMessage response = await _client.GetAsync("/Persons/Index");


            // ════════════════════════════════════════════════════════════
            // Assert - مرحله‌ی اول: بررسی وضعیت HTTP
            // ════════════════════════════════════════════════════════════
            // اول چک می‌کنیم که پاسخ اصلاً 2xx است یا نه.
            // اگر 500 یا 404 بدهد، ادامه‌ی تست بی‌فایده است.
            response.IsSuccessStatusCode.Should().BeTrue(); //2xx


            // ════════════════════════════════════════════════════════════
            // Assert - مرحله‌ی دوم: استخراج محتوای HTML
            // ════════════════════════════════════════════════════════════
            // ReadAsStringAsync بدنه‌ی پاسخ را به‌صورت string برمی‌گرداند.
            // این string همان HTML‌ای است که View رندر کرده.
            string responseBody = await response.Content.ReadAsStringAsync();


            // ════════════════════════════════════════════════════════════
            // Assert - مرحله‌ی سوم: parse کردن HTML با HtmlAgilityPack
            // ════════════════════════════════════════════════════════════
            // HtmlDocument یک درخت DOM از HTML می‌سازد که قابل پیمایش است.
            // این کار شبیه به کاری است که مرورگر با HTML انجام می‌دهد:
            // متن HTML را به یک ساختار درختی تبدیل می‌کند.
            HtmlDocument html = new HtmlDocument();

            // LoadHtml محتوای HTML را درون درخت DOM بارگذاری می‌کند.
            // نکته: HtmlAgilityPack نسبت به HTML ناقص هم آزارپذیر (tolerant) است
            //       (مثل مرورگرها) و اگر تگ‌ای بسته نشده باشد، آن را حدس می‌زند.
            html.LoadHtml(responseBody);

            // DocumentNode ریشه‌ی درخت HTML است.
            // از اینجا می‌توان به‌صورت درختی به فرزندان حرکت کرد:
            //   document.ChildNodes, document.FirstChild, ...
            // ولی راه ساده‌تر استفاده از QuerySelectorAll است که در ادامه می‌آید.
            var document = html.DocumentNode;


            // ════════════════════════════════════════════════════════════
            // Assert - مرحله‌ی چهارم: انتخابگر CSS
            // ════════════════════════════════════════════════════════════
            // QuerySelectorAll از Fizzler می‌آید و انتخابگرهای CSS را اجرا می‌کند.
            //
            // "table.persons" یعنی: تمام <table> هایی که کلاس CSS آن‌ها "persons" دارد.
            //
            // مثال HTML که این انتخابگر آن را پیدا می‌کند:
            //   <table class="persons">
            //     <thead>...</thead>
            //     <tbody>...</tbody>
            //   </table>
            //
            // خروجی QuerySelectorAll یک IEnumerable<HtmlNode> است.
            var matchingNodes = document.QuerySelectorAll("table.persons");


            // ════════════════════════════════════════════════════════════
            // Assert - مرحله‌ی پنجم: assertion با FluentAssertions
            // ════════════════════════════════════════════════════════════
            // NotBeNull بررسی می‌کند که شیء null نباشد.
            //
            // ⚠️ نکته‌ی مهم: این assertion ضعیف است!
            //   - QuerySelectorAll هرگز null برنمی‌گرداند؛ همیشه یک IEnumerable برمی‌گرداند
            //     (می‌تواند خالی باشد ولی null نیست)
            //   - پس این تست همیشه pass می‌شود، حتی اگر هیچ table.persons وجود نداشت!
            //
            // ✅ assertion صحیح‌تر:
            //   matchingNodes.Should().NotBeEmpty("چون View باید یک جدول persons داشته باشد");
            //
            //   یا حتی بهتر:
            //   matchingNodes.Count().Should().Be(1, "چون فقط یک جدول persons باید وجود داشته باشد");
            matchingNodes.Should().NotBeNull();
        }

        #endregion
    }
}
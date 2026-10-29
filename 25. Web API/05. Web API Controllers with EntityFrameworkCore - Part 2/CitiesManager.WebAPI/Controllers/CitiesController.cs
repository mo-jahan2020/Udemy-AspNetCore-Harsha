using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitiesManager.WebAPI.DatabaseContext;
using CitiesManager.WebAPI.Models;

namespace CitiesManager.WebAPI.Controllers
{
    // [ApiController] وجود این خصوصیت باعث شش اتفاق و امکان در این برنمه و منترلر میشود
    /*
     * 1- استنتاج منبع Binding : دیگر نیازی به نوشتن [FromBody]، [FromRoute] و... روی پارامترها نیست. فریم‌ورک خودش حدس می‌زند
     * 2- اعتبارسنجی خودکار مدل بدون if (!ModelState.IsValid) : گر مدل نامعتبر باشد (مثلاً CityName با [Required] خالی باشد)، قبل از ورود به کد اکشن، پاسخ 400 با فرمت استاندارد برمی‌گردد
     * 3- الزام به Attribute Routing : هر کنترلری که [ApiController] دارد باید [Route("...")] داشته باشد و اکشن‌ها فقط از طریق attribute route ها قابل دسترسی‌اند. اگر با convention routing (مثل MapDefaultControllerRoute) سعی کنید به آن دسترسی پیدا کنید، خطای runtime می‌گیرید.
     *      به همین دلیل در Program.cs پروژه شما app.MapControllers() است، نه MapControllerRoute.
     * 4- استنتاج multipart/form-data برای فرم‌ها : اگر جایی [FromForm] بنویسید، فریم‌ورک خودش به اکشن [Consumes("multipart/form-data")] هم اضافه می‌کند. این باعث می‌شود:
     *      Swagger مستندات درست تولید کند (دکمه‌ی انتخاب فایل/فرم نمایش می‌دهد)
     *      اگر Content-Type اشتباهی بفرستید، پاسخ شفاف 415 بگیرید
     * 5- تبدیل خطاهای HTTP به ProblemDetails استاندارد RFC 7807 : وقتی NotFound() یا BadRequest() برمی‌گردانید، بدنه‌ی پاسخ به‌صورت خودکار به ساختار استاندارد ProblemDetails تبدیل می‌شود نه صرفاً یک بدنه‌ی خالی
     * 6- خودکار 404 برای مقدار null : اگر اکشن از نوع ActionResult<T> باشد و مقدار null برگرداند، به‌جای پاسخ 200 خالی، خودکار 404 صادر می‌شود
     * البته این رفتار ها را میتوان در برنامه کنترل کرد و در صورت نیاز فعال یا غیر فعال نمود به program.cs مراجعه کنید
     */
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Cities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            // مرتب سازی
            var cities = await _context.Cities
             .OrderBy(temp => temp.CityName).ToListAsync();
            return cities;
        }


        // GET: api/Cities/5
        [HttpGet("{id}")]
        public async Task<ActionResult<City>> GetCity(Guid cityID)
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            var city = await _context.Cities.FindAsync(cityID);

            if (city == null)
            {
                return NotFound();
            }

            return city;
        }


        // PUT: api/Cities/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCity(Guid id, City city)
        {
            if (id != city.CityID)
            {
                return BadRequest();
            }

            _context.Entry(city).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CityExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // POST: api/Cities
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<City>> PostCity(City city)
        {
            if (_context.Cities == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Cities'  is null.");
            }
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCity", new { id = city.CityID }, city);
        }


        // DELETE: api/Cities/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCity(Guid id)
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }

            _context.Cities.Remove(city);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CityExists(Guid id)
        {
            return (_context.Cities?.Any(e => e.CityID == id)).GetValueOrDefault();
        }
    }
}

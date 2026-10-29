using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CitiesManager.WebAPI.DatabaseContext;
using CitiesManager.WebAPI.Models;
// بعد از ساختن EF , model , ... برای ساختن API Controller
// روی پوشه کنترلر راست کلید -> افزودن کنترلر -> از لیست ای پی آی را انتخاب کنید -> حال گزینه زیر را انتخاب کنید
// API Controller with Action AND EF
namespace CitiesManager.WebAPI.Controllers
{
    // بر اساس تعریف ساختار اکشن ها از این پس فراخوانی متد های داخل این کنترلر از این خط گرفته خواهند شد:
    // /api/Cities
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CitiesController(ApplicationDbContext context)
        {
            _context = context;
        }
        // به سر آدرس بالا هیچ چیزی افزوده نمیشود
        // GET: api/Cities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            if (_context.Cities == null)
            {
                return NotFound();
            }
            return await _context.Cities.ToListAsync();
        }

        //  به سر آدرس بالا یک آی دی اختیاری افزوده میشود البته در متد گت 
        // GET: api/Cities/bf160cfd-e693-4c6a-9417-037b4501ec9b
        [HttpGet("{id}")]
        public async Task<ActionResult<City>> GetCity(Guid id)
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

            return city;
        }

        // در متد پست به انتهای آدرس بالا یک آی دی اختیاری افزوده میشود
        // PUT: api/Cities/Cities/bf160cfd-e693-4c6a-9417-037b4501ec9b
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

        // دقیقاً مانند گت فقط این متد پست است
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        // POST: api/Cities
       // مثال برای ورودی اکشن فوق در پوست من
        /*
          {
             "cityName": "New York1"
          }
         */
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


        // DELETE: api/Cities/f82c6c0f-e252-4dff-17fc-08df3399caec
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace JwtDemo.Pages;

// صفحه‌ی خانه فقط به صفحه‌ی اشخاص هدایت می‌شود (تنها صفحه‌ی اصلی برنامه)
public class IndexModel : PageModel
{
    public IActionResult OnGet() => RedirectToPage("/Persons");
}

using Microsoft.AspNetCore.Mvc;

namespace AsvsIntegratedDemo.Controllers {
    public class FileUploadController : Controller {
        public IActionResult Index() => View();

        // بند OWASP ASVS: V12.3.3 - آپلود امن فایل
        [HttpPost]
        public IActionResult FileUploadDemo(IFormFile file) {
            string[] allowedExtensions = { ".jpg", ".png", ".pdf" };
            string ext = Path.GetExtension(file.FileName).ToLower();
            
            if (Array.IndexOf(allowedExtensions, ext) == -1) {
                ViewBag.Message = $"خطا (V12.3.3): پسوند فایل '{ext}' مجاز نیست.";
                ViewBag.IsSuccess = false;
            } else {
                ViewBag.Message = $"فایل '{file.FileName}' با موفقیت اعتبارسنجی شد (V12.3.3).";
                ViewBag.IsSuccess = true;
            }
            return View("Result");
        }
    }
}

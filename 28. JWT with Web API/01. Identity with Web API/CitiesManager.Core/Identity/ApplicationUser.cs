// این دو فایل را از نوع کلاس بسازید
using Microsoft.AspNetCore.Identity;

namespace CitiesManager.Core.Identity
{
 public class ApplicationUser : IdentityUser<Guid>
 {
  public string? PersonName { get; set; }
 }
}


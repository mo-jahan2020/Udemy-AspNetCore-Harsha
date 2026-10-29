//این دو پکیج ابتدا باید نصب شود
// Microsoft.AspNetCore.Identity  for IdentityUser and IdentityRole
// Microsoft.AspNetCore.Identity.EntityFrameworkCore برای ارتباط Identiry با بانک اطلاعاتی و ذخیر و بازیابی اطلاعات در بانک و  جدول Identity
using Microsoft.AspNetCore.Identity;
using System;

namespace ContactsManager.Core.Domain.IdentityEntities
{
 public class ApplicationUser : IdentityUser<Guid>
 {
  public string? PersonName { get; set; }
 }
}

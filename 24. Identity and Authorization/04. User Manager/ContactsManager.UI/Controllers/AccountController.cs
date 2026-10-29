using ContactsManager.Core.Domain.IdentityEntities;
using ContactsManager.Core.DTO;
using CRUDExample.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ContactsManager.UI.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            // حال این قسمت از کنترلر را تکمیل کرده 
            // مهم : در package manager Consol : project= ContactsManager.Infrastructure :
            // Add-Migration IdentityTables
            // Update-Database
            //Check for validation errors
            if (ModelState.IsValid == false)
            {
                ViewBag.Errors = ModelState.Values.SelectMany(temp => temp.Errors).Select(temp => temp.ErrorMessage);
                return View(registerDTO);
            }

            ApplicationUser user = new ApplicationUser() { Email = registerDTO.Email, PhoneNumber = registerDTO.Phone, UserName = registerDTO.Email, PersonName = registerDTO.PersonName };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
            //IdentityResult result1 = await _userManager.DeleteAsync(user);
            //IdentityResult result1 = await _userManager.UpdateAsync(user);
            //Boolean result1 = await _userManager.IsInRoleAsync(user,"Admin");
            //ApplicationUser? result1 = await _userManager.FindByEmailAsync("Mo@gamil.com");
            //ApplicationUser? result1 = await _userManager.FindByIdAsync("GUID.Tostring()");
            //ApplicationUser? result1 = await _userManager.FindByNameAsync("Mo.jahan");
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(PersonsController.Index), "Persons");
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("Register", error.Description);
                }

                return View(registerDTO);
            }

        }
    }
}

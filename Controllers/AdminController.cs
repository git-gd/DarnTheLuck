using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DarnTheLuck.Controllers
{
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(RoleManager<IdentityRole> roleManager)
        {
            this._roleManager = roleManager;
        }

        public IActionResult Index()
        {
            // this should be restricted to admins.. but if we have no admin?
            // TODO: research best way to initialize admin account (database command? hardcode?)

            IEnumerable<IdentityRole> roles = _roleManager.Roles;

            return View(roles);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                // We just need to specify a unique role name to create a new role
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                // Saves the role in the underlying AspNetRoles table
                IdentityResult result = await _roleManager.CreateAsync(identityRole);

                if (result.Succeeded)
                {
                    return RedirectToAction("index", "admin");
                }

                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }
    }
}

/*
 * https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-5.0
 * https://csharp-video-tutorials.blogspot.com/2019/07/creating-roles-in-aspnet-core.html
 * 
 * var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
 * await roleManager.CreateAsync(new IdentityRole(role));
 * await userManager.FindByIdAsync(uid);
 * await userManager.AddToRoleAsync(user, role);
 * 
 * TODO: Tech and Admin user roles
 * Advanced features like tech groups and authorization links (allow others to view Ticket) can likely be simple database functions.
 */
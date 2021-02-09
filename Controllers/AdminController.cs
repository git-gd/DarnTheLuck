using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarnTheLuck.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            // Trying injection on the Razor page
            return View();
        }

        /*
         * CreateRole will be repurposed to create the Admin and Technician Roles on a first run
         */
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> CreateRole()
        {
            /*
             * Create the Admin role
             */

            IdentityRole admin = new IdentityRole
            {
                Name = "Admin"
            };

            bool roleExists = await _roleManager.RoleExistsAsync(admin.Name);

            if (!roleExists)
            {
                IdentityResult result = await _roleManager.CreateAsync(admin);

                if (result.Succeeded)
                {
                    IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);
                    if (!(await _userManager.IsInRoleAsync(user, admin.Name)))
                    {
                        await _userManager.AddToRoleAsync(user, admin.Name);
                    }
                }
            }

            /*
             * Create the Technician role
             */

            IdentityRole tech = new IdentityRole
            {
                Name = "Technician"
            };

            roleExists = await _roleManager.RoleExistsAsync(tech.Name);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(tech);
            }

            return RedirectToAction("index", "admin");
        }

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(roleId); // Get the role from the ID

            List<UserRoleViewModel> model = new List<UserRoleViewModel>();

            if (role == null) { return View(model); } // send our Null model to the View to display a message

            ViewBag.roleName = role.Name;

            foreach (var user in _userManager.Users.ToList())
            {
                UserRoleViewModel userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                userRoleViewModel.IsSelected = await _userManager.IsInRoleAsync(user, role.Name);

                model.Add(userRoleViewModel);
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> models, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            foreach (UserRoleViewModel model in models) // Loop through the list
            {
                var user = await _userManager.FindByIdAsync(model.UserId); // Pull the User info

                bool isInRole = await _userManager.IsInRoleAsync(user, role.Name);

                // if checked and the user is NOT in the role, add them
                if (model.IsSelected && !isInRole)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
                // if NOT checked and the user was in the role, remove them
                else if (!model.IsSelected && isInRole)
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
            }
            return RedirectToAction("Index");
        }
    }
}

/*
 * https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-5.0
 * https://csharp-video-tutorials.blogspot.com/2019/07/creating-roles-in-aspnet-core.html
 */
using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarnTheLuck.Controllers
{
    // this should be restricted to admins.. but if we have no admin?
    // TODO: research best way to initialize admin account (database command? hardcode?)
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
    


        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                RedirectToAction("Index"); // ToDo: return error page
            }

            ViewBag.roleId = roleId;

            var model = new List<UserRoleViewModel>();

            /*****
             * LEARNING OPPORTUNITY:
             * 
             * _userManager.Users seems to keep the database connection OPEN
             * 
             * using .ToList() seems to save the Users to a List and close the connection
             * 
             *****/

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
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                return View("Index"); // ToDo: Error page
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);

                IdentityResult result;

                if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded && i > (model.Count))
                    return RedirectToAction("Index"); // EditRole - we did not do this
            }

            return RedirectToAction("Index"); // EditRole - we did not do this .. the example redirects an edit page
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
 * 
 * TODO: EVERY View will need to test for NULL - If a user types an URL that produces a NULL we need to catch and deal with this
 */
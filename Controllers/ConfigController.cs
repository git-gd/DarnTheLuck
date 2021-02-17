using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DarnTheLuck.Controllers
{
    public class ConfigController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public ConfigController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /*****************************************************************
         * Create the Admin and Technician Roles And Set The User To Admin
         *****************************************************************/
        public async Task<IActionResult> Index()
        {
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

                    /*
                     *  Updating the Role isn't enough. The Role is stored in the cookie. (~Ugh!)
                     *  We need to refresh the user's Role with SignInManager:
                     */
                    await _signInManager.RefreshSignInAsync(user);
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
    }
}

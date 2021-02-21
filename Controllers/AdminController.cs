using DarnTheLuck.Data;
using DarnTheLuck.Models;
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
        private readonly ApplicationDbContext _context;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            // Trying injection on the Razor page
            return View();
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

        [HttpGet]
        public ActionResult ListAllUsers()
        {
            List<ListAllUsersViewModel> users = (
                    from User in _userManager.Users
                    select new ListAllUsersViewModel()
                    {
                        Email = User.Email,
                        Delete = false
                    }).ToList();

            return View(users);
        }

        // Delete Users
        // TODO: confirmation
        [HttpPost]
        public async Task<ActionResult> ListAllUsers(List<ListAllUsersViewModel> users)
        {
            if (!ModelState.IsValid)
            {
                return View(users);
            }

            foreach(ListAllUsersViewModel user in users)
            {
                if (user.Delete)
                {
                    IdentityUser target = await _userManager.FindByEmailAsync(user.Email);
                    bool isAdmin = await _userManager.IsInRoleAsync(target, "Admin"); // don't delete Admin
                    if(!isAdmin)
                    {
                        // remove all usergroups that contain the user to be deleted
                        List<UserGroup> userGroups = _context.UserGroups
                            .Where(u => u.UserId == target.Id ||
                                        u.GrantId == target.Id)
                            .ToList();
                        foreach(UserGroup group in userGroups)
                        {
                            _context.UserGroups.Remove(group);
                        }

                        // remove all of their tickets before removing the user
                        List<Ticket> tickets = _context.Tickets
                            .Where(t => t.UserId == target.Id)
                            .ToList();

                        foreach(Ticket ticket in tickets)
                        {
                            _context.Remove(ticket);
                        }
                        await _context.SaveChangesAsync();

                        await _userManager.DeleteAsync(target);

                        //TODO: Force User Logout
                    }
                }
            }

            return Redirect("ListAllUsers");
        }
    }
}

/*
 * https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-5.0
 * https://csharp-video-tutorials.blogspot.com/2019/07/creating-roles-in-aspnet-core.html
 */
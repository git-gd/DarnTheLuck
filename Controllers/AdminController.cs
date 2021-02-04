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
    // this should be restricted to admins.. but if we have no admin?
    // TODO: research best way to initialize admin account (database command? hardcode?)
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext      _context;

        public AdminController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context     = context;
        }

        public IActionResult Index()
        {
            // Trying injection on the Razor page
            return View();
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
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            for (int i = 0; i < model.Count; i++) // Loop through the list
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId); // Pull the User info

                // if checked and the user is NOT in the role, add them
                if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                }
                // if NOT checked and the user was in the role, remove them
                else if (!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult CreateStatus()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateStatus(StatusViewModel model)
        {
            if (ModelState.IsValid)
            {
                TicketStatus ticketStatus = _context.TicketStatuses
                    .Where(ts => ts.Name == model.Name)
                    .FirstOrDefault();

                if (ticketStatus == null)
                {

                    ticketStatus = new TicketStatus()
                    {
                        Name = model.Name
                    };

                    _context.TicketStatuses.Add(ticketStatus);
                    _context.SaveChanges();
                }

                return RedirectToAction("index", "admin"); // TODO: return to ticket status list
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult EditStatus()
        {
            EditStatusViewModel editStatusViewModel = new EditStatusViewModel();

            editStatusViewModel.TicketStatuses = _context.TicketStatuses.ToList();

            return View(editStatusViewModel);
        }

        [HttpPost]
        public IActionResult EditStatus(EditStatusViewModel model)
        {
            /*
             * TODO: Currently a partially working mess... consider moving to its own controller?
             * 
             */

            if (ModelState.IsValid)
            {
                // what is the function? Update or Delete?
                // on update, verify the NEW name does NOT exist and save changes
                // on delete verify it exists and then delete

                //TicketStatus ticketStatus = _context.TicketStatuses
                //    .Where(ts => ts.Name == model.Name)
                //    .FirstOrDefault();

                //if (ticketStatus == null)
                //{

                //    ticketStatus = new TicketStatus()
                //    {
                //        Name = model.Name
                //    };

                //    _context.TicketStatuses.Add(ticketStatus);
                //    _context.SaveChanges();
                //}

                return RedirectToAction("EditStatus", "admin"); // TODO: return to ticket status list
            }

            return View(model);
        }
    }
}

/*
 * https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-5.0
 * https://csharp-video-tutorials.blogspot.com/2019/07/creating-roles-in-aspnet-core.html
 */
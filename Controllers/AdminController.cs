﻿using DarnTheLuck.Data;
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

            IdentityRole tech = new IdentityRole
            {
                Name = "Technician"
            };

            roleExists = await _roleManager.RoleExistsAsync(tech.Name);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(admin);
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
    }
}

/*
 * https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-5.0
 * https://csharp-video-tutorials.blogspot.com/2019/07/creating-roles-in-aspnet-core.html
 */
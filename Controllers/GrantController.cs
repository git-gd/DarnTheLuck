using DarnTheLuck.Data;
using DarnTheLuck.Models;
using DarnTheLuck.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarnTheLuck.Controllers
{
    public class GrantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public GrantController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Index will list authorized users, unauthorized users and unused codes
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            List<UserGroupViewModel> userList = (
            from Grant in _context.UserGroups
            where Grant.UserId == user.Id
            select new UserGroupViewModel()
            {
                GrantEmail = Grant.GrantEmail,
                Authorized = Grant.Authorized,
            }).ToList();

            return View(userList);
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<UserGroupViewModel> userList)
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            foreach (UserGroupViewModel grant in userList)
            {
                UserGroup userGroup = _context.UserGroups.FirstOrDefault(u => u.UserId == user.Id && u.GrantEmail == grant.GrantEmail);

                if (userGroup != null)
                {
                    userGroup.Authorized = grant.Authorized;
                }
            }
            _context.SaveChanges();

            return View(userList);
        }

        // Create shareable code/link
        public async Task<IActionResult> CreateCode()
        {
            //TODO: Generate Code
            /*
             * TEMPORARY - Getting this code working, will decide on how to do real codes
             */

            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            _context.UserGroups.Add(new UserGroup(){
                UserId = user.Id,
                UserEmail = user.Email,
                GrantId = DateTime.Now.ToString("g")
            });
            _context.SaveChanges();

            return Redirect("Index");
        }

        [HttpGet]
        public IActionResult UseCode()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UseCode(UseCodeViewModel code)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

                UserGroup userGroup = _context.UserGroups.FirstOrDefault(u => u.GrantId == code.Value);

                if(userGroup == null)
                {
                    ModelState.AddModelError("Value", "CODE NOT FOUND");
                    return View(code);
                }

                // because I chose to use a composite key... add a new record, remove the old
                // TODO: research to see if there is any way to update the value of a composite key

                // TODO: if the UserID of a valid grant is entered this will try to create a new entry... We Don't Want This
                _context.UserGroups.Add(new UserGroup()
                {
                    UserId = userGroup.UserId,
                    UserEmail = userGroup.UserEmail,
                    GrantId = user.Id,
                    GrantEmail = user.Email,
                    Authorized = userGroup.Authorized
                });
                _context.UserGroups.Remove(userGroup);
                _context.SaveChanges();
            }
            else
            {
                return View(code);
            }
            return Redirect("Index");
        }
    }
}

/*
 * A user wants to allow others to view their ticket
 * 
 * A user clicks a link (Create Share Link) to create a shareable code/link
 * 
 * We create a new entry in our UserGroups Table of UserGroup: UserId/code/false
 * 
 * The link is passed to another user (text/email???) - we will not code this part
 * 
 * A user consumes/uses a shared code/link and their UserId replaces the code
 * 
 * The creator of the share code/link can view a list of users/codes
 * 
 * The creator of the share code/link can authroize/deauthorize confirmed users
 * 
 * The creator of the share code/link can delete users/codes
 * 
 * Users will want to be able to selectively view other users' tickets
 * - add the option to the ticket list?
 * - view from the grant controller?
 * 
 */
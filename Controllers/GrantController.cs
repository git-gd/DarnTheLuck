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
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            /*
             * This is not meant to be a strong code.
             * Each code can be used once and is then replaced.
             * Using a code adds the current User's Id to the access table
             * The access bool is defaulted to false requiring the user to enable it
             */
            string accessCode = User.Identity.Name.Substring(0,3) + DateTime.Now.Ticks.ToString("X");

            _context.UserGroups.Add(new UserGroup(){
                UserId = user.Id,
                UserEmail = user.Email,
                GrantId = accessCode,
                GrantEmail = accessCode
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

                string trimmedCode = code.Value.Trim();

                /*
                 * IMPORTANT:
                 * To prevent being able to enter the Id of a verified user as a code and hijack their permission
                 * We check against both GrantId AND GrantEmail.
                 * Both Id and Email fields are set to the permission code before a code is consumed
                 * The only time Id and Email will be the same is when a code has not been consumed
                 */
                UserGroup userGroup = _context.UserGroups.FirstOrDefault(u => u.GrantId == trimmedCode && u.GrantEmail == trimmedCode);

                // Code wasn't found
                if (userGroup == null)
                {
                    ModelState.AddModelError("Value", "CODE NOT FOUND");
                    return View(code);
                }
                else
                {
                    // Did the user try to consume their own code?
                    if(userGroup.UserId == user.Id)
                    {
                        ModelState.AddModelError("Value", "YOU CAN'T CONSUME YOUR OWN CODE.");
                        return View(code);
                    }

                    // Does the user already have a table entry?
                    UserGroup alreadyExists = await _context.UserGroups.FindAsync(userGroup.UserId, user.Id);

                    if (alreadyExists != null)
                    {
                        ModelState.AddModelError("Value", "YOU HAVE ALREADY CONSUMED A CODE FROM THIS PROVIDER.");
                        return View(code);
                    }
                }

                // because I chose to use a composite key... add a new record, remove the old
                // TODO: research to see if there is any way to update the value of a composite key

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
            return View("CodeSuccess");
        }

        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            List<DeleteGroupViewModel> userList = (
            from Grant in _context.UserGroups
            where Grant.UserId == user.Id
            select new DeleteGroupViewModel()
            {
                GrantEmail = Grant.GrantEmail,
                Delete = false
            }).ToList();

            return View(userList);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(List<DeleteGroupViewModel> userList)
        {
            IdentityUser user = await _userManager.GetUserAsync(HttpContext.User);

            foreach (DeleteGroupViewModel grant in userList)
            {
                if (grant.Delete) {
                    UserGroup userGroup = _context.UserGroups.FirstOrDefault(u => u.UserId == user.Id && u.GrantEmail == grant.GrantEmail);

                    if (userGroup != null)
                    {
                        _context.UserGroups.Remove(userGroup);
                    }
                }
            }
            _context.SaveChanges();

            return Redirect("Index");
        }
    }
}

/*
 * A user wants to allow others to view their ticket ✔
 * 
 * A user clicks a link (Create Share Link) to create a shareable code/link ✔
 * 
 * We create a new entry in our UserGroups Table of UserGroup: UserId/code/false ✔
 * 
 * The link is passed to another user (text/email???) - we will not code this part ✔
 * 
 * A user consumes/uses a shared code/link and their UserId replaces the code ✔
 * 
 * The creator of the share code/link can view a list of users/codes ✔
 * 
 * The creator of the share code/link can authroize/deauthorize confirmed users ✔
 * 
 * The creator of the share code/link can delete users/codes ✔
 * 
 * Users will want to be able to selectively view other users' tickets
 * - add the option to the ticket list? 💤 - additional filtering can be added
 * - view from the grant controller?    ❌ - this needs to be in the ticket controller
 * 
 */
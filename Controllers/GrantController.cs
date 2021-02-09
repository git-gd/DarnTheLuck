using DarnTheLuck.Data;
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

            /*
             * Oof! Joins! What are they good for?
             * I'm using a Join to pull the email address from the Users table
             * If there's no match then it should be a code, so we display it
             */

            List<UserGroupViewModel> userList = (
            from Grant in _context.UserGroups
            join U1 in _context.Users on Grant.GrantId equals U1.Id
            where Grant.UserId == user.Id
            select new UserGroupViewModel()
            {
                GrantId = string.IsNullOrEmpty(U1.Email)?Grant.GrantId:U1.Email,
                Authorized = Grant.Authorized
            }).ToList();

            return View(userList);
        }

        [HttpPost]
        public IActionResult Index(List<UserGroupViewModel> userList)
        {
            return View(userList);
        }

        // Create shareable code/link
        public IActionResult CreateCode()
        {
            return View();
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
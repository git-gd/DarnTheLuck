using DarnTheLuck.Data;
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
        public IActionResult Index()
        {
            return View();
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
 */
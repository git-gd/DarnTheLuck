using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DarnTheLuck.Controllers
{
    public class GrantController : Controller
    {
        public IActionResult Index()
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
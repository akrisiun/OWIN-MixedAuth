using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SPA.Controllers
{
    
    public class HomeController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Fail(string message)
        {
            Response.Write(message);
            return View();
        }


        //[Authorize]
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }
    }
}

using System;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SPA.Models;

namespace SPA.Controllers
{
    [Authorize]
    public class MeController : ApiController
    {
        private ApplicationUserManager _userManager;

        public MeController()
        {
        }

        public MeController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET api/Me
        public GetViewModel Get()
        {
            ApplicationUser user = UserManager?.FindById(User.Identity.GetUserId());
            var principal = User?.Identity;
            var name = principal?.Name ?? "";

            return new GetViewModel() { Hometown = user?.Hometown };
        }
    }
}
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using static SPA.Controllers.AccountController;

namespace SPA
{
    public static class AccountActions
    {
        public static ChallengeResult ChallengeResult(string provider, string returnUrl, UrlHelper Url)
        {
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new
            {
                ReturnUrl = returnUrl
            }));
        }

        // Request a redirect to the external login provider to link a login for the current user
        public static ActionResult LinkLogin(string provider, UrlHelper Url, IPrincipal User)
        {
            // IdentityExtensions User.Identity.GetUserId()
            var id = User.Identity.GetUserId();
            return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), id);
        }

        public static void SignOut(IAuthenticationManager AuthenticationManager, IPrincipal User, HttpContextBase http)
        {
            AuthenticationManager.SignOut();

            var claims = new ClaimsPrincipal().Claims.ToArray();
            var identity = new ClaimsIdentity(claims, "Bearer");
            //  AuthenticationManager.SignIn(identity);
            if (User.Identity.IsAuthenticated)
            {
                System.Web.HttpContext.Current.User =
                        new GenericPrincipal(new GenericIdentity(string.Empty), null);
                http.User = System.Web.HttpContext.Current.User;
            }
        }
    }

    public class ChallengeResult : HttpUnauthorizedResult
    {
        public ChallengeResult(string provider, string redirectUri)
            : this(provider, redirectUri, null)
        {
        }

        public ChallengeResult(string provider, string redirectUri, string userId)
        {
            LoginProvider = provider;
            RedirectUri = redirectUri;
            UserId = userId;
        }

        public string LoginProvider { get; set; }
        public string RedirectUri { get; set; }
        public string UserId { get; set; }

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        public override void ExecuteResult(ControllerContext context)
        {
            var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
            if (UserId != null)
            {
                properties.Dictionary[XsrfKey] = UserId;
            }
            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
        }
    }

}

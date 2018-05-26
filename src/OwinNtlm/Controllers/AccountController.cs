using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SPA.Models;

namespace SPA.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region ctor 
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager) // , ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            //SignInManager = signInManager;
            //if (signInManager == null)
            //{
            //    SignInManager = HttpContext.GetOwinContext().GetUserManager<ApplicationSignInManager>();
            //}
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        #endregion

        // The Authorize Action is the end point which gets called when you access any
        //[HttpGet]
        [AllowAnonymous]
        public ActionResult Authorize()
        {
            var claims = new ClaimsPrincipal(User).Claims.ToArray();
            var identity = new ClaimsIdentity(claims, "Bearer");
            AuthenticationManager.SignIn(identity);

            return new EmptyResult();
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            AccountActions.SignOut(AuthenticationManager, User, this.HttpContext);

            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl ?? "/";
            return View();
        }

        //private ApplicationSignInManager _signInManager;
        //public ApplicationSignInManager SignInManager
        //{
        //    get
        //    {
        //        return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        //    }
        //    private set { _signInManager = value; }
        //}

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult // async Task<ActionResult> 
               Login(object model, string returnUrl)
        {
            // LoginViewModel
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            SignInStatus result = SignInStatus.Success; 
            // await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        // [AllowAnonymous] public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)

        // POST: /Account/VerifyCode
        // [HttpPost]        [AllowAnonymous]        [ValidateAntiForgeryToken]
        // public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        
         
        // POST: /Account/Register
        // [HttpPost]         [AllowAnonymous]         [ValidateAntiForgeryToken]
        // public async Task<ActionResult> Register(RegisterViewModel model)
        
           
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return AccountActions.ChallengeResult(provider, returnUrl, Url);
        }

        //
        // GET: /Account/SendCode
        // [AllowAnonymous]        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        
         

        [AllowAnonymous]
        public ActionResult Fail(string message)
        {
            var stack = new System.Text.StringBuilder(Environment.StackTrace)
                    .Replace(Environment.NewLine, "<br>" + Environment.NewLine).ToString();
            if (message == null)
            {
                var raw = System.Net.WebUtility.HtmlDecode(Request.RawUrl);
                message = raw;
            }
            if (message.Contains("%20"))
                message = new System.Text.StringBuilder(message)
                    .Replace("%20", " ")
                    .Replace("%5C", @"\")
                    .ToString();

            //  Response.Write(message);
            var model = new HandleErrorInfo(new Exception(message) { Source = stack },
                Request.RequestContext.RouteData.Values["controller"] as string,
                Request.RequestContext.RouteData.Values["action"] as string);

            return View("Error", masterName: null, model: model);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login

            Task<SignInStatus> resultTask = null;
            SignInStatus result = default(SignInStatus);
            LastError = null;

            try
            {
                //var manager = SignInManager;
                //if (manager != null)
                //{
                //    resultTask = manager.ExternalSignInAsync(loginInfo, isPersistent: false);
                //    result = await resultTask;
                //}
                if (User.Identity.IsAuthenticated)
                {
                    result = SignInStatus.Success;
                }

                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
            }
            catch (Exception ex)
            {
                LastError = ex.InnerException ?? ex;
                Response.Write($"SignInManager.ExternalSignInAsync fail: {LastError.Message}<br>{LastError.StackTrace}");
            }

            if (LastError != null)
                return RedirectToAction($"/Fail/?message={LastError.Message}");

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        public static Exception LastError { get; set; }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        // await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        #region Helpers
        
        
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        
        #endregion
    }
}

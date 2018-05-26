using System;
using SPA.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace SPA
{ 

    // Configure the application user manager which is used in this application.
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        //public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
        //    IOwinContext context)
        //{
        //    // Microsoft.AspNet.Identity.EntityFramework.UserStore
        //    var manager = new ApplicationUserManager(
        //         new UserStore<ApplicationUser>()); // new UserStore<ApplicationUser>(null));
        //             // context.Get<ApplicationDbContext>()));

        //    // Configure validation logic for usernames
        //    //manager.UserValidator = new UserValidator<ApplicationUser>(manager)

        //    // Configure validation logic for passwords
        //    manager.PasswordValidator = new PasswordValidator
        //    {
        //        RequiredLength = 3,
        //        RequireNonLetterOrDigit = false,
        //        RequireDigit = true,
        //        RequireLowercase = true,
        //        RequireUppercase = false,
        //    };

        //    // Configure user lockout defaults
        //    manager.UserLockoutEnabledByDefault = true;
        //    manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
        //    manager.MaxFailedAccessAttemptsBeforeLockout = 5;

        //    // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
        //    // You can write your own provider and plug it in here.
        //    // manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
        //    //manager.EmailService = new EmailService();

        //    var dataProtectionProvider = options.DataProtectionProvider;
        //    if (dataProtectionProvider != null)
        //    {
        //        manager.UserTokenProvider =
        //            new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
        //    }
        //    return manager;
        //}

    }

    // Configure the application sign-in manager which is used in this application.  
    public class ApplicationSignInManager : SignInManager<ApplicationUser, string>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) :
            base(userManager, authenticationManager) { }

        //public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        //{
        //    return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        //}

        //public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        //{
        //    // !!!
        //    var manager = context.GetUserManager<ApplicationUserManager>();
        //    if (manager == null)
        //        manager = ApplicationUserManager.Create(new IdentityFactoryOptions<ApplicationUserManager>(), context);

        //    return new ApplicationSignInManager(manager, context.Authentication);
        //}
    }
}

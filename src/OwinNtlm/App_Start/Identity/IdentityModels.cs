//using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.ComponentModel.DataAnnotations;

namespace SPA.Models
{

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Hometown")]
        public string Hometown { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    /*
     * Microsoft.AspNet.Identity.EntityFramework     IdentityDbContext Class
        Assembly:  Microsoft.AspNet.Identity.EntityFramework (in Microsoft.AspNet.Identity.EntityFramework.dll)
    */

    // AuthContext.cs:
    //public class AuthContext : IdentityDbContext<IdentityUser>
    //{
    //    public AuthContext()
    //        : base("AuthContext")
    //    {

    //    }
    //}


    // You can add profile data for the user by adding more properties to your ApplicationUser class,
    // please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IUser // : IdentityUser
    {
        public string Hometown { get; set; }

        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        //    // Add custom user claims here
        //    return userIdentity;
        //}
    }

    //public class ApplicationDbContext  // : IdentityDbContext<ApplicationUser>
    //{
    //    public ApplicationDbContext()
    //        // : base("DefaultConnection", throwIfV1Schema: false)
    //    {
    //    }

    //    public static ApplicationDbContext Create()
    //    {
    //        return new ApplicationDbContext();
    //    }
    //}
}

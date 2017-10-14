//using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace SPA.Models
{
    /*
     * Microsoft.AspNet.Identity.EntityFramework
    IdentityDbContext Class

Assembly:  Microsoft.AspNet.Identity.EntityFramework (in Microsoft.AspNet.Identity.EntityFramework.dll)

        System.Data.Entity.DbContext
Microsoft.AspNet.Identity.EntityFramework.IdentityDbContext<TUser, TRole, TKey, TUserLogin, TUserRole, TUserClaim>
      Microsoft.AspNet.Identity.EntityFramework.IdentityDbContext
        */

    // AuthContext.cs:
    public class AuthContext : IdentityDbContext<IdentityUser>
    {
        public AuthContext()
            : base("AuthContext")
        {

        }
    }


    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string Hometown { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext  // : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            // : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}
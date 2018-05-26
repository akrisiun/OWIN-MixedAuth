/// <copyright file="IMixedAuthProvider.cs" auther="Mohammad Younes">
/// 
/// Based on IGoogleAuthenticationProvider
/// https://katanaproject.codeplex.com/SourceControl/latest#src/Microsoft.Owin.Security.Google/Provider/IGoogleAuthenticationProvider.cs
/// 
/// Copyright 2014 Mohammad Younes. 
/// https://github.com/MohammadYounes/Owin-MixedAuth
/// 
/// Released under the MIT license
/// http://opensource.org/licenses/MIT
///
/// </copyright>

using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace MohammadYounes.Owin.Security.MixedAuth
{
    // Specifies callback methods which the <see cref="MixedAuthMiddleware"></see> invokes to enable developer control over the authentication process. />
    public interface IMixedAuthProvider
    {

        // Invoked to get the LogonUserIdentity
        WindowsIdentity GetLogonUserIdentity(IOwinContext context);

        // Invoked to import custom claims
        List<Claim> ImportClaims(WindowsIdentity identity);

        // Invoked whenever MixedAuth succesfully authenticates a user
        // <returns>A <see cref="Task"/> representing the completed operation.</returns>
        void ApplyRedirect(MixedAuthApplyRedirectContext context);

        // Called when a Challenge causes a redirect to authorize endpoint in the MixedAuth middleware
        Task Authenticated(MixedAuthAuthenticatedContext context);
    }

    /// Context passed when a Challenge causes a redirect to authorize endpoint in the MixedAuth middleware
    /// </summary>
    public class MixedAuthApplyRedirectContext : BaseContext<MixedAuthOptions>
    {
        /// <summary>
        /// Creates a new context object.
        /// </summary>
        /// <param name="context">The OWIN request context</param>
        /// <param name="options">The MixedAuth middleware options</param>
        /// <param name="properties">The authentication properties of the challenge</param>
        /// <param name="redirectUri">The initial redirect URI</param>
        public MixedAuthApplyRedirectContext(IOwinContext context, MixedAuthOptions options,
              AuthenticationProperties properties, string redirectUri)
            : base(context, options)
        {
            RedirectUri = redirectUri;
            Properties = properties;
        }

        /// <summary>
        /// Gets the URI used for the redirect operation.
        /// </summary>
        public string RedirectUri { get; private set; }

        /// <summary>
        /// Gets the authentication properties of the challenge
        /// </summary>
        public AuthenticationProperties Properties { get; private set; }
    }

    /// Default <see cref="IMixedAuthProvider"/> implementation.
    /// </summary>
    public class MixedAuthProvider : IMixedAuthProvider
    {
        /// <summary>
        /// Initializes a <see cref="MixedAuthProvider"/>
        /// </summary>
        public MixedAuthProvider()
        {
            OnAuthenticated = context => Task.FromResult<object>(null);

            OnApplyRedirect = context =>
                context.Response.Redirect(context.RedirectUri);

            OnGetLogonUserIdentity = context =>
            {
                var httpRequest = ((System.Web.HttpContextBase)context.Environment["System.Web.HttpContextBase"]).Request;
                return httpRequest.LogonUserIdentity;
            };

            OnImportClaims = identity =>
            {
                return new List<Claim>();
            };
        }

        /// <summary>
        /// Gets or sets the function that is invoked when the Authenticated method is invoked.
        /// </summary>
        public Func<MixedAuthAuthenticatedContext, Task> OnAuthenticated { get; set; }

        /// <summary>
        /// Gets or sets the delegate that is invoked when the ApplyRedirect method is invoked.
        /// </summary>
        public Action<MixedAuthApplyRedirectContext> OnApplyRedirect { get; set; }


        /// <summary>
        /// Gets or sets the delegate that is invoked to get the LogonUserIdentity.
        /// </summary>
        public Func<IOwinContext, WindowsIdentity> OnGetLogonUserIdentity { get; set; }

        /// <summary>
        /// Gets or sets the delegate that is invoked to import the custom claims.
        /// </summary>
        public Func<WindowsIdentity, List<Claim>> OnImportClaims { get; set; }


        /// <summary>
        /// Invoked whenever MixedAuth successfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task Authenticated(MixedAuthAuthenticatedContext context)
        {
            return OnAuthenticated(context);
        }

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the MixedAuth middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        public virtual void ApplyRedirect(MixedAuthApplyRedirectContext context)
        {
            OnApplyRedirect(context);
        }


        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the MixedAuth middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="AuthenticationProperties"/> of the challenge </param>
        public virtual WindowsIdentity GetLogonUserIdentity(IOwinContext context)
        {
            return OnGetLogonUserIdentity(context);
        }

        /// <summary>
        /// Called when a user is authenticated to allow adding your own custom claims. 
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public List<Claim> ImportClaims(WindowsIdentity identity)
        {
            return OnImportClaims(identity);
        }
    }
}

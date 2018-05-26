/// <copyright file="MixedAuthAuthenticatedContext.cs" auther="Mohammad Younes">
/// Based on GoogleAuthenticatedContext
/// https://katanaproject.codeplex.com/SourceControl/latest#src/Microsoft.Owin.Security.Google/Provider/GoogleAuthenticatedContext.cs
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
using System.Security.Claims;

namespace MohammadYounes.Owin.Security.MixedAuth
{
    // Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    public class MixedAuthAuthenticatedContext : BaseContext
    {
        // Initializes a <see cref="MixedAuthAuthenticatedContext"/>
        public MixedAuthAuthenticatedContext(
            IOwinContext context,
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            string accessToken)
            : base(context)
        {
            this.Identity = identity;
            this.Properties = properties;
            this.AccessToken = accessToken;
        }

        // Gets the MixedAuth access token
        public string AccessToken { get; private set; }

        // Gets or sets the <see cref="ClaimsIdentity"/> representing the user
        public ClaimsIdentity Identity { get; set; }

        // Gets or sets a property bag for common authentication properties
        public AuthenticationProperties Properties { get; set; }
    }
}

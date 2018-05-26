﻿/// <copyright file="MixedAuthHandler.cs" auther="Mohammad Younes">
/// Copyright 2014 Mohammad Younes. 
/// https://github.com/MohammadYounes/Owin-MixedAuth
/// 
/// Released under the MIT license
/// http://opensource.org/licenses/MIT
///
/// </copyright>

using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MohammadYounes.Owin.Security.MixedAuth
{
    /// <summary>
    /// The handler of mixed-authentication middleware.
    /// </summary>
    public class MixedAuthHandler : AuthenticationHandler<MixedAuthOptions>
    {
        #region ctor
        /// <summary>
        /// Initializes a new instance of <see cref="MixedAuthHandler "/>
        /// </summary>
        public MixedAuthHandler()
        {
            // Debugger.Break();
        }
        #endregion

        #region implementation
        /// <summary>
        ///  The core authentication logic.
        /// </summary>
        /// <returns>The ticket data provided by the authentication logic.</returns>
        protected async override System.Threading.Tasks.Task<AuthenticationTicket> AuthenticateCoreAsync()
        {

            AuthenticationProperties properties = UnpackStateParameter(Request.Query);

            if (properties != null)
            {
                var logonUserIdentity = Options.Provider.GetLogonUserIdentity(Context);

                if (!logonUserIdentity.AuthenticationType.Equals((Options.CookieOptions?.AuthenticationType ?? ""))
                    && logonUserIdentity.IsAuthenticated)
                {
                    AddCookieBackIfExists();

                    ClaimsIdentity claimsIdentity = new ClaimsIdentity(logonUserIdentity.Claims, Options.SignInAsAuthenticationType);

                    //name identifier
                    // Microsoft.Owin.Security.AuthenticationManagerExtensions: ExternalLoginInfo GetExternalLoginInfo(AuthenticateResult result)

                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, logonUserIdentity.User.Value, null, Options.AuthenticationType));

                    // Import custom claims.
                    List<Claim> customClaims = Options.Provider.ImportClaims(logonUserIdentity);
                    claimsIdentity.AddClaims(customClaims
                                                .Where(c => c.Type != ClaimTypes.NameIdentifier)
                                                .Select(c => new Claim(c.Type, c.Value, c.ValueType, Options.AuthenticationType)));

                    var ticket = new AuthenticationTicket(claimsIdentity, properties);

                    var context = new MixedAuthAuthenticatedContext(
                       Context,
                       claimsIdentity,
                       properties,
                       Options.AccessTokenFormat.Protect(ticket));

                    await Options.Provider.Authenticated(context);

                    return ticket;
                }
            }
            return new AuthenticationTicket(null, properties);
        }

        /// <summary>
        /// Decides if this request is to be handled by mixed-authentication middleware or not. 
        /// </summary>
        /// <returns>True if the request was handled.</returns>
        public async override System.Threading.Tasks.Task<bool> InvokeAsync()
        {

            // This is always invoked on each request. For passive middleware, only do anything if this is
            // for our callback path when the user is redirected back from the authentication provider.
            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
            {
                // Debugger.Break();

                //request token info
                if (!string.IsNullOrEmpty(Request.Query["access_token"]) && Request.QueryString.Value.IndexOf("token_info") >= 0)
                {
                    AuthenticationTicket ticket = null;
                    try
                    {
                        ticket = UnpackAccessTokenParameter(Request.Query);

                        Newtonsoft.Json.Linq.JObject token = new Newtonsoft.Json.Linq.JObject();
                        var claim = ticket.Identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                        token["user_id"] = claim != null ? claim.Value : "";
                        token["app_id"] = Options.ClientId;
                        Response.StatusCode = 200;
                        Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(token));
                    }
                    catch
                    {
                        Newtonsoft.Json.Linq.JObject result = new Newtonsoft.Json.Linq.JObject();
                        result["reason"] = "Invalid access token";
                        Response.StatusCode = 200;
                        Response.Write(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                    }

                    // Prevent further processing by the owin pipeline.
                    return true;
                }


                //Authenticate

                var logonUserIdentity = Options.Provider.GetLogonUserIdentity(Context);
                // If not authenticated or already authenticated using cookies, current identity will be the IIS App Pool, must re-authenticate.

                if (logonUserIdentity == null)
                {
                    Response.StatusCode = MixedAuthConstants.FakeStatusCode;
                    return true;
                }

                AuthenticationProperties state = null;

                if (logonUserIdentity.AuthenticationType.Equals((Options.CookieOptions?.AuthenticationType ?? ""))
                    || !logonUserIdentity.IsAuthenticated)
                {
                    var path = Request?.Path.ToString() ?? "";
                    bool isMixed = path.StartsWith("/MixedAuth");

                    // IEnumerable<
                    KeyValuePair <string, string[]> paramOne = Request.Query.FirstOrDefault();
                    isMixed = isMixed && "state".Equals(paramOne.Key ?? "");
                    if (isMixed)
                    {
                        var stateStr = paramOne.Value[0] as string ?? "";
                        state = StateUnprotect(Options, stateStr);

                    }

                    // fake status code, will be changed to 401 by HttpApplication.EndRequest event.
                    Response.StatusCode = MixedAuthConstants.FakeStatusCode;
                    // Prevent further processing by the owin pipeline.

                    return true;
                }

                // else
                
                    var ticket2 = await AuthenticateAsync();
                    //authenticatd
                    if (ticket2 != null)
                    {
                        Context.Authentication.SignIn(ticket2.Properties, ticket2.Identity);

                        Response.Redirect(ticket2.Properties.RedirectUri);
                        // Prevent further processing by the owin pipeline.
                        return true;
                    }
                
            }
            else
            {
                // add the cookie back if it does exist.
                AddCookieBackIfExists();
            }

            // Let the rest of the pipeline run.
            return false;

        }

        /// <summary>
        /// Deals with 401 challenge concerns.
        /// </summary>
        /// <returns>return null</returns>
        protected override Task ApplyResponseChallengeAsync()
        {
            var Request = this.Context.Request;
            var path = Request?.Path.ToString() ?? "";
            bool isExternalLogin = path.StartsWith("/Account/ExternalLogin");

            if (Response.StatusCode == MixedAuthConstants.FakeStatusCode)
            {
                // fake status code to be handled by HttpApplication.EndRequest Event.
                bool isMixed = path.StartsWith("/MixedAuth");

                if (isMixed)
                {
                    // IEnumerable<
                    KeyValuePair<string, string[]> paramOne = Request.Query.FirstOrDefault();
                    isMixed = isMixed && "state".Equals(paramOne.Key ?? "");
                    var stateData = paramOne.Value[0] as string ?? "";

                    var state = StateUnprotect(this.Options, stateData);
                }

                if (!isMixed)
                    return Task.FromResult<object>(null);
                else 
                    Response.StatusCode = 401;
            }

            if (Response.StatusCode != 401)
            {
                // Not a challege, move on.

                return Task.FromResult<object>(null);
            }

            // Debugger.Break();
            AuthenticationResponseChallenge challenge = 
                Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                //update redirect uri if not set.
                AuthenticationProperties state = challenge.Properties;
                if (String.IsNullOrEmpty(state.RedirectUri))
                    state.RedirectUri = Request.Scheme + Uri.SchemeDelimiter + Request.Host + Request.PathBase + Request.Path + Request.QueryString;

                //if (Context.Request.User.Identity.IsAuthenticated)

                var logonUserIdentity = Options.Provider.GetLogonUserIdentity(Context);

                if (logonUserIdentity != null)
                {
                    // If not authenticated or already authenticated using cookies, current identity will be the IIS App Pool, must re-authenticate.
                    if (logonUserIdentity.AuthenticationType
                        .Equals((Options.CookieOptions?.AuthenticationType ?? ""))
                        || !logonUserIdentity.IsAuthenticated)
                    {
                        //replace cookie if already authenticated, must re-authenticate.
                        ReplaceCookie();
                    }
                }

                string redirectUri = Request.Scheme +
                    Uri.SchemeDelimiter +
                    Request.Host +
                    RequestPathBase +
                    Options.CallbackPath + "?state=" + StateProtect(Options, state);


                var redirectContext = new MixedAuthApplyRedirectContext(Context, Options, state, redirectUri);
                Options.Provider.ApplyRedirect(redirectContext);


            }
            return Task.FromResult<object>(null);
        }



        #endregion

        public static string StateProtect(MixedAuthOptions Options, AuthenticationProperties state)
        {
            var data = Options.StateDataFormat.Protect(state);
            return Uri.EscapeDataString(data);
        }

        public static AuthenticationProperties StateUnprotect(MixedAuthOptions Options, string data)
        {
            var str = Uri.UnescapeDataString(data);
            if (string.IsNullOrWhiteSpace(str) || Options == null)
                return null;

            var state = Options.StateDataFormat.Unprotect(str);
            return state as AuthenticationProperties;
        }
        
        #region helpers
        /// <summary>
        /// Helper: reads state query string parameter.
        /// </summary>
        /// <param name="query">The <see cref="IReadableStringCollection"/></param>
        /// <returns>The value of the state parameter.</returns>
        private static string GetParameter(IReadableStringCollection query, string key)
        {
            IList<string> values = query.GetValues(key);
            if (values != null && values.Count == 1)
            {
                return values[0];
            }
            return null;
        }

        /// <summary>
        /// Helper: reads state query string parameter and unprotect it.
        /// </summary>
        /// <param name="query">The <see cref="IReadableStringCollection"/></param>
        /// <returns>The unprotected value of the state parameter.</returns>
        private AuthenticationProperties UnpackStateParameter(IReadableStringCollection query)
        {
            string state = GetParameter(query, "state");
            if (state != null)
            {
                return Options.StateDataFormat.Unprotect(state);
            }
            return null;
        }

        /// <summary>
        /// Helper: reads state query string parameter and unprotect it.
        /// </summary>
        /// <param name="query">The <see cref="IReadableStringCollection"/></param>
        /// <returns>The unprotected value of the state parameter.</returns>
        private AuthenticationTicket UnpackAccessTokenParameter(IReadableStringCollection query)
        {
            string access_token = GetParameter(query, "access_token");
            if (access_token != null)
            {
                return Options.AccessTokenFormat.Unprotect(access_token);
            }
            return null;
        }

        /// <summary>
        /// Reads the temp cookie and append it to the reponse.
        /// </summary>
        private void AddCookieBackIfExists()
        {
            if (!string.IsNullOrEmpty(Context.Request.Cookies[MixedAuthConstants.TempCookieName]))
            {
                //extract ticket
                AuthenticationTicket ticket = Options.CookieOptions.TicketDataFormat.Unprotect(Context.Request.Cookies[MixedAuthConstants.TempCookieName]);
                if (ticket != null)
                {
                    //delete mixed auth temporary cookie
                    Options.CookieOptions.CookieManager.DeleteCookie(Context,
                        MixedAuthConstants.TempCookieName,
                        Options.CookieOptions.ToCookieOptions(DateTime.UtcNow.AddDays(-1)));

                    //add asp.net cookie
                    Options.CookieOptions.CookieManager.AppendResponseCookie(Context,
                        Options.CookieOptions.CookieName,
                        Options.CookieOptions.TicketDataFormat.Protect(ticket),
                        Options.CookieOptions.ToCookieOptions(ticket.Properties.ExpiresUtc.Value.ToUniversalTime().DateTime));
                }
            }
        }

        /// <summary>
        /// Delete the cookies-authentication middleware cookie and saves it in a temporary cookie.
        /// </summary>
        private void ReplaceCookie()
        {
            var CookieName = Options.CookieOptions?.CookieName ?? "Mixed.Auth";
            string cookieValue = Context.Request.Cookies[CookieName];

            if (!string.IsNullOrEmpty(cookieValue))
            {
                //extract ticket
                AuthenticationTicket ticket = Options.CookieOptions.TicketDataFormat.Unprotect(cookieValue);
                if (ticket != null)
                {
                    //delete asp.net cookie
                    Options.CookieOptions.CookieManager.DeleteCookie(Context,
                        Options.CookieOptions.CookieName,
                        Options.CookieOptions.ToCookieOptions(DateTime.UtcNow.AddDays(-1)));

                    //add mixed auth temporary cookie
                    Options.CookieOptions.CookieManager.AppendResponseCookie(Context,
                        MixedAuthConstants.TempCookieName,
                        Options.CookieOptions.TicketDataFormat.Protect(ticket),
                        Options.CookieOptions.ToCookieOptions(DateTime.UtcNow.AddMinutes(5)));
                }
            }

        }
        #endregion

    }
}

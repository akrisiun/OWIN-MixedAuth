// https://blogs.iis.net/brian-murphy-booth/iis7-using-basic-authentication-may-cause-premature-user-lockouts
/*
   [DllImport(ModName.ADVAPI32_FULL_NAME, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern int LogonUser(String username, String domain, String password, 
				int dwLogonType, int dwLogonProvider, ref IntPtr phToken);
*/
     
using System; 
using System.ComponentModel; 
using System.Runtime.InteropServices; 
using System.Text; 
using System.Web;

	
//	http://referencesource.microsoft.com/#System.IdentityModel/System/IdentityModel/Selectors/
//	WindowsUserNameSecurityTokenAuthenticator.cs,52
	
namespace System.IdentityModel.Tokens
{
    /// <summary>
    /// The token handler will validated the Windows Username token.
    /// </summary>
    public class WindowsUserNameSecurityTokenHandler : UserNameSecurityTokenHandler
    {
        public WindowsUserNameSecurityTokenHandler()
        {
        }
 
        // Returns true to indicate that the token handler can Validate
        public override bool CanValidateToken
        {
            get
            {
                return true;
            }
        }
 
        // Validates a <see cref="UserNameSecurityToken"/>.
        // <exception cref="ArgumentException">If username is not if the form 'user\domain'.</exception>
        // <exception cref="SecurityTokenValidationException">LogonUser using the given token failed.</exception>
        public override ReadOnlyCollection<ClaimsIdentity> ValidateToken(SecurityToken token)
        {
            if (token == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");
            }
 
            UserNameSecurityToken usernameToken = token as UserNameSecurityToken;
            if (usernameToken == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token",
					  SR.GetString(SR.ID0018, typeof(UserNameSecurityToken)));
            }
 
            if (this.Configuration == null)
            {
                throw DiagnosticUtility.ThrowHelperInvalidOperation(SR.GetString(SR.ID4274));
            }
 
            try
            {
                string userName = usernameToken.UserName;
                string password = usernameToken.Password;
                string domain = null;
                string[] strings = usernameToken.UserName.Split('\\');
                if (strings.Length != 1)
                {
                    if (strings.Length != 2 || string.IsNullOrEmpty(strings[0]))
                    {
                        // Only support one slash and domain cannot be empty (consistent with windowslogon).
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("token", SR.GetString(SR.ID4062));
                    }
 
                    // This is the downlevel case - domain\userName
                    userName = strings[1];
                    domain = strings[0];
                }
 
                const uint LOGON32_PROVIDER_DEFAULT = 0;
                const uint LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
				
                SafeCloseHandle tokenHandle = null;
                try
                {
                    if (!NativeMethods.LogonUser(userName, domain, password, 
						 LOGON32_LOGON_NETWORK_CLEARTEXT, LOGON32_PROVIDER_DEFAULT, out tokenHandle))
                    {
                        int error = Marshal.GetLastWin32Error();
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenValidationException(SR.GetString(SR.ID4063, userName), new Win32Exception(error)));
                    }
 
                    WindowsIdentity windowsIdentity = new WindowsIdentity(tokenHandle.DangerousGetHandle(), 
						AuthenticationTypes.Password, WindowsAccountType.Normal, true);
 
                    // PARTIAL TRUST: will fail when adding claims, AddClaim is SecurityCritical.
                    windowsIdentity.AddClaim(new Claim(ClaimTypes.AuthenticationInstant, XmlConvert.ToString(DateTime.UtcNow, DateTimeFormats.Generated), ClaimValueTypes.DateTime));
                    windowsIdentity.AddClaim(new Claim(ClaimTypes.AuthenticationMethod, AuthenticationMethods.Password));
 
                    if (this.Configuration.SaveBootstrapContext)
                    {
                        if (RetainPassword)
                        {
                            windowsIdentity.BootstrapContext = new BootstrapContext(usernameToken, this);
                        }
                        else
                        {
                            windowsIdentity.BootstrapContext = new BootstrapContext(new UserNameSecurityToken(usernameToken.UserName, null), this);
                        }
                    }
 
                    this.TraceTokenValidationSuccess(token);
 
                    List<ClaimsIdentity> identities = new List<ClaimsIdentity>(1);
                    identities.Add(windowsIdentity);
                    return identities.AsReadOnly();
                }
                finally
                {
                    if (tokenHandle != null)
                    {
                        tokenHandle.Close();
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }
 
                this.TraceTokenValidationFailure(token, e.Message);
                throw e;
            }
        }
    }
	/*
		protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateUserNamePasswordCore(string userName, string password)
        {
            string domain = null;
            string[] strings = userName.Split('\\');
            if (strings.Length != 1)
            {
                if (strings.Length != 2 || string.IsNullOrEmpty(strings[0]))
                {
                    // Only support one slash and domain cannot be empty (consistent with windowslogon).
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.IncorrectUserNameFormat));
                }
 
                // This is the downlevel case - domain\userName
                userName = strings[1];
                domain = strings[0];
            }
 
            const uint LOGON32_PROVIDER_DEFAULT = 0;
            const uint LOGON32_LOGON_NETWORK_CLEARTEXT = 8;
			
            SafeCloseHandle tokenHandle = null;
            try
            {
                if (!NativeMethods.LogonUser(userName, domain, password, 
					 LOGON32_LOGON_NETWORK_CLEARTEXT, LOGON32_PROVIDER_DEFAULT, out tokenHandle))
                {
                    int error = Marshal.GetLastWin32Error();
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
						  new SecurityTokenValidationException(SR.GetString(SR.FailLogonUser, userName), new Win32Exception(error)));
                }
 
                WindowsIdentity windowsIdentity = new WindowsIdentity(tokenHandle.DangerousGetHandle(), SecurityUtils.AuthTypeBasic);
                WindowsClaimSet claimSet = 
					   new WindowsClaimSet(windowsIdentity, SecurityUtils.AuthTypeBasic, this.includeWindowsGroups, false);
                return SecurityUtils.CreateAuthorizationPolicies(claimSet, claimSet.ExpirationTime);
            }
            finally
            {
                if (tokenHandle != null)
                    tokenHandle.Close();
            }
        }
    }
	*/
	
}
	
namespace IIS
{ 

    public class BasicLogonModule  : IHttpModule 
    { 
       #region Members 
        private bool _disposed; 
        #endregion

        #region Imports 
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)] 
        private static extern bool LogonUser( 
            string Username, 
            string Domain, 
            string Password, 
            LOGON32_LOGON LogonType, 
            LOGON32_PROVIDER LogonProvider, 
            ref IntPtr Token 
            );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)] 
        private static extern bool CloseHandle( 
            IntPtr handle 
            ); 
        #endregion

        #region Enumerations 
        public enum LOGON32_PROVIDER : uint 
        { 
            DEFAULT = 0, 
            WINNT35 = 1, 
            WINNT40 = 2, 
            WINNT50 = 3 
        } 
        public enum LOGON32_LOGON : uint 
        { 
            /// <summary>This logon type is intended for users who will be interactively using the computer, such as a user being 
            /// logged on by a terminal server, remote shell, or similar process. This logon type has the additional expense of 
            /// caching logon information for disconnected operations; therefore, it is inappropriate for some client/server applications, 
            /// such as a mail server.</summary> 
            INTERACTIVE = 2, 
            /// <summary>This logon type is intended for high performance servers to authenticate plaintext passwords. The LogonUser 
            /// function does not cache credentials for this logon type.</summary> 
            NETWORK = 3, 
            /// <summary>This logon type is intended for batch servers, where processes may be executing on behalf of a user 
            /// without their direct intervention. This type is also for higher performance servers that process many plaintext 
            /// authentication attempts at a time, such as mail or Web servers. The LogonUser function does not cache credentials 
            /// for this logon type.</summary> 
            BATCH = 4, 
            /// <summary>Indicates a service-type logon. The account provided must have the service privilege enabled.</summary> 
            SERVICE = 5, 
            /// <summary>This logon type is for GINA DLLs that log on users who will be interactively using the computer. This logon 
            /// type can generate a unique audit record that shows when the workstation was unlocked.</summary> 
            UNLOCK = 7, 
            /// <summary>This logon type preserves the name and password in the authentication package, which allows the server to make 
            /// connections to other network servers while impersonating the client. A server can accept plaintext credentials from a client, 
            /// call LogonUser, verify that the user can access the system across the network, and still communicate with other servers.</summary> 
            NETWORK_CLEARTEXT = 8, 
            /// <summary>This logon type allows the caller to clone its current token and specify new credentials for outbound connections. 
            /// The new logon session has the same local identifier but uses different credentials for other network connections. This logon type 
            /// is supported only by the LOGON32_PROVIDER_WINNT50 logon provider.</summary> 
            NEW_CREDENTIALS = 9 
        } 
        #endregion

        #region Methods 
       public void Init(HttpApplication application) 
        { 
            application.BeginRequest += new EventHandler(this.OnBeginRequest); 
        } 
        private void OnBeginRequest(object sender, EventArgs e) 
        { 
            HttpContext context = HttpContext.Current;

            if (context != null) 
            { 
                string authorization = context.Request.ServerVariables["HTTP_AUTHORIZATION"]; 
                if (authorization != null) 
                { 
                    if (authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)) 
                    { 
                        int index = authorization.IndexOf(" "); 
                        if (index != -1) 
                        { 
                            try 
                            { 
                                authorization = authorization.Substring(index + 1);

                                UTF8Encoding encoder = new UTF8Encoding(); 
                                Decoder decoder = encoder.GetDecoder();

                                byte[] bytes = Convert.FromBase64String(authorization); 
                                int count = decoder.GetCharCount(bytes, 0, bytes.Length); 
                                char[] characters = new char[count]; 
                                decoder.GetChars(bytes, 0, bytes.Length, characters, 0); 
                                authorization = new string(characters); 
                                index = authorization.IndexOf(":"); 
                                if (index != -1) 
                                { 
                                    IntPtr token = IntPtr.Zero; 
                                    string domainName = null; 
                                    string userName = authorization.Substring(0, index); 
                                    string password = authorization.Substring(index + 1);

                                    index = userName.IndexOf("\\"); 
                                    if (index != -1) 
                                    { 
                                        domainName = userName.Substring(0, index); 
                                        userName = userName.Substring(index + 1); 
                                    } 
                                    bool success = false;

                                    try 
                                    { 
                                        success = LogonUser( 
                                            userName, 
                                            domainName, 
                                            password, 
                                            LOGON32_LOGON.NETWORK_CLEARTEXT, 
                                            LOGON32_PROVIDER.DEFAULT, 
                                            ref token 
                                            ); 
                                    } 
                                    finally 
                                    { 
                                        if (!success) 
                                        { 
                                            Win32Exception exception = new Win32Exception(); 
                                            context.Response.AppendToLog("401Reason=0x" + exception.NativeErrorCode.ToString("X"));

                                            context.Response.Clear(); 
                                            context.Response.StatusCode = 401; 
                                            context.Response.SubStatusCode = 1; 
                                            context.Response.StatusDescription = exception.Message;
                                            context.Response.End(); 
                                        }

                                        if (token != IntPtr.Zero) 
                                            CloseHandle(token); 
                                    } 
                                } 
                            } 
                            catch (Exception ex) 
                            { 
                                context.Trace.Write("OnBeginRequest", "Error decoding Authorization header: " + ex.Message); 
                            } 
                        } 
                    } 
                } 
            } 
        } 
    
		public void Dispose() 
        { 
            if (!this._disposed) 
            { 
                lock (this) 
                { 
                    if (!this._disposed) 
                    { 
                        this._disposed = true;

                        HttpContext context = HttpContext.Current;

                        if (context != null) 
                        { 
                            HttpApplication application = context.ApplicationInstance; 
                            application.BeginRequest -= new EventHandler(this.OnBeginRequest); 
                        } 
                    } 
                } 
            } 
        } 
        #endregion 
    } 
	
} 
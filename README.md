### Problem with 401

```
#1
ubuntu 16.04 error
/mnt/d/Beta/Owin/OWIN-MixedAuth/src/SPA/SPA.csproj(295,3): error MSB4019: The imported project 
/root/.dotnet/sdk/1.0.4/Microsoft/VisualStudio/v10.0/WebApplications/Microsoft.WebApplication.targets" was not found. 
Confirm that the path in the <Import> declaration is correct, and that

#2
HTTP Error 401.1 - Unauthorized
You do not have permission to view this directory or page using the credentials that you supplied.
Most likely causes:
The username supplied to IIS is invalid.
The password supplied to IIS was not typed correctly. 
Incorrect credentials were cached by the browser.
IIS could not verify the identity of the username and password provided.
The resource is configured for Anonymous authentication, but the configured anonymous account
 either has an invalid password or was disabled.
The server is configured to deny login privileges to the authenticating user or the group in which the user is a member.
Invalid Kerberos configuration may be the cause if all of the following are true:
Integrated authentication was used.
the application pool identity is a custom account.
the server is a member of a domain.
Things you can try:
Verify that the username and password are correct, and are not cached by the browser.
Use a different username and password.
If you are using a custom anonymous account, verify that the password has not expired.
Verify that the authenticating user or the user's group, has not been denied login access to the server.
Verify that the account was not locked out due to numerous failed login attempts.
If you are using authentication and the server is a member of a domain, verify that you have 
configured the application pool identity using the utility SETSPN.exe, or changed the configuration so 
that NTLM is the favored authentication type.
Check the failed request tracing logs for additional information about this error. For more information, click here. 
Detailed Error Information:
Module
   WindowsAuthenticationModule
Notification
   AuthenticateRequest
Handler
   ExtensionlessUrlHandler-Integrated-4.0
Error Code
   0xc000006d
Requested URL
   http://localhost:28636/MixedAuth?state=q_WrRigbn0vwAf_6SrX-YCPbUxeFGJ-BnthVwktmTwrqmTVPi_Kf7Cj465-sMJnND3I4s8nLs6uWtBLWqW4fOSmqgJsIHXCDYbgwbwlvLHAVqTE6wWQWFKllWp_ZpY9YXE2wHZdq_t2HQz3akGC81Q
Physical Path
   OWIN-MixedAuth\src\SPA\MixedAuth
```

# OWIN Mixed Authentication

OWIN middleware implementation mixing Windows and Forms Authentication.

![mixed-auth](https://cloud.githubusercontent.com/assets/4712046/4690732/0bbe62f8-56f8-11e4-8757-2d10cdeca17e.png)

## Install with [NuGet](https://www.nuget.org/packages/OWIN-MixedAuth/)
```
PM> Install-Package OWIN-MixedAuth
```

# Running the samples

Before running the samples, make sure to unlock `windowsAuthentication` section:

### IIS
1. Open IIS Manager, select the server node, then Feature Delegation.
2. Set `Authentication - Windows` to `Read/Write`

 ![unlock-section](https://cloud.githubusercontent.com/assets/4712046/4689687/d28f8df8-56c6-11e4-9b88-8f5cb769ae93.png)

### IIS Express
1. Open **applicationhost.config** located at:
  * **Pre VS2015**: *$:\Users\\{username}\Documents\IISExpress\config* 
  * **VS2015**: *$(SolutionDir)\\.vs\config*
2. Search for `windowsAuthentication` section and update `overrideModeDefault` value to `Allow`.

  ```XML
   <section name="windowsAuthentication" overrideModeDefault="Allow" />
  ```

# Usage

1. Add reference to `MohammadYounes.Owin.Security.MixedAuth.dll`

2. Register `MixedAuth` in **Global.asax**
  ```C#
  //add using statement
  using MohammadYounes.Owin.Security.MixedAuth;

  public class MyWebApplication : HttpApplication
  {
     //ctor
     public MyWebApplication()
     {
       //register MixedAuth
       this.RegisterMixedAuth();
     }
     .
     .
     .
  }
```
3. Use `MixedAuth` in **Startup.Auth.cs**
  ```C#
  //Enable Mixed Authentication
  //As we are using LogonUserIdentity, its required to run in PipelineStage.PostAuthenticate
  //Register this after any middleware that uses stage marker PipelineStage.Authenticate

  app.UseMixedAuth(cookieOptions);
  ```
  **Important!** MixedAuth is required to run in `PipelineStage.PostAuthenticate`, make sure the use statement is after any other middleware that uses `PipelineStage.Authenticate`. See [OWIN Middleware in the IIS integrated pipeline](http://www.asp.net/aspnet/overview/owin-and-katana/owin-middleware-in-the-iis-integrated-pipeline).

4. Enable Windows authentication in **Web.config**

  ```XML
  <!-- Enable Mixed Auth -->
  <location path="MixedAuth">
    <system.webServer>
      <security>
        <authentication>
          <windowsAuthentication enabled="true" />
        </authentication>
      </security>
    </system.webServer>
  </location>
  ```
  **Important!** Enabling windows authentication for a sub path requires `windowsAuthentication` section to be unlocked at a parent level.

------

#### Importing Custom Claims

Adding custom claims in OWIN-MixedAuth is pretty straightforward, simply use `MixedAuthProvider` and place your own logic for fetching those custom claims. 

The following example shows how to import user Email, Surname and GiveName from Active Directory:
```C#
// Enable mixed auth
 app.UseMixedAuth(new MixedAuthOptions()
 {
   Provider = new MixedAuthProvider()
   {
     OnImportClaims = identity =>
     {
       List<Claim> claims = new List<Claim>();
       using (var principalContext = new PrincipalContext(ContextType.Domain)) //or ContextType.Machine
       {
         using (UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(principalContext, identity.Name))
         {
           if (userPrincipal != null)
           {
             claims.Add(new Claim(ClaimTypes.Email, userPrincipal.EmailAddress ?? string.Empty));
             claims.Add(new Claim(ClaimTypes.Surname, userPrincipal.Surname ?? string.Empty));
             claims.Add(new Claim(ClaimTypes.GivenName, userPrincipal.GivenName ?? string.Empty));
           }
         }
       }
       return claims;
     }
   }
 }, cookieOptions);
```
------
##### Please [share any issues](https://github.com/MohammadYounes/OWIN-MixedAuth/issues?state=open) you may have.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SPA.Results
{
    public class ChallengeResult : IHttpActionResult
    {
        public ChallengeResult(string loginProvider, ApiController controller)
        {
            LoginProvider = loginProvider;
            Request = controller.Request;
        }

        public string LoginProvider { get; set; }
        public HttpRequestMessage Request { get; set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return null;
        }

        /*
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            // Owin.System.Web
            //HttpRequestBase req = Request
            //HttpContextBase ctx = 
            //var owinCtx = HttpContextBaseExtensions.GetOwinContext(Request);
            //owinCtx.Authentication.Challenge(LoginProvider);

            var httpOwin = OwinHttpRequestMessageExtensions.GetOwinContext(Request);
            // Request.GetOwinContext()
            httpOwin.Authentication.Challenge(LoginProvider);

            / * 
            #region Assembly System.Net.Http, Version=4.1.1.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
            // System.Net.Http.dll

            // HttpStatusCode.Unauthorized); 
            // Unauthorized = 401
            // to assembly 'System.Net.Primitives, Version=4.0.10.0 
            return Task.FromResult(null);
            * /

            HttpResponseMessage response = new HttpResponseMessage(Unauthorized); 
            // response.RequestMessage = Request;
            
            return Task.FromResult(response);
        }
        */

        public const int Unauthorized = 401;
    }
}

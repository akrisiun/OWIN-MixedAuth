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
            // Owin.System.Web
            //HttpRequestBase req = Request
            //HttpContextBase ctx = 
            //var owinCtx = HttpContextBaseExtensions.GetOwinContext(Request);
            //owinCtx.Authentication.Challenge(LoginProvider);

            var httpOwin = OwinHttpRequestMessageExtensions.GetOwinContext(Request);
            // Request.GetOwinContext()
            httpOwin.Authentication.Challenge(LoginProvider);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            response.RequestMessage = Request;
            return Task.FromResult(response);
        }
    }
}

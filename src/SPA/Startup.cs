using Microsoft.Owin;
using Owin;
using System.Diagnostics;

[assembly: OwinStartup(typeof(SPA.Startup))]

namespace SPA
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Debugger.Break();
            // Antrl: 3.4.1.9004 redirected to 3.5.0.2.

            ConfigureAuth(app);
        }
    }

}

namespace SPA.Web
{ 
    using System.Web;
    using MohammadYounes.Owin.Security.MixedAuth;
    using System.Web.Mvc;
    using System.Web.Http;
    using System.Web.Optimization;
    using System.Web.Routing;

    public class MvcApplication1 : HttpApplication
    {
        public MvcApplication1()
        {
            this.RegisterMixedAuth();
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

}

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
            // Antrl: 3.4.1.9004 redirected to 3.5.0.2.

            ConfigureAuth(app);
        }
    }

}

namespace SPA.Web
{ 
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Http;
    using System.Web.Optimization;
    using System.Web.Routing;

    public class MvcApplication1 : HttpApplication
    {
        public MvcApplication1()
        {
            MohammadYounes.Owin.Security.MixedAuth.MixedAuthExtensions.RegisterMixedAuth(this);
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

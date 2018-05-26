<%@ Page Language="C#" %>
<%@ Import Namespace="System.Linq" %>
<%@ Import Namespace="System.Reflection" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="System.Web" %>
<%@ Import Namespace="System.Web.Hosting" %>
<%
    // using System.Linq; -> Import Namespace="System.Linq" -> // http://www.mono-project.com/docs/faq/aspnet/
    Response.Write(".net Version: " + System.Environment.Version.ToString());
    try { if (null != typeof(System.Web.Compilation.BuildManager).GetMember("get_TargetFramework", BindingFlags.Static | BindingFlags.Public)) {
            string TargetFramework = System.Web.Compilation.BuildManager.TargetFramework?.Version.ToString();
            Response.Write($" System.Web.Compilation.BuildManager.TargetFramework.Version=net{TargetFramework ?? ""}");
        } } catch {}
    Response.Write($" | OSVersion.Platform={Environment.OSVersion.Platform.ToString()} | Environment.OSVersion={Environment.OSVersion.VersionString}");
    Response.Write("\n<br> | UsingIntegratedPipeline=" + System.Web.HttpRuntime.UsingIntegratedPipeline.ToString());
    try {
        var IISVersion = typeof(System.Web.HttpRuntime).GetMember("get_IISVersion", BindingFlags.Static | BindingFlags.Public);
        if (IISVersion != null)
            Response.Write($", IISVersion={System.Web.HttpRuntime.IISVersion?.ToString() ?? ""}");

        Response.Write($@"{'\n'} | Is64BitProcess={Environment.Is64BitProcess} 
                            | Is64BitOperatingSystem={Environment.Is64BitOperatingSystem}");
    } catch {}

    Response.Write("<br/>AspInstallDirectory=" + System.Web.HttpRuntime.AspInstallDirectory.ToString());
    Response.Write("<br/>AppDomain.BaseDirectory=" + System.AppDomain.CurrentDomain.BaseDirectory);

    Response.Write($@"<br>{'\n'}Links: 
        <a href=""api/status"">/api/status</a> |
        <a href=""api/zeegit"">/api/zeegit</a> 
        {'\n'}");

    var VirtualPath = (HostingEnvironment.ApplicationVirtualPath ?? "") + "/";
    
    try {

        Response.Write("<br/>");
        object config = AppDomain.CurrentDomain.GetData("webapi.config");
        if (config != null)
        {
            var routes = (config as System.Web.Http.HttpConfiguration).Routes;
            // (HttpRouteCollection) a System.Web.Http.WebHost.Routing.HostedHttpRouteCollection
            // System.Web.Http.Routing.IHttpRoute

            Response.Write($"<br/><b>WebApi</b> Routes: {routes}");
            Response.Write($@"<div style=""display: block; border: 1px solid silver;"">");
            // table-layout: fixed; 

            var routesNum = routes.GetEnumerator();
            while (routesNum.MoveNext())
            {
                var route = routesNum.Current;
                // flex table: https://codepen.io/anon/pen/JMZLVx  https://hashnode.com/post/really-responsive-tables-using-css3-flexbox-cijzbxd8n00pwvm53sl4l42cx 
                Response.Write(@"<div style=""
                    display: flex; flex-direction: row; flex-grow: 0;  
                    flex-wrap: wrap;  margin-top: 10px; border-collapse: collapse; 
                    "">");
                Response.Write($"<div><b>{route?.ToString() ?? ""}</b></div></div>");

                var apiRoute = route as System.Web.Http.Routing.IHttpRoute; // System.Web.Http.Routing.RouteCollectionRoute;
                if (apiRoute  != null)
                {
                    var tokens = apiRoute.DataTokens;

                    var listRoute = apiRoute as System.Collections.Generic.IEnumerable<System.Web.Http.Routing.IHttpRoute>;
                    if (listRoute != null)
                    {
                        // Response.Write("<br>\n[");
                        foreach (var item in listRoute)
                        {
                            Response.Write($@"{' '}<div style=""
                                display: flex; flex-direction: row; flex-grow: 0;  flex-wrap: wrap;  border-collapse: collapse; padding-left: 10px;
                                "">");
                            {
                                Response.Write($@"{'\n'}<div style=""flex-grow: 1; width: 120px;"">");
                                Response.Write($"{item?.RouteTemplate ?? ""}</div>");

                                if ((item?.RouteTemplate ?? "").Contains("/"))
                                {
                                    Response.Write($@"<div style=""flex-grow: 1; width: 200px;"">");
                                    Response.Write($@"<a href=""{VirtualPath}{item.RouteTemplate}"">{VirtualPath}{item.RouteTemplate}</a> ");
                                    Response.Write($@"</div>");
                                }

                                var end = "";
                                if (!"System.Web.Http.Routing.HttpRoute".Equals(item?.ToString() ?? ""))
                                    end = item?.ToString() ?? "";

                                Response.Write($@"<div 
                                        style=""flex-grow: 1; width: 400px;""
                                        > {end}</div>");
                            }
                            Response.Write("</div>");
                        }
                    }
                }

                // var hostRoute = route as System.Web.Http.WebHost.Routing.HostedHttpRoute;
                // public Net.Http.HttpMessageHandler Handler { get; }
            }
            Response.Write($@"</div>");
        }
        else
            Response.Write("<br/><b>no WebApi routes:</b>");

        Response.Write("<br/>");
    } catch {}

    var SanitexMvcHttp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"bin{Path.DirectorySeparatorChar}Sanitex.MvcHttp.dll");
    var asmMvc =!File.Exists(SanitexMvcHttp) ? null : Assembly.LoadFile(SanitexMvcHttp);
    if (asmMvc== null) {
        Response.Write("<br/>no Sanitex.MvcHttp.dll: Sanitex.RazorGenerator.Mvc.EngineDebug");
        try { Sanitex.RazorGenerator.Mvc.EngineDebug.Output(Response);
        } catch {}
    }
    else
    {
        var EngineDebug = asmMvc.GetType("Sanitex.RazorGenerator.Mvc.EngineDebug");
        var Output = EngineDebug?.GetMethod("Output", BindingFlags.Static | BindingFlags.Public);
        Output?.Invoke(null, new object[] { Response });
    }

    Response.Write("<br/>Assemblies=" + System.AppDomain.CurrentDomain.GetAssemblies().Length.ToString());

    var list = System.Linq.Enumerable.ToList(System.AppDomain.CurrentDomain.GetAssemblies());

    foreach (var asm in list.OrderBy(s => s.FullName))
    {
        try
        {
            Response.Write("<br/>" + asm.CodeBase.Replace("file:///", ""));
        }
        catch
        {
            // case: Anonymously Hosted DynamicMethods Assembly
            Response.Write("<br/>" + asm.FullName);
        }
    }

%>
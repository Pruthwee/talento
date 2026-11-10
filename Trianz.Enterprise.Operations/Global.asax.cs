using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Net;

namespace Trianz.Enterprise.Operations
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; //TLS for security
            
            // Load configuration from AWS Systems Manager Parameter Store at startup
            // This replaces Web.config transformation files by externalizing configuration
            // to Parameter Store, enabling environment-specific configuration without rebuilds
            Trianz.Enterprise.Operations.General.ParameterStoreConfig.LoadParameters();
            
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Register Redis-backed controller factory to replace in-memory TempData provider
            // with Redis (Amazon ElastiCache) for distributed TempData state across instances
            ControllerBuilder.Current.SetControllerFactory(new Trianz.Enterprise.Operations.General.RedisControllerFactory());

			AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;//System.Security.Claims.ClaimTypes.Name;
		}

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex is HttpException && ((HttpException)ex).GetHttpCode() == 404)
            {
                Response.Redirect("error/notfound");
            }
        }
        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
            Response.AddHeader("Strict-Transport-Security", "max-age=300");
            Response.AddHeader("X-Frame-Options", "SAMEORIGIN");
            Response.AddHeader("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'");
        }

        //protected void Application_EndRequest(object sender, EventArgs e)
        //{
        //    if (Response.StatusCode == 401 && Request.IsAuthenticated)
        //    {
        //        //Response.StatusCode = 303;
        //        Response.Clear();
        //        //Response.Redirect("error/notauthorized");
        //        //Response.Redirect("http://www.google.com");
        //        Response.End();
        //    }
        //}
    }
}

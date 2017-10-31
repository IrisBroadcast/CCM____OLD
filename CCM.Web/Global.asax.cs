using System;
using System.Diagnostics;
using System.Web.Http;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CCM.Web.Infrastructure;
using CCM.Web.Infrastructure.MvcFilters;
using CCM.WebCommon.Infrastructure.WebApi;
using NLog;

namespace CCM.Web
{
    public class MvcApplication : HttpApplication
    {
        protected static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            AutoMapperWebConfiguration.Configure();

            // INFO: Ordningen på registreringarna är viktig. WebApi före Mvc.
            
            // WebApi
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.Filters.Add(new StopwatchAttribute(true));
            GlobalConfiguration.Configuration.Filters.Add(new ActivityIdAttribute());

            // Mvc
            GlobalFilters.Filters.Add(new ElapsedTimeFilter());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // For logging
            Trace.CorrelationManager.ActivityId = Guid.NewGuid();
        }

    }
}

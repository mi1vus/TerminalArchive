using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TerminalArchive.WebUI
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //ссылки типа http://localhost:53985/Page2
            routes.MapRoute(
                name: null,
                url: "Page{page}",
                defaults: new { controller = "TerminalMonitoring", action = "List" }
            );

            //ссылка типа http://localhost:53985 = http://localhost:53985/TerminalMonitoring/List
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "TerminalMonitoring", action = "List", id = UrlParameter.Optional }
            );
        }
    }
}

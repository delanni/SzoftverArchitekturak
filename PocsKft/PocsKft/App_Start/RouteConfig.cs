using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PocsKft
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Download",
                url: "Download/{*path}",
                defaults: new { controller = "Home", action = "Download" }
            );

            routes.MapRoute(
                name: "Search",
                url: "Search",
                defaults: new { controller = "Home", action = "Search" }
            );

            routes.MapRoute(
                name: "Default",
                url: "Account/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: null,
                url: "{*path}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
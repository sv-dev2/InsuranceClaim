using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace InsuranceClaim
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

          

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "InsuranceHome", action = "Index", id = UrlParameter.Optional }
            );


            routes.MapRoute(
             name: "",
             url: "Payment/{controller}/{action}/{id}/{TotalPremium}/{Email}/{PolicyNumber}",
             defaults: new { controller = "Paypal", action = "InitiatePaynowTransaction", id = UrlParameter.Optional, Email = UrlParameter.Optional, TotalPremiumPaid = UrlParameter.Optional, PolicyNumber = UrlParameter.Optional }
         );
        }
    }
}

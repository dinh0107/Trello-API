using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;
using Trello_API.Helper;

namespace Trello_API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var origins = "http://localhost:4200,https://trello-clone-wske.vercel.app";
            var headers = "*";
            var methods = "*";

            var cors = new EnableCorsAttribute(origins, headers, methods)
            {
                SupportsCredentials = true
            };

            config.EnableCors(cors);

            config.MessageHandlers.Add(new JwtCookieAuthHandler());

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}

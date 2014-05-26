using System;
using System.Web.Routing;

namespace DotNetCqs.AspNet
{
    /// <summary>
    /// Wrapper for our <c>EnableCorsProxy</c> extension method
    /// </summary>
    public static class RouteCollectionExtensions
    {
        /// <summary>
        /// Activates the cors proxy
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="url">Default route is "corsproxy/". It's the URI that the ajax request must redirect to</param>
        public static void EnableDotNetCqs(this RouteCollection routes, string url = "dotnetcqs/")
        {
            if (routes == null) throw new ArgumentNullException("routes");
            if (url == null) throw new ArgumentNullException("url");
            routes.Add(new DotNetCqsRoute(url));
        }
    }
}

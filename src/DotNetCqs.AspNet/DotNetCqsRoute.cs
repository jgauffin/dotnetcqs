using System.Web.Routing;

namespace DotNetCqs.AspNet
{
    /// <summary>
    /// Our route implementation which uses the <see cref="DotNetCqsHttpHandler"/>.
    /// </summary>
    public class DotNetCqsRoute : Route
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Web.Routing.Route"/> class, by using the specified URL pattern and handler class. 
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        public DotNetCqsRoute(string url)
            : base(url, new DotNetCqsRouteHandler())
        {
        }
    }
}
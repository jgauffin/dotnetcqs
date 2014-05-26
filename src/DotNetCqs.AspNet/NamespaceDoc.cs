using System.Runtime.CompilerServices;

namespace DotNetCqs.AspNet
{
    /// <summary>
    /// Proxy used to gracefully downgrade CORS Ajax requests to proxied HTTP requests for IE9 and below.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To get started edit your App_Start\RouteConfig.cs in your front-end project to enable the CorsProxy.
    /// </para>
    /// <code>
    /// public class RouteConfig
    /// {
    ///     public static void RegisterRoutes(RouteCollection routes)
    ///     {
    ///         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
    /// 
    ///         // add this line
    /// 		// (important, must be added before the default route.)
    ///         routes.EnableCorsProxy();
    /// 
    ///         routes.MapRoute(
    ///             name: "Default",
    ///             url: "{controller}/{action}/{id}",
    ///             defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
    ///         );
    /// 
    ///     }
    /// }
    /// </code>
    /// <para>
    /// Next you have to include our javascript to automatically redirect IE9 ajax requests through the HttpProxy. Add <code>jquery-corsproxy-{version}.js</code> to your App_Start\Bundle.config as this:
    /// </para>
    /// <code>
    /// public class BundleConfig
    /// {
    /// 	public static void RegisterBundles(BundleCollection bundles)
    /// 	{
    /// 		bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
    /// 					"~/Scripts/jquery-{version}.js",
    /// 					"~/Scripts/jquery-corsproxy-{version}.js")); //new line
    /// 		
    /// 		// [...]
    /// 	}
    /// }
    /// </code>
    /// <para>
    /// That's it.
    /// </para>
    /// </remarks>
    [CompilerGenerated]
    class NamespaceDoc
    {
    }
}

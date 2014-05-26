using System;
using System.Net;
using System.Text;
using System.Web;

namespace DotNetCqs.AspNet
{
    /// <summary>
    /// Proxies ajax requests over your front end web server.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Will copy most headers from the ajax request to the request that is sent to the proxied server. You can
    /// therefore use cookies, custom headers etc.
    /// </para>
    /// <para>
    /// Uses the HTTP header <c>X-CorsProxy-Url</c> to identify which server to send the proxy request to.
    /// </para>
    /// <para>
    /// Adds the <code>X-CorsProxy-Failure</code> header to indicate wether non 2xx responses is due
    /// to this library or the destination web server.
    /// </para>
    /// </remarks>
    public class DotNetCqsHttpHandler : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler"/> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext"/> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests. </param>
        public void ProcessRequest(HttpContext context)
        {

            var url = context.Request.Headers["X-CorsProxy-Url"];
            if (url == null)
            {
                context.Response.StatusCode = 501;
                context.Response.StatusDescription =
                    "X-CorsProxy-Url was not specified. The corsproxy should only be invoked from the proxy javascript.";
                context.Response.End();
                return;
            }


          
            try
            {
                var request = WebRequest.CreateHttp(url);
                context.Request.CopyHeadersTo(request);
                request.Method = context.Request.HttpMethod;
                request.ContentType = context.Request.ContentType;
                request.UserAgent = context.Request.UserAgent;
                
                if (context.Request.AcceptTypes != null)
                request.Accept = string.Join(";", context.Request.AcceptTypes);

                if (context.Request.UrlReferrer != null)
                    request.Referer = context.Request.UrlReferrer.ToString();

                if (!context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                    context.Request.InputStream.CopyTo(request.GetRequestStream());

                //context.Request.UrlReferrer = request.Referer;
                var response = (HttpWebResponse)request.GetResponse();
                response.CopyHeadersTo(context.Response);
                context.Response.ContentType = response.ContentType;
                context.Response.StatusCode =(int) response.StatusCode;
                context.Response.StatusDescription = response.StatusDescription;

                var stream = response.GetResponseStream();
                if (stream != null && response.ContentLength > 0)
                {
                    stream.CopyTo(context.Response.OutputStream);
                    stream.Flush();
                }
                //context.Response.Close();
            }
            catch (WebException exception)
            {
                context.Response.AddHeader("X-CorsProxy-Failure",  "false");

                var response = exception.Response as HttpWebResponse;
                if (response != null)
                {
                    context.Response.StatusCode = (int)response.StatusCode;
                    context.Response.StatusDescription = response.StatusDescription;
                    response.CopyHeadersTo(context.Response);
                    var stream = response.GetResponseStream();
                    if (stream != null)
                        stream.CopyTo(context.Response.OutputStream);

                    return;
                }

                context.Response.StatusCode = 501;
                context.Response.StatusDescription = exception.Status.ToString();
                var msg = Encoding.ASCII.GetBytes(exception.Message);
                context.Response.OutputStream.Write(msg, 0, msg.Length);
                context.Response.Close();

            }
            catch (Exception exception)
            {
                context.Response.StatusCode = 501;
                context.Response.StatusDescription = "Failed to call proxied url.";
                context.Response.AddHeader("X-CorsProxy-Failure", "true");
                var msg = Encoding.ASCII.GetBytes(exception.Message);
                context.Response.OutputStream.Write(msg, 0, msg.Length);
                context.Response.Close();

            }
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler"/> instance.
        /// </summary>
        /// <returns>
        /// true if the <see cref="T:System.Web.IHttpHandler"/> instance is reusable; otherwise, false.
        /// </returns>
        public bool IsReusable { get { return true; }}
    }
}
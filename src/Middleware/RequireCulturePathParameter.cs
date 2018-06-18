using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Fiontar.Localization
{
    /// <summary>
    /// Redirects the request to a localized root path if the URL contains no culture parameter
    /// </summary>
    public class RequireCulturePathParameter
    {
        private readonly RequestDelegate Next;
        private readonly IOptions<RouteOptions> RouteOptions;
        private readonly IOptions<RouteCultureOptions> RouteCultureOptions;
        private readonly ILogger Logger;
        private readonly int StatusCode;

        /// <summary>
        /// Redirects the request to a localized root path if the URL contains no culture parameter
        /// </summary>
        public RequireCulturePathParameter(
            RequestDelegate next,
            IOptions<RouteOptions> routeOptions,
            IOptions<RouteCultureOptions> routeCultureOptions,
            ILogger<RequireCulturePathParameter> logger, 
            int statusCode)
        {
            Next = next;
            RouteOptions = routeOptions;
            RouteCultureOptions = routeCultureOptions;
            Logger = logger;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Assesses if request contains a culture parameter. If not, appends current culture name to path and redirects.
        /// </summary>
        /// <remarks>
        /// The redirect URL respects the <see cref="RouteOptions"/> setting as regards whether a trailing slash should be appeneded to the generated URL
        /// </remarks>
        public Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var parameters = request.Path.Value.Split('/');
            var culture = parameters[RouteCultureOptions.Value.CultureParameterIndex];

            if (!string.IsNullOrEmpty(culture))
            {
                return Next(context);
            }

            string newPath = "/" + CultureInfo.CurrentCulture.Name;
            newPath += (RouteOptions.Value.AppendTrailingSlash == true) ? "/" : string.Empty;
            string newUrl = UrlBuilder.ReplacePath(request, newPath);

            Logger.LogRequiredCultureRedirect(newPath);

            context.Response.Clear();
            context.Response.StatusCode = StatusCode;
            context.Response.Headers[HeaderNames.Location] = newUrl;

            return Next(context);
        }
    }
}
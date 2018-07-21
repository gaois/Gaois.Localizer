using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Gaois.Localizer
{
    /// <summary>
    /// Causes the request to be redirected to a page in the default culture when a <see cref="System.Globalization.CultureNotFoundException"/> is thrown in the request execution pipeline
    /// </summary>
    public class RequestCultureExceptionRerouter
    {
        private readonly RequestDelegate Next;
        private readonly IOptions<RouteCultureOptions> RouteCultureOptions;
        private readonly ILogger Logger;
        private readonly int StatusCode;
        private readonly string ResponsePath;

        /// <summary>
        /// Causes the request to be redirected to a page in the default culture when a <see cref="System.Globalization.CultureNotFoundException"/> is thrown in the request execution pipeline
        /// </summary>
        public RequestCultureExceptionRerouter(
            RequestDelegate next,
            IOptions<RouteCultureOptions> routeCultureOptions,
            ILogger<RequestCultureExceptionRerouter> logger)
        {
            Next = next ?? throw new CultureNotFoundException(nameof(next));
            RouteCultureOptions = routeCultureOptions;
            Logger = logger;
            StatusCode = (int)HttpStatusCode.Redirect;
        }

        /// <summary>
        /// Causes the request to be redirected to a page in the default culture when a <see cref="System.Globalization.CultureNotFoundException"/> is thrown in the request execution pipeline
        /// </summary>
        /// <param name="next">A task that represents the completion of request processing</param>
        /// <param name="routeCultureOptions">The <see cref="RouteCultureOptions"/> to configure the rerouter with</param>
        /// <param name="logger">An instance of <see cref="ILogger"/></param>
        /// <param name="options">The <see cref="RequestCultureRerouterOptions"/> to configure the middleware with</param>
        public RequestCultureExceptionRerouter(
            RequestDelegate next, 
            IOptions<RouteCultureOptions> routeCultureOptions,
            ILogger<LocalizationCookies> logger, 
            RequestCultureRerouterOptions options)
        {
            Next = next ?? throw new CultureNotFoundException(nameof(next));
            RouteCultureOptions = routeCultureOptions;
            Logger = logger;
            StatusCode = options.StatusCode;
            ResponsePath = options.ResponsePath;
        }

        /// <summary>
        /// Async method that listens for events in the request execution pipeline. Will catch <see cref="System.Globalization.CultureNotFoundException"/> and return instructions to redirect the request in the response.
        /// </summary>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await Next(context);
            }
            catch (CultureNotFoundException)
            {
                if (context.Response.HasStarted)
                {
                    throw;
                }

                var request = context.Request;
                var parts = request.Path.Value.Split('/');
                parts[RouteCultureOptions.Value.CultureParameterIndex] = CultureInfo.CurrentCulture.Name;

                string newPath = (string.IsNullOrEmpty(ResponsePath)) ? string.Join("/", parts) : ResponsePath;
                string newUrl = UrlBuilder.ReplacePath(request, newPath);

                Logger.LogRequestCultureNotFoundRedirect(newPath);

                context.Response.Clear();
                context.Response.StatusCode = StatusCode;
                context.Response.Headers[HeaderNames.Location] = newUrl;

                return;
            }
        } 
    }
}
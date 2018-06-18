using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Fiontar.Localization
{
    /// <summary>
    /// Causes a 404 Not Found HTTP status code to returned when a <see cref="System.Globalization.CultureNotFoundException"/> is thrown in the request execution pipeline
    /// </summary>
    public class RequestCultureExceptionHandler
    {
        private readonly RequestDelegate Next;
        private readonly ILogger Logger;

        /// <summary>
        /// Causes a 404 Not Found HTTP status code to returned when a <see cref="System.Globalization.CultureNotFoundException"/> is thrown in the request execution pipeline
        /// </summary>
        public RequestCultureExceptionHandler(RequestDelegate next, ILogger<RequestCultureExceptionHandler> logger)
        {
            Next = next ?? throw new CultureNotFoundException(nameof(next));
            Logger = logger;
        }

        /// <summary>
        /// Async method that listens for events in the request execution pipeline. Will catch <see cref="System.Globalization.CultureNotFoundException"/> and return a 404 Not Found HTTP status code in the response.
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

                Logger.LogRequestCultureNotFound(context.Request.Path);

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;

                return;
            }
        }
    }
}
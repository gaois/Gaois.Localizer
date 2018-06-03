using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Fiontar.Localization
{
    public class RequestCultureNotFoundExceptionPage
    {
        private readonly RequestDelegate Next;

        public RequestCultureNotFoundExceptionPage(RequestDelegate next)
        {
            Next = next ?? throw new CultureNotFoundException(nameof(next));
        }

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

                context.Response.Clear();
                context.Response.StatusCode = 404;

                return;
            }
        }
    }
}
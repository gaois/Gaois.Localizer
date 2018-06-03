using Microsoft.AspNetCore.Builder;

namespace Fiontar.Localization
{
    public static class RequestCultureNotFoundExceptionPageExtensions
    {
        public static IApplicationBuilder UseRequestCultureNotFoundExceptionPage(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestCultureNotFoundExceptionPage>();
        }
    }
}
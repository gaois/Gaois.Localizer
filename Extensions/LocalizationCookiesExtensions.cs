using Microsoft.AspNetCore.Builder;

namespace Fiontar.Localization
{
    public static class LocalizationCookiesExtensions
    {
        public static IApplicationBuilder UseLocalizationCookies(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LocalizationCookies>();
        }
    }
}
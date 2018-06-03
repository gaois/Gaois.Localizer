using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Fiontar.Localization
{
    public static class RequestCultureValidationExtensions
    {
        public static IApplicationBuilder UseRequestCultureValidation(
            this IApplicationBuilder builder, 
            IOptions<RequestLocalizationOptions> localizationOptions = null,
            IOptions<RouteCultureOptions> routeCultureOptions = null)
        {
            return builder.UseMiddleware<RequestCultureValidation>();
        }
    }
}
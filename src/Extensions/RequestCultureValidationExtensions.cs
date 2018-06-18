using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Fiontar.Localization
{
    /// <summary>
    /// Extension method to validate request culture support
    /// </summary>
    public static class RequestCultureValidationExtensions
    {
        /// <summary>
        /// Adds middleware to validate that the culture provided in the HTTP request is a supported culture
        /// </summary>
        public static IApplicationBuilder UseRequestCultureValidation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestCultureValidation>();
        }
    }
}
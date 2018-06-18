using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Fiontar.Localization
{
    /// <summary>
    /// Verifies that the request culture is supported by the application
    /// </summary>
    public class RequestCultureValidation
    {
        private readonly RequestDelegate Next;
        private readonly IOptions<RequestLocalizationOptions> LocalizationOptions;
        private readonly IOptions<RouteCultureOptions> RouteCultureOptions;
    
        /// <summary>
        /// Verifies that the request culture is supported by the application
        /// </summary>
        public RequestCultureValidation(
            RequestDelegate next, 
            IOptions<RequestLocalizationOptions> localizationOptions, 
            IOptions<RouteCultureOptions> routeCultureOptions)
        {
            Next = next;
            LocalizationOptions = localizationOptions;
            RouteCultureOptions = routeCultureOptions;
        }

        /// <summary>
        /// Tests the request culture against supported application cultures and non-culture values. Throws a <see cref="System.Globalization.CultureNotFoundException"/> if request culture is not supported.
        /// </summary>
        public Task Invoke(HttpContext context)
        {
            var parameters = context.Request.Path.Value.Split('/');
            var culture = parameters[RouteCultureOptions.Value.CultureParameterIndex];
            var nonCultures = RouteCultureOptions.Value.NonCultures ?? new List<string>();

            nonCultures = AddDefaultNonCultures(nonCultures);

            if (nonCultures.Count > 0 && nonCultures.Contains(culture))
            {
                return Next(context);
            }
            else if (IsSupportedCulture(culture))
            {
                return Next(context);
            }
            else
            {
                throw new CultureNotFoundException();
            }
        }

        /// <summary>
        /// Protects against circular culture exception errors when an error view is called
        /// </summary>
        private IList<string> AddDefaultNonCultures(IList<string> nonCultures)
        {
            nonCultures.Add("Error");
            return nonCultures;
        }

        /// <summary>
        /// Tests that a given request culture is supported by the application
        /// </summary>
        /// <param name="requestCulture">The culture provided in the request</param>
        public bool IsSupportedCulture(string requestCulture)
        {
            if (string.IsNullOrEmpty(requestCulture))
            {
                return true;
            }
            
            var culture = new CultureSupportValidation(LocalizationOptions);
            return culture.IsSupportedCulture(requestCulture);
        }
    }
}
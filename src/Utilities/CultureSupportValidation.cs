using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Gaois.Localizer
{
    /// <summary>
    /// Provides methods to validate that a given request culture is supported by the application
    /// </summary>
    public class CultureSupportValidation
    {
        private readonly IOptions<RequestLocalizationOptions> LocalizationOptions;

        /// <summary>
        /// Provides methods to validate that a given request culture is supported by the application
        /// </summary>
        /// <param name="options">The <see cref="RequestLocalizationOptions"/> that define the supported cultures</param>
        public CultureSupportValidation(IOptions<RequestLocalizationOptions> options)
        {  
            LocalizationOptions = options;
        }

        /// <summary>
        /// Validates that a given request culture is supported by the application
        /// </summary>
        /// <param name="requestCulture">The culture provided in the request</param>
        public bool IsSupportedCulture(string requestCulture)
        {
            List<string> supportedCultures = new List<string>();

            foreach (var culture in LocalizationOptions.Value.SupportedUICultures)
            {
                supportedCultures.Add(culture.Name);
            }

            return (supportedCultures.Contains(requestCulture)) ? true : false;
        }
    }
}
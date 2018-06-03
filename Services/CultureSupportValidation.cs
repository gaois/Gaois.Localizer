using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Fiontar.Localization
{
    public class CultureSupportValidation
    {
        private readonly IOptions<RequestLocalizationOptions> LocalizationOptions;

        public CultureSupportValidation(IOptions<RequestLocalizationOptions> options)
        {  
            LocalizationOptions = options;
        }

        public bool IsSupportedCulture(string lang)
        {
            List<string> supportedCultures = new List<string>();

            foreach (var culture in LocalizationOptions.Value.SupportedUICultures)
            {
                supportedCultures.Add(culture.Name);
            }

            return (supportedCultures.Contains(lang)) ? true : false;
        }
    }
}
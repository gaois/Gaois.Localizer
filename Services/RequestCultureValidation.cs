using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Fiontar.Localization
{
    public class RequestCultureValidation
    {
        private readonly RequestDelegate Next;
        private readonly IOptions<RequestLocalizationOptions> LocalizationOptions;
        private readonly IOptions<RouteCultureOptions> RouteCultureOptions;
    
        public RequestCultureValidation(
            RequestDelegate next,
            IOptions<RequestLocalizationOptions> localizationOptions = null,
            IOptions<RouteCultureOptions> routeCultureOptions = null)
        {
            Next = next;
            LocalizationOptions = localizationOptions;
            RouteCultureOptions = routeCultureOptions;
        }

        public Task Invoke(HttpContext context)
        {
            var parameters = context.Request.Path.Value.Split('/');
            var culture = parameters[1];
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

        private IList<string> AddDefaultNonCultures(IList<string> nonCultures)
        {
            // Protect against circular culture exception errors when error view is called
            nonCultures.Add("Error");
            return nonCultures;
        }

        public bool IsSupportedCulture(string language)
        {
            if (string.IsNullOrEmpty(language))
            {
                return true;
            }
            
            var culture = new CultureSupportValidation(LocalizationOptions);
            return culture.IsSupportedCulture(language);
        }
    }
}
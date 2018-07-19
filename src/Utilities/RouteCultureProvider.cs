using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Gaois.Localizer
{
    /// <summary>
    /// Determines the request's culture information
    /// </summary>
    /// <remarks>
    /// The culture is determined with reference to the following criteria, in order: (1) the culture parameter in URL, (2) the culture request cookie, (3) the HTTP Accept-Language header.
    /// The first criterion to be met will be returned. If no criteria are met, the default culture is returned.
    /// </remarks>
    public class RouteCultureProvider : IRequestCultureProvider
    {
        private readonly CultureInfo DefaultCulture;
        private readonly CultureInfo DefaultUICulture;
        private readonly int CultureParameterIndex;
        
        private readonly IList<CultureInfo>  SupportedCultures;

        /// <summary>
        /// Determines the request's culture information
        /// </summary>
        /// <param name="supportedCultures">The cultures supported by the application</param>
        /// <param name="requestCulture">The default culture to use for requests when a supported culture could not be determined by one of the configured <see cref="Microsoft.AspNetCore.Localization.IRequestCultureProvider"/>s. Defaults to <see cref="System.Globalization.CultureInfo.CurrentCulture"/> and <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>.</param>
        /// <param name="cultureParameterIndex">Index of the request path parameter that represents the desired culture. The default value is 1.</param>
        /// <remarks>
        /// The culture is determined with reference to the following criteria, in order: (1) the culture parameter in URL, (2) the culture request cookie, (3) the HTTP Accept-Language header.
        /// The first criterion to be met will be returned. If no criteria are met, the default culture is returned.
        /// </remarks>
        public RouteCultureProvider(
            IList<CultureInfo> supportedCultures, 
            RequestCulture requestCulture, 
            int cultureParameterIndex = 1)
        {
            SupportedCultures = supportedCultures;
            DefaultCulture = requestCulture.Culture;
            DefaultUICulture = requestCulture.UICulture;
            CultureParameterIndex = cultureParameterIndex;
        }

        /// <summary>
        /// Assesses if request culture is provided in URL. If not, culture is inferred from HTTP headers and request cookies. If no culture can be inferred, the default culture is selected. 
        /// </summary>
        public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext context)
        {
            PathString url = context.Request.Path;

            // If no culture provided in URL, infer from HTTP headers & cookies, or else fall back to default culture
            if (url.ToString().Length <= 1)
            {
                // Establish default culture, it will be updated if subsequent criteria are met
                string locale = DefaultCulture.Name;

                // If culture is present in HTTP Accept-Language header
                var acceptLanguages = context.Request.Headers["Accept-Language"].ToString().Split(',');

                foreach (var supportedCulture in SupportedCultures)
                {
                    if (acceptLanguages.Contains(supportedCulture.Name)
                        || acceptLanguages.Contains(supportedCulture.TwoLetterISOLanguageName)
                        || acceptLanguages.Contains(supportedCulture.ThreeLetterISOLanguageName))
                    {
                        locale = supportedCulture.Name;
                        break;
                    }
                }

                // If culture is present in request cookies
                string cookie = context.Request.Cookies[".AspNetCore.Culture"];

                if (!string.IsNullOrEmpty(cookie))
                {
                    string cultureCookie = CookieCultureProvider.GetCultureFromCookie(cookie);

                    foreach (var supportedCulture in SupportedCultures)
                    {
                        if (cultureCookie == supportedCulture.Name
                            || cultureCookie == supportedCulture.TwoLetterISOLanguageName
                            || cultureCookie == supportedCulture.ThreeLetterISOLanguageName)
                        {
                            locale = supportedCulture.Name;
                            break;
                        }
                    }
                }

                return Task.FromResult<ProviderCultureResult>(new ProviderCultureResult(locale, locale));
            }

            var parameters = context.Request.Path.Value.Split('/');
            var culture = parameters[CultureParameterIndex];

            // If culture is not formatted correctly, return default culture
            if (!Regex.IsMatch(culture, @"^[a-z]{2}(-[A-Z]{2})*$"))
            {
                return Task.FromResult<ProviderCultureResult>(new ProviderCultureResult(DefaultCulture.Name, DefaultUICulture.Name));
            }

            // Otherwise, return Culture and UICulture from route culture parameter
            return Task.FromResult<ProviderCultureResult>(new ProviderCultureResult(culture, culture));
        }
    }
}
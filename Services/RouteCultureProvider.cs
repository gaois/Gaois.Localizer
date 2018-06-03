using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Fiontar.Localization
{
    public class RouteCultureProvider : IRequestCultureProvider
    {
        private CultureInfo DefaultCulture;
        private CultureInfo DefaultUICulture;
        
        private readonly IList<CultureInfo>  SupportedCultures;

        public RouteCultureProvider(IList<CultureInfo> supportedCultures, RequestCulture requestCulture)
        {
            SupportedCultures = supportedCultures;
            DefaultCulture = requestCulture.Culture;
            DefaultUICulture = requestCulture.UICulture;
        }

        public Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext context)
        {
            PathString url = context.Request.Path;

            // If no culture provided, estimate from HTTP headers & cookies, or else fall back to default
            if (url.ToString().Length <= 1)
            {
                // Establish default culture, updated if subsequent criteria are met
                string locale = DefaultCulture.Name;

                // If culture is present in HTTP Accept-Language header
                var acceptLanguages = context.Request.Headers["Accept-Language"].ToString().Split(',');

                foreach (var supportedCulture in SupportedCultures)
                {
                    if (acceptLanguages.Contains(supportedCulture.Name))
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
                        if (cultureCookie == supportedCulture.Name)
                        {
                            locale = supportedCulture.Name;
                            break;
                        }
                    }
                }

                return Task.FromResult<ProviderCultureResult>(new ProviderCultureResult(locale, locale));
            }

            var parameters = context.Request.Path.Value.Split('/');
            var culture = parameters[1];

            // If culture is not formatted correctly, return default
            if (!Regex.IsMatch(culture, @"^[a-z]{2}(-[A-Z]{2})*$"))
            {
                return Task.FromResult<ProviderCultureResult>(new ProviderCultureResult(DefaultCulture.Name, DefaultUICulture.Name));
            }

            // Otherwise, return Culture and UICulture from route culture parameter
            return Task.FromResult<ProviderCultureResult>(new ProviderCultureResult(culture, culture));
        }
    }
}
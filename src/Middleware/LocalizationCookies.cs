using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gaois.Localizer
{
    /// <summary>
    /// Stores current request culture in a user cookie 
    /// </summary>
    public class LocalizationCookies
    {
        private readonly RequestDelegate Next;
        private readonly IOptions<RouteCultureOptions> RouteCultureOptions;
        private readonly ILogger Logger;

        private readonly DateTimeOffset? CookieExpires;
        private readonly bool CookieIsEssential;

        /// <summary>
        /// Stores current request culture in a user cookie
        /// </summary>
        public LocalizationCookies(
            RequestDelegate next, 
            IOptions<RouteCultureOptions> routeCultureOptions,
            ILogger<LocalizationCookies> logger)
        {
            Next = next;
            RouteCultureOptions = routeCultureOptions;
            Logger = logger;
            CookieExpires = DateTimeOffset.UtcNow.AddYears(1);
            CookieIsEssential = false;
        }

        /// <summary>
        /// Stores current request culture in a user cookie
        /// </summary>
        public LocalizationCookies(
            RequestDelegate next,
            IOptions<RouteCultureOptions> routeCultureOptions,
            ILogger<LocalizationCookies> logger,
            LocalizationCookiesOptions options)
        {
            Next = next;
            RouteCultureOptions = routeCultureOptions;
            Logger = logger;
            CookieExpires = options.Expires;
            CookieIsEssential = options.IsEssential;
        }

        /// <summary>
        /// Appends a culture cookie to the HTTP response, when appropriate, to store the client's culture preference
        /// </summary>
        /// <remarks>
        /// If the current request does not already contain a culture cookie, a new one will be appended to the response. 
        /// If the request contains a culture cookie but it is different from the current culture, a new cookie will be appended.
        /// </remarks>
        public Task Invoke(HttpContext context)
        {
            var request = context.Request;
            var path = request.Path;
            var excludedRoutes = RouteCultureOptions.Value.ExcludedRoutes ?? new List<string>();

            if (ExcludedRouteProvider.IsExcludedRoute(excludedRoutes, path))
            {
                return Next(context);
            }

            string locale = CultureInfo.CurrentCulture.Name;
            string cookie = CookieRequestCultureProvider.DefaultCookieName;

            if (!string.IsNullOrEmpty(cookie))
            {
                string cookieCulture = CookieCultureProvider.GetCultureFromCookie(cookie);

                if (cookieCulture == locale)
                {
                    return Next(context);
                }
            }

            var response = context.Response;
            
            response.Cookies.Append(
                cookie, 
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture: locale, uiCulture: locale)),
                new CookieOptions { Expires = CookieExpires, IsEssential = CookieIsEssential }
            );
            
            Logger.LogLocalizationCookieAppended(request.Path);
            
            return Next(context);
        }
    }
}
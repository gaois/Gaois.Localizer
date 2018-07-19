using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Logging;

namespace Gaois.Localizer
{
    /// <summary>
    /// Stores current request culture in a user cookie 
    /// </summary>
    public class LocalizationCookies
    {
        private readonly RequestDelegate Next;
        private readonly ILogger Logger;
        private readonly DateTimeOffset? CookieExpires;
        private readonly bool CookieIsEssential;

        /// <summary>
        /// Stores current request culture in a user cookie
        /// </summary>
        public LocalizationCookies(RequestDelegate next, ILogger<LocalizationCookies> logger)
        {
            Next = next;
            Logger = logger;
            CookieExpires = DateTimeOffset.UtcNow.AddYears(1);
            CookieIsEssential = false;
        }

        /// <summary>
        /// Stores current request culture in a user cookie
        /// </summary>
        public LocalizationCookies(RequestDelegate next, ILogger<LocalizationCookies> logger, LocalizationCookiesOptions options)
        {
            Next = next;
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
            string locale = CultureInfo.CurrentCulture.Name;
            string cookie = request.Cookies[".AspNetCore.Culture"];

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
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture: locale, uiCulture: locale)),
                new CookieOptions { Expires = CookieExpires, IsEssential = CookieIsEssential }
            );
            
            Logger.LogLocalizationCookieAppended(request.Path);
            
            return Next(context);
        }
    }
}
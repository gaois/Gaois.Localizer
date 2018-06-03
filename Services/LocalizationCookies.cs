using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Fiontar.Localization
{
    public class LocalizationCookies
    {
        private readonly RequestDelegate Next;
    
        public LocalizationCookies(RequestDelegate next)
        {
            Next = next;
        }

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
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = false }
            );

		    return Next(context);
        }
    }
}
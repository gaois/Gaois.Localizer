using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Fiontar.Localization
{
    /// <summary>
    /// Redirects a request containing a two- or three-letter ISO language code to a URL containing another supported culture
    /// </summary>
    /// <remarks>
    /// Helpful if you wish to migrate your application to a request path scheme that uses a different language or locale format
    /// </remarks>
    public class RedirectLanguageToLocale : IRule
    {
        private readonly IOptions<RouteCultureOptions> RouteCultureOptions;
        private readonly IDictionary<string, string> LanguageLocaleMap;
        private readonly int StatusCode;


        /// <summary>
        /// Redirects a request containing a two- or three-letter ISO language code to a URL containing another supported culture
        /// </summary>
        /// <param name="routeCultureOptions">The <see cref="RouteCultureOptions"/> to configure the redirect rule with</param>
        /// <param name="statusCode">The HTTP status code to return in the response. The default is a 302 <see cref="HttpStatusCode.Redirect"/> code.</param>
        public RedirectLanguageToLocale(
            IOptions<RouteCultureOptions> routeCultureOptions, 
            int statusCode = (int)HttpStatusCode.Redirect)
        {
            RouteCultureOptions = routeCultureOptions;
            LanguageLocaleMap = routeCultureOptions.Value.LanguageLocaleMap;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Checks that the <see cref="LanguageLocaleMap"/> contains the current request culture. If yes, redirects to a URL containing the matching locale.
        /// </summary>
        /// <param name="context">A <see cref="Microsoft.AspNetCore.Rewrite.RewriteContext"/> context</param>
        public void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;
            var parts = request.Path.Value.Split('/');
            var culture = parts[RouteCultureOptions.Value.CultureParameterIndex];

            if (!LanguageLocaleMap.ContainsKey(culture))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            foreach (var map in LanguageLocaleMap)
            {
                if (map.Key == culture)
                {
                    parts[RouteCultureOptions.Value.CultureParameterIndex] = map.Value;
                    break;
                }
            }

            string newPath = string.Join("/", parts);
            string newUrl = UrlBuilder.ReplacePath(request, newPath);

            var response = context.HttpContext.Response;
            response.StatusCode = StatusCode;
            response.Headers[HeaderNames.Location] = newUrl;
            context.Result = RuleResult.EndResponse;  
        }
    }
}
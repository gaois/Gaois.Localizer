using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;

namespace Fiontar.Localization
{
    public class RedirectLanguageToLocale : IRule
    {
        public readonly IDictionary<string, string> LanguageLocaleMap;
        public int StatusCode { get; set; }

        public RedirectLanguageToLocale(IDictionary<string, string> languageLocaleMap, int? statusCode = null)
        {
            LanguageLocaleMap = languageLocaleMap;
            StatusCode = statusCode ?? (int)HttpStatusCode.Moved;
        }

        public void ApplyRule(RewriteContext context)
        {
            var request = context.HttpContext.Request;
            var parts = request.Path.Value.Split('/');
            var culture = parts[1];

            if (!LanguageLocaleMap.ContainsKey(culture))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            foreach (var map in LanguageLocaleMap)
            {
                if (map.Key == culture)
                {
                    parts[1] = map.Value;
                    break;
                }
            }

            string newPath = string.Join("/", parts);
            string newUrl = request.Scheme + "://" + request.Host + request.PathBase + newPath + request.QueryString;

            var response = context.HttpContext.Response;
            response.StatusCode = StatusCode;
            response.Headers[HeaderNames.Location] = newUrl;
            context.Result = RuleResult.EndResponse;  
        }
    }
}
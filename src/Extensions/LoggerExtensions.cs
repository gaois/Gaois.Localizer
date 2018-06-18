
using System;
using Microsoft.Extensions.Logging;

namespace Fiontar.Localization
{
    internal static class LoggerExtensions
    {
        private static Action<ILogger, string, Exception> logRequestCultureNotFound = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1000,
            formatString: "Request culture at '{Path}' is not supported by the application. 404 HTTP status code returned."
        );

        private static Action<ILogger, string, Exception> logRequestCultureNotFoundRedirect = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1001,
            formatString: "Request culture is not supported by the application. Redirecting to '{Path}'."
        );

        private static Action<ILogger, string, Exception> logRequiredCultureRedirect = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1002,
            formatString: "No request culture parameter found. Redirecting to '{Path}'."
        );

        private static Action<ILogger, string, Exception> logLocalizationCookieAppended = LoggerMessage.Define<string>(
            logLevel: LogLevel.Information,
            eventId: 1003,
            formatString: "Localization cookie appended for '{Path}'"
        );

        public static void LogLocalizationCookieAppended(this ILogger logger, string path) => 
            logLocalizationCookieAppended(logger, path, null);

        public static void LogRequestCultureNotFound(this ILogger logger, string path) => 
            logRequestCultureNotFound(logger, path, null);

        public static void LogRequestCultureNotFoundRedirect(this ILogger logger, string path) => 
            logRequestCultureNotFoundRedirect(logger, path, null);

        public static void LogRequiredCultureRedirect(this ILogger logger, string path) => 
            logRequiredCultureRedirect(logger, path, null);
    }
}
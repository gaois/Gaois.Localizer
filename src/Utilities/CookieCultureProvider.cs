namespace Fiontar.Localization
{
    /// <summary>
    /// Provides methods to extract culture values from a <see cref="Microsoft.AspNetCore.Localization.CookieRequestCultureProvider"/> cookie
    /// </summary>
    public static class CookieCultureProvider
    {
        /// <summary>
        /// Extracts Culture value from a cookie
        /// </summary>
        /// <param name="cookie">
        /// A default <see cref="Microsoft.AspNetCore.Localization.CookieRequestCultureProvider"/> (.AspNetCore.Culture) cookie
        /// </param>
        public static string GetCultureFromCookie(string cookie)
        {
            string[] parts = cookie.Split('|');
            return parts[0].Replace("c=", string.Empty);
        }

        /// <summary>
        /// Extracts UICulture value from a cookie
        /// </summary>
        /// <param name="cookie">
        /// A default <see cref="Microsoft.AspNetCore.Localization.CookieRequestCultureProvider"/> (.AspNetCore.Culture) cookie
        /// </param>
        public static string GetUICultureFromCookie(string cookie)
        {
            string[] parts = cookie.Split('|');
            return parts[1].Replace("uic=", string.Empty);
        }
    }
}
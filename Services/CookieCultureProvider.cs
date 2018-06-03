namespace Fiontar.Localization
{
    public static class CookieCultureProvider
    {
        public static string GetCultureFromCookie(string cookie)
        {
            string[] parts = cookie.Split('|');
            return parts[0].Replace("c=", string.Empty).Replace("uic=", string.Empty);
        }
    }
}
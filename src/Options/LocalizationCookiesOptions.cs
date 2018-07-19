using System;

namespace Gaois.Localizer
{
    /// <summary>
    ///  Specifies options for the <see cref="LocalizationCookies"/> middleware
    /// </summary>
    public class LocalizationCookiesOptions
    {
        /// <summary>
        /// The expiration date and time for the cookie. The default value is one year.
        /// </summary>
        public DateTimeOffset? Expires = DateTimeOffset.UtcNow.AddYears(1);
        /// <summary>
        /// Specifies if this cookie is essential for the application to function correctly. If true then consent policy checks may be bypassed. The default value is false.
        /// </summary>
        public bool IsEssential = false;
    }
}
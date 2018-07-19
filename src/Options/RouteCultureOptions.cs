using System.Collections.Generic;

namespace Gaois.Localizer
{
    /// <summary>
    ///  Specifies options for various middleware provided by Gaois.Localizer
    /// </summary>
    public class RouteCultureOptions
    {
        /// <summary>
        /// Index of the request path parameter that represents the desired culture. The default value is 1.
        /// </summary>
        public int CultureParameterIndex = 1;
        /// <summary>
        /// A list of strings that are not involved in localization but are allowed to be used in place of request culture values
        /// </summary>
        /// <remarks>
        /// Example values: "api", "static".
        /// If you are using RequestCultureValidation this will avoid throwing a <see cref="System.Globalization.CultureNotFoundException"/> when these strings occupy the route parameter normally reserved for the request culture, e.g. "/api/v1.2/documents/"
        /// </remarks>
        public IList<string> NonCultures { get; set; }
        /// <summary>
        /// A collection of keys and values where the keys are presumed to be two- or three-letter ISO language codes and the values are locale or extended language subtags
        /// </summary>
        public IDictionary<string, string> LanguageLocaleMap { get; set; }

        /// <summary>
        /// Specifies options for various middleware provided by Gaois.Localizer
        /// </summary>
        public RouteCultureOptions()
        {
            NonCultures = new List<string>();
            LanguageLocaleMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// Get the value of a locale or extended language subtag based on the values provided in the <see cref="LanguageLocaleMap"/>
        /// </summary>
        /// <param name="code">The two- or three-letter language code</param>
        public string InferLocaleFromLanguage(string code)
        {
            if (LanguageLocaleMap.ContainsKey(code))
            {
                return LanguageLocaleMap[code];
            }

            return code;
        }
    }
}
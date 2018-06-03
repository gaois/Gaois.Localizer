using System.Collections.Generic;

namespace Fiontar.Localization
{
    public class RouteCultureOptions
    {
        public IList<string> NonCultures { get; set; }
        public IDictionary<string, string> LanguageLocaleMap { get; set; }

        public RouteCultureOptions()
        {
            NonCultures = new List<string>();
            LanguageLocaleMap = new Dictionary<string, string>();
        }

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
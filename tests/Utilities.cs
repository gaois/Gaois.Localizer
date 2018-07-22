using System;
using Xunit;

namespace Gaois.Localizer.Tests
{
    public class Utilities
    {
        [Fact]
        public void GetsCultureFromCookie()
        {
            string cookie = "c=ga-IE|uic=ga-IE";
            string culture = CookieCultureProvider.GetCultureFromCookie(cookie);
            Assert.Equal("ga-IE", culture);
        }

        [Fact]
        public void GetsUICultureFromCookie()
        {
            string cookie = "c=ga-IE|uic=ga-IE";
            string uiCulture = CookieCultureProvider.GetUICultureFromCookie(cookie);
            Assert.Equal("ga-IE", uiCulture);
        }
    }
}

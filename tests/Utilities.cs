using Xunit;

namespace Gaois.Localizer.Tests
{
    public class Utilities
    {
        [Fact]
        public void GetsCultureFromCookie()
        {
            var cookie = "c=ga-IE|uic=ga-IE";
            var culture = CookieCultureProvider.GetCultureFromCookie(cookie);
            Assert.Equal("ga-IE", culture);
        }

        [Fact]
        public void GetsUICultureFromCookie()
        {
            var cookie = "c=ga-IE|uic=ga-IE";
            var uiCulture = CookieCultureProvider.GetUICultureFromCookie(cookie);
            Assert.Equal("ga-IE", uiCulture);
        }
    }
}
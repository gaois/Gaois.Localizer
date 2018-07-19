using System.Net;

namespace Gaois.Localizer
{
    /// <summary>
    ///  Specifies options for the <see cref="RequestCultureExceptionRerouter"/> middleware
    /// </summary>
    public class RequestCultureRerouterOptions
    {
        /// <summary>
        /// The HTTP status code to return in the response. The default is a 302 <see cref="HttpStatusCode.Redirect"/> code
        /// </summary>
        public int StatusCode = (int)HttpStatusCode.Redirect;
        /// <summary>
        /// Specifies a relative path for the redirect URL
        /// </summary>
        public string ResponsePath { get; set; }
    }
}
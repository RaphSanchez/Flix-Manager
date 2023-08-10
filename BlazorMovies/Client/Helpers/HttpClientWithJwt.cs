
namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// This class represents a typed HttpClient that is configured as a
    /// service in the dependency injection container of the Application/Client
    /// project to supply HttpClient instances that include authorization tokens
    /// (JWTs) when making requests to the Application/Server-Api. This allows
    /// the IdentityServer engine to validate that the Http request originates
    /// from an authenticated user.
    /// <para>
    /// HttpClient instances can be configured by name so you can refer to
    /// them by name or by type. By type means that this class represents
    /// an instance of the HttpClient. A typed HttpClient can handle all of
    /// the Http and token acquisition concerns within a single class; i.e.,
    /// the class can wrap multiple HttpClient configurations.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The additional HttpClient configuration is simply a read-only property
    /// member: <see cref="HttpClient"/> that is instantiated with a built in
    /// <see cref="System.Net.Http.HttpClient"/> instance that is constructor
    /// injected. The built-in <see cref="System.Net.Http.HttpClient"/> is
    /// needed to have access to its public interface; e.g., to be able to
    /// access it <see cref="HttpClient.BaseAddress"/> needed by the
    /// Application/Client/ApiServices/ApiManager ApiConnector class to build
    /// the URI for the requested endpoint.
    /// <para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-6.0#attach-tokens-to-outgoing-requests">
    /// Attach tokens to outgoing requests</see>,
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-6.0#typed-httpclient">
    /// Typed HttpClient
    /// </see>, 
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&amp;tabs=visual-studio#httpclient-configuration">
    /// HttpClient configuration</see>, and Udemy Programming in Blazor - ASP.Net Core 5
    /// <see href="https://www.udemy.com/share/102l0i3@TPUc0N__wS5oMwN-ULpB7Kz6_upTi0lfVWed0gZsH-w4EA6Y4ez9kMK1KkSLuQlS/">
    /// episode 102</see> Sending the JWT through the HttpClient.
    /// </para>
    /// </remarks>
    public class HttpClientWithJwt
    {
        /// <summary>
        /// The consumer of the <see cref="HttpClientWithJwt"/> can have access
        /// to the built in <typeparamref name="HttpClient"/>. This provides
        /// access to its public interface and all its members; e.g., properties
        /// and methods.
        /// </summary>
        public HttpClient HttpClient { get; }

        public HttpClientWithJwt(HttpClient httpClient)
        {
            /// Initializes the HttpClient property with an injected instance
            /// of the built-in HttpClient.
            HttpClient = httpClient;

            /// Http headers let the client and the server pass additional
            /// information with an HTTP request or response. E.g., "request
            /// headers" contain additional info about the resource to be 
            /// fetched, "response headers" contain additional information 
            /// about the response such as its location or the server
            /// providing it, and "representation headers" describe the 
            /// particular representation (or format) of the resource sent
            /// in an HTTP message body. 
            /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Clear();

            /// Content Negotiation with an Accept Header:
            /// Representations in a representation header are different
            /// forms of a particular resource. The data might be formatted
            /// as a particular "media type" such as XML, text, or JSON.
            /// 
            /// The "media type" formerly "MIME type" and sometimes called
            /// "content type" is a string sent along with the response to
            /// describe the content format. It serves the same purpose as
            /// filename extensions on Windows. The "media type" is a two
            /// part identifier for file format and content format 
            /// transmitted on the internet. 
            /// 
            /// The MediaTypeWithQualityHeaderValue class represents a 
            /// media type with an additional quality factor used in a
            /// Content-Type header. It is used here to add the
            /// application/json media type format to the
            /// HttpHeaderValueCollection. 
            /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.headers.mediatypewithqualityheadervalue?view=net-6.0
            /// https://stackoverflow.com/questions/30649347/setting-accept-header-without-using-mediatypewithqualityheadervalue
            /// https://restfulapi.net/
            /// https://restfulapi.net/introduction-to-json/
            /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.headers?redirectedfrom=MSDN&view=net-6.0
            /// https://docs.microsoft.com/en-us/dotnet/csharp/tutorials/console-webapiclient#make-http-requests
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers
                .MediaTypeWithQualityHeaderValue("application/json"));
        }
    }
}



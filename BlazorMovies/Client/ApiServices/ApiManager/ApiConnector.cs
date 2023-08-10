using System.Net.Http.Json;
using System.Text;

using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.Helpers;

/// <summary>
/// In the context of a REST API a "resource" is an abstraction 
/// of information. Any information that can be named is a resource. 
/// For example, a REST resource can be a document, an image, a
/// temporal service, a collection of other resources, or a non-
/// virtual object such as a Person or a Genre data entity. Data
/// and functionality are considered resources and are accessed
/// using Uniform Resource Identifiers (URIs).
/// 
/// The clients and servers exchange representations of resources
/// by using a standarized interface (API) and a protocol (HTTP).
/// The resources have to be decoupled from their representation
/// so that clients can access the content in various formats such
/// as HTML, XML, plain text, PDF, JPEG, JSON, and others.
/// 
/// The state of the resource at any particular time is known as the
/// "resource representation". A resource representation consists of
/// 1. The data.
/// 2. The metadata describing the data to control caching, detect
///     transmission errors, negotiate appropriate representation
///     format, and perform authentication or access control.
/// 3. The hypermedia links that can help the clients transition to
///     the next desired state.
/// 
/// The data format of a representation is known as a "media type"
/// which identifies a specification that defines how a representation
/// is to be processed and is defined in the constructor of this
/// ApiConnector class.
/// 
/// An important building block of a Representational State Transfer
/// (REST) service is "resource methods". Resource methods are used
/// to perform the desired transition between two states of any
/// resource. This ApiConnector class defines the generic methods that
/// encapsulate the required code logic for the API resource methods
/// (.Net's JSON helpers) that build the Http requests/responses and
/// serialize/deserialize .Net objects to JSON format so they can travel
/// through the internet to the Application/Server-Api/Controllers and
/// back. Resource methods are not Http action methods.
/// https://restfulapi.net/
/// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-6.0
/// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview?pivots=dotnet-6-0
/// https://docs.microsoft.com/en-us/dotnet/api/system.text.json?view=net-6.0
/// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.headers.mediatypewithqualityheadervalue?view=net-6.0
/// https://stackoverflow.com/questions/30649347/setting-accept-header-without-using-mediatypewithqualityheadervalue
/// https://www.youtube.com/watch?v=N6JBjzPssQI
/// https://docs.microsoft.com/en-us/previous-versions/windows/apps/hh781239(v=win.10)?redirectedfrom=MSDN
/// 
/// A REST API consists of an assembly of interlinked resources. This
/// set of resources is known as the REST API's "resource model". 
/// </summary>
namespace BlazorMovies.Client.ApiServices.ApiManager
{
    /// <summary>
    /// The ApiConnector class implements the IApiConnector interface
    /// which defines the protocol for functionality required by the
    /// IApiService  (available to the client) to connect to the 
    /// Application/Server-Api/Controllers (unavailable to the client)
    /// to send/receive Http requests/responses.
    /// </summary>
    /// <remarks>
    /// In other words, it defines the methods that encapsulate the
    /// required code logic for the "resource methods" responsible for
    /// building the Http requests/responses and serializing/deserializing 
    /// .Net objects to JSON format so they can travel through the 
    /// internet to the
    /// Application/Server-Api/Controllers and back. 
    /// <para>
    /// After receiving and interpreting a request message, a 
    /// server responds with an Http response as plain text in
    /// either JSON or XML format, just like Http requests.
    ///</para>
    /// <para>
    /// The ApiConnector class also defines the data format of a 
    /// representation; i.e., it defines the "media type" which 
    /// identifies a specification that defines how a representation is
    /// to be processed. For example: "application/json". 
    /// </para>
    /// <para>
    /// The generic methods defined here can send/receive Http requests/
    /// responses for any type and can receive any Http response type;
    /// e.g., 
    /// Task<typeparam name="Genre">&lt;Genre&gt;</typeparam> or even 
    /// Task<typeparam name="IEnumerable">&lt;IEnumerable
    /// <typeparam name="Genre">&lt;Genre&gt;&gt;</typeparam></typeparam>
    /// </para>
    /// <para>   
    /// HttpClient is intended to be instantiated once and re-used
    /// throughout the life of an application. Instantiating an 
    /// HttpClient class for every request will exhaust the number of
    /// sockets available under heavy loads. 
    /// <strong> The IApiConnector service and its implementation must
    /// be configured with a "scoped" lifecycle to prevent exhausting
    /// available sockets.
    /// </strong> 
    /// </para>
    /// <para>
    /// Its methods have an "explicit interface implementation" to hide
    /// them from unwanted consumers. 
    /// </para>
    /// <para>
    /// System.Text.Json serializer-deserializer is configured in the
    /// Application/Server/Program class to ignore an object when a
    /// reference cycle is detected during serialization and to be
    /// case-insensitive although web defaults for JsonSerializerOptions
    /// includes PropertyNameCaseInsensitive. 
    /// </para>
    /// <para>
    /// It does not have an exception handling mechanism (try-catch blocks)
    /// because exceptions propagate up the stack until a catch statement for
    /// the exception is found. The Application/Client/ApiServices/ApiManager
    /// ApiEntityName that consumes method(s) in this ApiConnector has an
    /// exception handling mechanism. 
    /// </para>
    /// </remarks>
    public class ApiConnector : IApiConnector
    {
        /// <summary>
        /// Stores the absolute URI of the internet resource used when
        /// sending requests; includes the entire URI stored in the Uri
        /// instance, including all fragments and query strings. 
        /// </summary>
        private string? _baseUrl;

        /// <summary>
        /// Allows sending Http requests and receiving HTTP responses from a
        /// resource identified by a URI. Represents the typed HttpClient
        /// service that does not attach authorization JWTs to the requests/
        /// responses.
        /// <para>
        /// <see cref="HttpClientNoJwt"/> is a typed HttpClient (a class)
        /// that represents an HttpClient instance that is configured in
        /// the dependency injection container of the Application/Client to
        /// send Http requests/responses with NO authorization headers.
        /// </para>
        /// </summary>
        /// <remarks>
        /// A common use case for Http requests without authorization JWTs is
        /// when a User is not authenticated and requires access to
        /// Application/Server-Api resources decorated with an [AllowAnonymous]
        /// attribute; i.e., when it requests access to unprotected resources.
        /// <para>
        /// HttpClient is intended to be instantiated once and re-used
        /// throughout the life of an application. Instantiating an 
        /// HttpClient class for every request will exhaust the number of
        /// sockets available under heavy loads. 
        /// </para>
        /// <para>
        /// <strong> The IApiConnector service and its implementation must
        /// be configured with a "scoped" lifecycle to prevent exhausting
        /// available sockets.
        /// </strong> 
        /// </para>
        /// </remarks>
        private readonly HttpClientNoJwt _httpClientNoJwt;

        /// <summary>
        /// Allows sending Http requests and receiving HTTP responses from a
        /// resource identified by a URI. Represents the typed HttpClient
        /// service that attaches authorization JWTs to the requests/responses.
        /// <para>
        /// <see cref="HttpClientWithJwt"/> is a typed HttpClient (a class)
        /// that represents an HttpClient instance that is configured in
        /// the dependency injection container of the Application/Client to
        /// provide authorization headers that include JWTs (authorization
        /// tokens) on each Http request/response.
        /// </para>
        /// </summary>
        /// <remarks>
        /// The custom <see cref="HttpClientWithJwt"/> is designed to be
        /// consumed when an <strong>authenticated User</strong> intends to
        /// have access to Application/Server-Api resources (endpoints) that
        /// are protected.
        /// </remarks>
        private readonly HttpClientWithJwt _httpClientWithJwt;

        /// <summary>
        /// The actual HttpClient passed to the resource methods to build
        /// (serialize/deserialize) the Http requests/responses. 
        /// </summary>
        private HttpClient _httpClient = null!;

        /// <summary>
        /// The <see cref="IApiConnector"/> and its implementation (this
        /// class) are configured as a service in the dependency injection
        /// container of the Application/Client/Program class. It uses
        /// dependency injection to request a custom
        /// <see cref="HttpClientWithJwt"/> instance whenever the
        /// Application/Client - User makes a data request. 
        /// </summary>
        /// <param name="httpClientWithJwt">Concrete HttpClient instance
        /// used to send/receive Http requests/responses that include AuthZ
        /// JWTs. It is provided by the Application/Client automatically
        /// because the <see cref="IApiService"/> and its implementation
        /// <see cref="ApiService"/> are configured as a service in its
        /// dependency injection container.</param>
        /// <param name="httpClientNoJwt">Concrete HttpClient instance
        /// used to send/receive Http requests/responses that do NOT
        /// include AuthZ JWTs. It is provided by the Application/Client
        /// automatically because the <see cref="IApiService"/> and its
        /// implementation <see cref="ApiService"/> are configure as a service
        /// in its dependency injection container.</param>
        public ApiConnector(HttpClientWithJwt httpClientWithJwt,
            HttpClientNoJwt httpClientNoJwt)
        {
            /// Private field of type <see cref="HttpClientWithJwt"/> which is
            /// configured as a service that attaches JWT tokens in the Http
            /// request/response headers.
            ///
            /// HttpClientWithJwt MUST BE INSTANTIATED BEFORE HttpClient or it
            /// will throw an exception.
            this._httpClientWithJwt = httpClientWithJwt;

            /// Private field of type <see cref="HttpClientNoJwt"/> which is
            /// configured as a service that omits JWT tokens in the Http
            /// request/response headers.
            this._httpClientNoJwt = httpClientNoJwt;

            /// Moved to local helper method AllocateHttpClient(JwtOptions).
            /// 
            /// When using Blazor's WebAssembly template, the HttpClient's
            /// base address is set to the originating server's address.
            /// You can confirm this in the Main() method that resides in
            /// Appliation/Client/Program.cs
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/call-web-api?view=aspnetcore-6.0&pivots=webassembly#httpclient-and-json-helpers
            /// 
            /// The HttpClient.BaseAddress property gets or sets the base
            /// address or Uniform Resource Identifier (URI) of the internet
            /// resource used when sending requests; i.e., it takes a URL
            /// and returns a base address <strong>of type URI</strong>.
            ///
            /// As mentioned above, the HttpClient.BaseAddress property
            /// value is set in the Main() method of the
            /// Application/Client/Program.cs during configuration of the
            /// HttpClient service. 
            ///
            /// The URL value is passed, as a <strong>type URI</strong>, from
            /// the (builder)
            /// WebAssemblyHostBuilder.HostEnvironment.BaseAddress property
            /// value. This value is set depending on the environment that
            /// the application is running. The WebHostingEnvironment is
            /// configured in the Application/Server/Startup.cs file.
            ///
            /// In our example, the environment is "Development" which means
            /// that the value for the base address is set in the
            /// Application/Server/Properties/launchSettings.json file, under
            /// the "applicationUrl" key of the BlazorMovies.Server section.
            /// The Http and Https (sslPort) ports  are also defined there.
            ///
            /// If you go to the launchSettings.json file, you'll find that
            /// our application's secure URL is: https://localhost:7077/. This
            /// value is <strong>converted to a type URI</strong> by the
            /// HttpClient.BaseAddress property.
            /// 
            /// Uri.AbsoluteUri property gets the absolute URI; i.e., includes
            /// the entire URI stored in the Uri instance, including all fragments
            /// and query strings. This is just a precautionary measure in case
            /// the application is running as a sub-app:
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/?view=aspnetcore-6.0&tabs=visual-studio#app-base-path
            /// 
            /// In our example, the HttpClient.BaseAddress and the Uri.AbsoluteUri
            /// are the same. The value can also be found as the URL that appears
            /// in the address bar of the web browser.
            /// https://docs.microsoft.com/en-us/dotnet/api/system.uri?view=net-6.0
            /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.baseaddress?view=net-6.0
            /// https://docs.microsoft.com/en-us/dotnet/api/system.uri.absoluteuri?view=net-6.0
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/?view=aspnetcore-6.0&tabs=visual-studio#app-base-path
            //this._baseUrl = _httpClientWithJwt.HttpClient.BaseAddress?.AbsoluteUri;

            /// Http headers let the client and the server pass additional
            /// information with an HTTP request or response. E.g., "request
            /// headers" contain additional info about the resource to be 
            /// fetched, "response headers" contain additional information 
            /// about the response such as its location or the server
            /// providing it, and "representation headers" describe the 
            /// particular representation (or format) of the resource sent
            /// in an HTTP message body. 
            /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers
            ///
            /// This configuration was moved to the
            /// Application/Client/Helpers/HttpClientWithJwt class because that
            /// class is responsible for configuring Http instances. 
            //_httpClient.DefaultRequestHeaders.Clear();
            //_httpClient.DefaultRequestHeaders.Accept.Clear();

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
            ///
            /// This configuration was moved to the
            /// Application/Client/Helpers/HttpClientWithJwt class because that
            /// class is responsible for configuring Http instances.
            //_httpClient.DefaultRequestHeaders.Accept.Add(
            //    new System.Net.Http.Headers
            //        .MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Allocates an HttpClient instance of the
        /// <see cref="HttpClientWithJwt"/> or the <see cref="HttpClientNoJwt"/>
        /// services dependent on the value passed to satisfy its formal input
        /// parameter. 
        /// </summary>
        /// <param name="jwtOptions">Determines which HttpClient instance
        /// to allocate; e.g., the one that represents the service that appends
        /// JWTs or the one that doesn't.</param>
        private void AllocateHttpClient(JwtOptions jwtOptions)
        {
            _httpClient = jwtOptions == JwtOptions.IncludeJWTs
                ? _httpClientWithJwt.HttpClient
                : _httpClientNoJwt.HttpClient;

            /// When using Blazor's WebAssembly template, the HttpClient's
            /// base address is set to the originating server's address.
            /// You can confirm this in the Main() method that resides in
            /// Appliation/Client/Program.cs
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/call-web-api?view=aspnetcore-6.0&pivots=webassembly#httpclient-and-json-helpers
            /// 
            /// The HttpClient.BaseAddress property gets or sets the base
            /// address or Uniform Resource Identifier (URI) of the internet
            /// resource used when sending requests; i.e., it takes a URL
            /// and returns a base address <strong>of type URI</strong>.
            ///
            /// As mentioned above, the HttpClient.BaseAddress property
            /// value is set in the Main() method of the
            /// Application/Client/Program.cs during configuration of the
            /// HttpClient service. 
            ///
            /// The URL value is passed, as a <strong>type URI</strong>, from
            /// the (builder)
            /// WebAssemblyHostBuilder.HostEnvironment.BaseAddress property
            /// value. This value is set depending on the environment that
            /// the application is running. The WebHostingEnvironment is
            /// configured in the Application/Server/Startup.cs file.
            ///
            /// In our example, the environment is "Development" which means
            /// that the value for the base address is set in the
            /// Application/Server/Properties/launchSettings.json file, under
            /// the "applicationUrl" key of the BlazorMovies.Server section.
            /// The Http and Https (sslPort) ports  are also defined there.
            ///
            /// If you go to the launchSettings.json file, you'll find that
            /// our application's secure URL is: https://localhost:7077/. This
            /// value is <strong>converted to a type URI</strong> by the
            /// HttpClient.BaseAddress property.
            /// 
            /// Uri.AbsoluteUri property gets the absolute URI; i.e., includes
            /// the entire URI stored in the Uri instance, including all fragments
            /// and query strings. This is just a precautionary measure in case
            /// the application is running as a sub-app:
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/?view=aspnetcore-6.0&tabs=visual-studio#app-base-path
            /// 
            /// In our example, the HttpClient.BaseAddress and the Uri.AbsoluteUri
            /// are the same. The value can also be found as the URL that appears
            /// in the address bar of the web browser.
            /// https://docs.microsoft.com/en-us/dotnet/api/system.uri?view=net-6.0
            /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.baseaddress?view=net-6.0
            /// https://docs.microsoft.com/en-us/dotnet/api/system.uri.absoluteuri?view=net-6.0
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/?view=aspnetcore-6.0&tabs=visual-studio#app-base-path
            this._baseUrl = _httpClient.BaseAddress?.AbsoluteUri;
        }

        #region Post-Create methods

        /// <summary>
        /// Encapsulates an Api <em>resource method</em> and the details
        /// of building an HTTP POST request for the specified Uri
        /// (controller endpoint).
        /// It contains the value serialized as JSON (JavaScript Object 
        /// Notation) in the body of the Http request. 
        /// </summary>
        /// <remarks>
        /// It is consumed by the <see cref="ApiRepository{TEntity}"/>
        /// class with general functionality and by the ApiEntityName 
        /// classes with data entity specific operations.
        /// <para>
        /// Its "explicit interface implementation" limits its access.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The target type to deserialize to; e.g.,
        /// Genre.
        /// </typeparam>
        /// <param name="objectToPost">The entity object to insert into 
        /// the database.</param>
        /// <param name="controllerName">The name of the controller of 
        /// the resource (data entity) of interest; e.g., "genres".</param>
        /// <param name="routeTemplateComplement">Any additional route
        /// segments to add to the route template to build the URL that
        /// maps to a particular controller action (endpoint). It can
        /// include simple route segments (strings) and/or route parameters
        /// (inside curly braces) with or without constraints. For
        /// example: $"filter?textToSearch={movieTitle}".
        /// </param>
        /// <param name="jwtOptions">Enumeration determines if security JWTs
        /// (JSON web tokens) should be included in the Http request. If so,
        /// the <see cref="AllocateHttpClient"/> method allocates the
        /// <see cref="HttpClientWithJwt"/>, otherwise, it allocates the
        /// <see cref="HttpClientNoJwt"/>.
        /// <para>
        /// It is dependent on the authorization rules applied to the
        /// Application/Server-Api/Controllers/Controller action that this
        /// resource method is targeting; e.g., if decorated with an
        /// [AllowAnonymous] attribute, then no JWTs are required. 
        /// </para>
        /// </param>
        /// <returns>The deserialized JSON content from the response 
        /// message; i.e., the status line and the Http response headers
        /// which include the response body (resource data) successfully
        /// inserted into the database. 
        /// </returns>
        async Task<T> IApiConnector.InvokePostAsync<T>(
            T objectToPost,
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions)
        {
            AllocateHttpClient(jwtOptions);

            /// Custom method builds the <dfn>absolute URI</dfn> required
            /// to send (and match) an Http request to the internet resource
            /// (controller endpoint). It includes the entire Uri with all
            /// fragments and query strings.
            string? uri = BuildUri(controllerName, routeTemplateComplement);

            /// PostAsJsonAsync sends a POST request to the specified URI 
            /// containing the value serialized as JSON in the request body.
            /// 
            /// HttpResponseMessage represents the content of an Http
            /// response that includes the complete information of the
            /// Http response including the Status Code, Response Headers,
            /// and an optional Message Body.
            HttpResponseMessage httpResponseMessage =
                await _httpClient.PostAsJsonAsync(uri, objectToPost);

            /// Custom method to test if the HttpResponseMessage was
            /// successful. If not, it throws an HttpRequestException with
            /// its HttpContent serialized as a string. 
            await HandleHttpRequestErrorAsync(httpResponseMessage);

            /// The value that results from deserializing the content (in
            /// JSON format) of an HttpResponseMessage; i.e., the object
            /// value successfully inserted to the DB.
            T? deserializedData = await httpResponseMessage.Content
                .ReadFromJsonAsync<T>();

            return deserializedData ??
                   throw new ArgumentNullException(
                       $"{nameof(IApiConnector.InvokePostAsync)}",
                       @"Deserialized HttResponseMessage is null");
        }

        /// <summary>
        /// Encapsulates an Api<em>resource method</em> and the details
        /// of building an HTTP POST request for the specified Uri
        /// (controller endpoint).
        /// </summary>
        /// <remarks>
        /// It has two types of type parameters. First type parameter
        /// (TEntityDto) takes a combination of one or more data entities
        /// that have a relationship and one or more properties of each
        /// entity encapsulated into a single model (class) to serve a
        /// request from the client. The other type parameter (T) is
        /// the target type after deserializing JSON content from the
        /// Http response message.
        /// <para>
        /// The TEntityDto is serialized as JSON (JavaScript Object
        /// Notation) in the body of the Http request.
        /// </para>
        /// </remarks>
        /// <typeparam name="TEntityDto">A DTO that encapsulates a data
        /// entity (e.g., a type Movie) and any related data (entities) to
        /// be persisted to the <dfn>linking table</dfn> of the database.
        /// </typeparam>
        /// <typeparam name="T">The target type to deserialize to; e.g.,
        /// type Movie.</typeparam>
        /// <param name="entityDto">The entity object to insert into
        /// the database.</param>
        /// <param name="controllerName">The name of the controller of the
        /// resource (data entity) of interest; e.g., "movies".</param>
        /// <param name="routeTemplateComplement">Any additional route
        /// segments to add to the route template to build the URL that
        /// maps to a particular controller action (endpoint). It can
        /// include simple route segments (strings) and/or route parameters
        /// (inside curly braces) with or without constraints. For
        /// example: $"filter?textToSearch={movieTitle}".
        /// </param>
        /// <param name="jwtOptions">Enumeration determines if security JWTs
        /// (JSON web tokens) should be included in the Http request. If so,
        /// the <see cref="AllocateHttpClient"/> method allocates the
        /// <see cref="HttpClientWithJwt"/>, otherwise, it allocates the
        /// <see cref="HttpClientNoJwt"/>.
        /// <para>
        /// It is dependent on the authorization rules applied to the
        /// Application/Server-Api/Controllers/Controller action that this
        /// resource method is targeting; e.g., if decorated with an
        /// [AllowAnonymous] attribute, then no JWTs are required. 
        /// </para>
        /// </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the status line and the Http response headers which include
        /// the response body (resource data) successfully inserted into the
        /// database.</returns>
        async Task<T> IApiConnector.InvokePostAsync<TEntityDto, T>(
            TEntityDto entityDto,
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions)
        {
            AllocateHttpClient(jwtOptions);

            /// Custom method builds the <dfn>absolute URI</dfn> required
            /// to send (and match) an Http request to the internet resource
            /// (controller endpoint). It includes the entire Uri with all
            /// fragments and query strings.
            string? uri = BuildUri(controllerName, routeTemplateComplement);

            /// PostAsJsonAsync sends a POST request to the specified URI 
            /// containing the value serialized as JSON in the request body.
            /// 
            /// HttpResponseMessage represents the content of an Http
            /// response that includes the complete information of the
            /// Http response including the Status Code, Response Headers,
            /// and an optional Message Body.
            HttpResponseMessage httpResponseMessage =
                await _httpClient.PostAsJsonAsync(uri, entityDto);

            /// Custom method to test if the HttpResponseMessage was
            /// successful. If not, it throws an HttpRequestException with
            /// its HttpContent serialized as a string. 
            await HandleHttpRequestErrorAsync(httpResponseMessage);

            /// The value that results from deserializing the content (in
            /// JSON format) of an HttpResponseMessage; i.e., the object
            /// value successfully inserted to the DB.
            T? deserializedData = await httpResponseMessage.Content
                .ReadFromJsonAsync<T>();

            return deserializedData ??
                   throw new ArgumentNullException(
                       $"{nameof(IApiConnector.InvokePostAsync)}",
                       @"Deserialized HttResponseMessage is null");
        }

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Encapsulates an Api <em>resource method</em> and the details
        /// of building an HTTP GET request for the specified Uri
        /// (controller endpoint).
        /// </summary>
        /// <remarks>
        /// It is consumed by the 
        /// ApiRepository<typeparam name="T">&lt;TEntity&gt;</typeparam>
        /// class with general functionality and by the ApiEntityName
        /// classes with data entity specific operations. 
        /// <para>
        /// Its "explicit interface implementation" limits its access.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The target type to deserialize to; e.g.,
        /// Genre.
        /// </typeparam>
        /// <param name="controllerName">The name of the controller of 
        /// the resource (data entity) of interest; e.g., "genres".</param>
        /// <param name="routeTemplateComplement">Any additional route
        /// segments to add to the route template to build the URL that
        /// maps to a particular controller action (endpoint). It can
        /// include simple route segments (strings) and/or route parameters
        /// (inside curly braces) with or without constraints. For
        /// example: $"filter?textToSearch={movieTitle}".</param>
        /// <param name="jwtOptions">Enumeration determines if security JWTs
        /// (JSON web tokens) should be included in the Http request. If so,
        /// the <see cref="AllocateHttpClient"/> method allocates the
        /// <see cref="HttpClientWithJwt"/>, otherwise, it allocates the
        /// <see cref="HttpClientNoJwt"/>.
        /// <para>
        /// It is dependent on the authorization rules applied to the
        /// Application/Server-Api/Controllers/Controller action that this
        /// resource method is targeting; e.g., if decorated with an
        /// [AllowAnonymous] attribute, then no JWTs are required. 
        /// </para>
        /// </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the status line and the Http response headers which include
        /// the response body (resource data) successfully retrieved from the
        /// database. 
        /// </returns>
        async Task<T> IApiConnector.InvokeGetAsync<T>(
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions)
        {
            AllocateHttpClient(jwtOptions);

            /// Custom method builds the <dfn>absolute URI</dfn> required
            /// to send (and match) an Http request to the internet resource
            /// (controller endpoint). It includes the entire Uri with all
            /// fragments and query strings.
            string? uri = BuildUri(controllerName, routeTemplateComplement);

            /// System.Net.Http.Json.GetFromJsonAsync() extension method sends
            /// an HTTP GET request, parses (deserializes) the JSON response
            /// body, and returns the value as a .Net object but it does not
            /// include the HttpResponseMessage. For this reason, it is better
            /// to use its older version System.Net.Http.HttpClient.GetAsync()
            /// method shown below. 
            /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-6.0
            /// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-overview?pivots=dotnet-6-0
            //T? deserializedResponse =
            //  await _httpClient.GetFromJsonAsync<T>(uri);

            /// System.Net.Http.HttpClient.GetAsync() method sends an HTTP
            /// GET request and returns a 
            /// Task<HttpResponseMessage> that includes the status code and
            /// the data.
            /// https://esg.dev/posts/httpclient-with-system-text-json/
            /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-6.0
            /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.getasync?view=net-6.0#system-net-http-httpclient-getasync(system-string)
            /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage?view=net-6.0
            /// https://www.youtube.com/watch?v=N6JBjzPssQI
            HttpResponseMessage responseMessage =
                await _httpClient.GetAsync(uri);

            /// Custom method evaluates if the HttpResponse sent from a
            /// given Application/Server-Api/ controller is successful. 
            /// If not, it deserializes the Http response and passes it as
            /// the message property value of an
            /// <see cref="HttpRequestException"/>.
            /// 
            /// Every controller action should customize an exception
            /// message passed when an unexpected error is encountered. 
            /// This message is later consumed to convey a meaningful
            /// message to the end user. 
            await HandleHttpRequestErrorAsync(responseMessage);

            /// The value that results from deserializingas JSON the content
            /// of an HttpResponseMessage; i.e., the object value successfully
            /// retrieved from the database. 
            T? deserializedResponse =
                await responseMessage.Content.ReadFromJsonAsync<T>();

            return deserializedResponse ??
                   throw new ArgumentNullException(
                       $"{nameof(IApiConnector.InvokeGetAsync)}",
                       @"Deserialized HttpResponseMessage is null");
        }

        #endregion

        #region Put-Update methods

        /// <summary>
        /// Encapsulates an Api <em>resource method</em> and the details
        /// of building an HTTP PUT request for the specified Uri
        /// (controller endpoint).
        /// It contains the value serialized as JSON (JavaScript Object
        /// Notation) in the request body. 
        /// </summary>
        /// <remarks>
        /// It is consumed by the
        /// ApiRepository<typeparam name="T">&lt;TEntity&gt;</typeparam>
        /// class with general functionality and by the ApiEntityName
        /// classes with data entity specific operations. 
        /// <para>
        /// Its "explicit interface implementation" limits its access.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The target type to deserialize to; e.g.,
        /// Genre.
        /// </typeparam>
        /// <param name="objectToPut">The entity object with the new 
        /// values to overwrite the existing values in the database.</param>
        /// <param name="controllerName">The name of the controller of 
        /// the resource (data entity) of interest; e.g., "genres".</param>
        /// <param name="routeTemplateComplement">Any additional route
        /// segments to add to the route template to build the URL that
        /// maps to a particular controller action (endpoint). It can
        /// include simple route segments (strings) and/or route parameters
        /// (inside curly braces) with or without constraints. For
        /// example: $"filter?textToSearch={movieTitle}".</param>
        /// <param name="jwtOptions">Enumeration determines if security JWTs
        /// (JSON web tokens) should be included in the Http request. If so,
        /// the <see cref="AllocateHttpClient"/> method allocates the
        /// <see cref="HttpClientWithJwt"/>, otherwise, it allocates the
        /// <see cref="HttpClientNoJwt"/>.
        /// <para>
        /// It is dependent on the authorization rules applied to the
        /// Application/Server-Api/Controllers/Controller action that this
        /// resource method is targeting; e.g., if decorated with an
        /// [AllowAnonymous] attribute, then no JWTs are required. 
        /// </para>
        /// </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the status line and the Http response headers
        /// which include the response body (resource data) successfully
        /// updated into the database. 
        /// </returns>
        async Task<T> IApiConnector.InvokePutAsync<T>(
            T objectToPut,
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions)
        {
            AllocateHttpClient(jwtOptions);

            /// Custom method builds the <dfn>absolute URI</dfn> required
            /// to send (and match) an Http request to the internet resource
            /// (controller endpoint). It includes the entire Uri with all
            /// fragments and query strings.
            string? uri = BuildUri(controllerName, routeTemplateComplement);

            /// PutAsJsonAsync sends a PUT request to the URI containing the
            /// value serialized as JSON in the request body. 
            /// 
            /// HttpResponseMessage represents an HTTP response that includes
            /// the Status Code and the data. 
            HttpResponseMessage httpResponseMessage =
                await _httpClient.PutAsJsonAsync<T>(uri, objectToPut);

            /// Custom method to test if the HttpResponseMessage was
            /// successful. If not, it throws an HttpRequestException with its
            /// HttpContent serialized as a string. 
            await HandleHttpRequestErrorAsync(httpResponseMessage);

            /// The value that results from deserializing as JSON the content
            /// of an HttpResponseMessage; i.e., the object successfully
            /// inserted to the DB.
            T? deserializedResponse =
                await httpResponseMessage.Content.ReadFromJsonAsync<T>();

            return deserializedResponse ??
                   throw new ArgumentNullException(
                       $"{nameof(IApiConnector.InvokePutAsync)}",
                       @"Deserialized HttpResponseMessage is null.");
        }

        /// <summary>
        /// Encapsulates an Api <em>resource method</em> and the details
        /// of building an HTTP PUT request for the specified Uri
        /// (controller endpoint).
        /// It contains the value serialized as JSON (JavaScript Object
        /// Notation) in the request body. 
        /// </summary>
        /// <remarks>
        /// It has two types of type parameters. First type parameter
        /// (TEntityDto) takes a combination of one or more data entities
        /// and/or property values encapsulated into a single model (class)
        /// to send an Http PUT request. The other type parameter (T) is
        /// the target type after deserializing JSON content from the Http
        /// response message. 
        /// <para>
        /// Its implementation is consumed by the 
        /// ApiRepository<typeparam name="T">&lt;TEntity&gt;</typeparam>
        /// class with general functionality and by the ApiEntityName
        /// classes with data entity specific operations. 
        /// </para>
        /// <para>
        /// Its "internal" access modifier permits its access only to
        /// elements that reside in this same assembly (project).  
        /// </para>
        /// </remarks>
        /// <typeparam name="TEntityDto">A type that encapsulates a data
        /// entity with the new property values.</typeparam>
        /// <typeparam name="T">The target type to deserialize to; e.g.,
        /// type Movie.</typeparam>
        /// <param name="entityDto">A DTO that encapsulates a data entity
        /// with the new property values.</param>
        /// <param name="controllerName">The name of the controller of 
        /// the resource (data entity) of interest; e.g., "genres".</param>
        /// <param name="routeTemplateComplement">Any additional route
        /// segments to add to the route template to build the URL that
        /// maps to a particular controller action (endpoint). It can
        /// include simple route segments (strings) and/or route parameters
        /// (inside curly braces) with or without constraints. For
        /// example: $"filter?textToSearch={movieTitle}".
        /// </param>
        /// <param name="jwtOptions">Enumeration determines if security JWTs
        /// (JSON web tokens) should be included in the Http request. If so,
        /// the <see cref="AllocateHttpClient"/> method allocates the
        /// <see cref="HttpClientWithJwt"/>, otherwise, it allocates the
        /// <see cref="HttpClientNoJwt"/>.
        /// <para>
        /// It is dependent on the authorization rules applied to the
        /// Application/Server-Api/Controllers/Controller action that this
        /// resource method is targeting; e.g., if decorated with an
        /// [AllowAnonymous] attribute, then no JWTs are required. 
        /// </para>
        /// </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the status line and the Http response headers
        /// which include the response body (resource data) successfully
        /// updated into the database. 
        /// </returns>
        async Task<T> IApiConnector.InvokePutAsync<TEntityDto, T>(
            TEntityDto entityDto,
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions)
        {
            AllocateHttpClient(jwtOptions);

            /// Custom method builds the <dfn>absolute URI</dfn> required
            /// to send (and match) an Http request to the internet resource
            /// (controller endpoint). It includes the entire Uri with all
            /// fragments and query strings.
            string? uri = BuildUri(controllerName, routeTemplateComplement);

            /// PutAsJsonAsync sends a PUT request to the URI containing the
            /// value serialized as JSON in the request body. 
            /// 
            /// HttpResponseMessage represents an HTTP response that includes
            /// the Status Code and the data. 
            HttpResponseMessage httpResponseMessage =
                await _httpClient.PutAsJsonAsync(uri, entityDto);

            /// Custom method to test if the HttpResponseMessage was
            /// successful. If not, it throws an HttpRequestException with its
            /// HttpContent serialized as a string. 
            await HandleHttpRequestErrorAsync(httpResponseMessage);

            /// The value that results from deserializing as JSON the content
            /// of an HttpResponseMessage; i.e., the object successfully
            /// inserted to the DB.
            T? deserializedResponse = await httpResponseMessage.Content
                .ReadFromJsonAsync<T>();

            return deserializedResponse ??
                   throw new ArgumentNullException(
                       $"{nameof(IApiConnector.InvokePutAsync)}",
                       @"Deserialized HttpResponseMessage is null.");
        }

        #endregion

        #region Delete methods

        /// <summary>
        /// Encapsulates an Api <em>resource method</em> and the details of
        /// building an HTTP DELETE request for the specified Uri (controller
        /// endpoint).
        /// </summary>
        /// <remarks>
        /// It is consumed by the <see cref="ApiRepository{TEntity}"/> class
        /// with general functionality and by the ApiEntityName classes with
        /// data entity specific operations. 
        /// <para>
        /// Its "explicit interface implementation" limits its access. 
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The target type to deserialize to; e.g., Genre.
        /// </typeparam>
        /// <param name="controllerName">The name of the controller of the
        /// resource (data entity) of interest; e.g., "genres".</param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint). It can include simple
        /// route segments (strings) and/or route parameters (inside curly
        /// braces) with or without constraints. For example:
        /// $"filter?textToSearch={movieTitle}".
        /// </param>
        /// <param name="jwtOptions">Enumeration determines if security JWTs
        /// (JSON web tokens) should be included in the Http request. If so,
        /// the <see cref="AllocateHttpClient"/> method allocates the
        /// <see cref="HttpClientWithJwt"/>, otherwise, it allocates the
        /// <see cref="HttpClientNoJwt"/>.
        /// <para>
        /// It is dependent on the authorization rules applied to the
        /// Application/Server-Api/Controllers/Controller action that this
        /// resource method is targeting; e.g., if decorated with an
        /// [AllowAnonymous] attribute, then no JWTs are required. 
        /// </para>
        /// </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the status line and the Http response headers
        /// which include the response body (resource data) successfully
        /// removed from the database.  
        /// </returns>
        async Task<T> IApiConnector.InvokeDeleteAsync<T>(
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions)
        {
            AllocateHttpClient(jwtOptions);

            /// Custom method builds the <dfn>absolute URI</dfn> required
            /// to send (and match) an Http request to the internet resource
            /// (controller endpoint). It includes the entire Uri with all
            /// fragments and query strings.
            string? uri = BuildUri(controllerName, routeTemplateComplement);

            /// DeleteAsync sends a DELETE request to the specified URI.
            /// 
            /// HttpResponseMessage represents an HTTP response that
            /// includes the Status Code and the data. 
            HttpResponseMessage httpResponseMessage =
                await _httpClient.DeleteAsync(uri);

            /// Custom method to test if the HttpResponseMessage was
            /// successful. If not, it throws an HttpRequestException with
            /// its HttpContent serialized as a  string. 
            await HandleHttpRequestErrorAsync(httpResponseMessage);

            /// HttpResponseMessage.Content property gets the content of the
            /// response message. ReadFromJasonAsync() reads the Http content
            /// and returns the value that results from deserializing the
            /// content as JSON.
            T? deserializedResponse =
                await httpResponseMessage.Content.ReadFromJsonAsync<T>();

            return deserializedResponse ??
                   throw new ArgumentNullException(
                       $"{nameof(IApiConnector.InvokeDeleteAsync)}",
                       @"Deserialized HttResponseMessage is null");
        }

        #endregion

        /// <summary>
        /// Builds the URI that the HTTP request must be sent to; i.e.,
        /// the URI that the routing middleware will use to match with
        /// the route template of a controller endpoint. It adds a segment,
        /// or relative path of the controller route template, to the absolute
        /// Uri. 
        /// <para>
        /// In a REST-Api, data and functionality are considered resources
        /// and they are accessed using Uniform Resource Identifiers (URIs).
        /// </para>
        /// </summary>
        /// <remarks>
        /// In the context of your API (Application/Server-Api/Controllers),
        /// a resource is a data entity identified or accessed through a URI
        /// similar to: 
        /// <see href="https://localhost:44365/api/genres/2"/>
        /// built with the following segments:
        /// <para>
        /// BaseUrl: <see href="https://localhost:44365/"/>
        /// </para>
        /// <para>
        /// Endpoint (controller route template): "api/genres/"
        /// </para>
        /// <para>
        /// Route parameters: "5"
        /// </para>
        /// </remarks>
        /// <param name="controllerName">The name of the API/Controller of
        /// the resource (data entity) of interest; e.g., "genres".</param>
        /// <param name="routeTemplateComplement">Any additional route
        /// segments to add to the route template to build the URL that
        /// maps to a particular controller action (endpoint). It can
        /// include simple route segments (strings) and/or route parameters
        /// (inside curly braces) with or without constraints. For
        /// example: $"filter?textToSearch={movieTitle}".
        /// </param>
        /// <returns>The absolute URI required for sending an HTTP request and
        /// matching it with the route template of the desired controller
        /// endpoint. It includes the entire URI with all fragments and query
        /// strings.
        /// </returns>
        private string? BuildUri(
            string controllerName,
            string? routeTemplateComplement)
        {
            if (_baseUrl is null)
                return null;

            /// No forward slash delimiter between {controllerName} and
            /// {routeTemplateComplement} because it may not be necessary;
            /// e.g., when building a query string the forward slash is
            /// replaced by a question mark delimiter.
            ///
            /// The delimiter, if any, should be included when producing the
            /// routeTemplateComplement which is usually inside the
            /// Application/Repository/ApiManager/ApiEntityName class.
            if (_baseUrl.EndsWith("/"))
            {
                return $"{_baseUrl}api/{controllerName}{routeTemplateComplement}"
                    .ToLower();
            }

            return $"{_baseUrl}/api/{controllerName}{routeTemplateComplement}"
                .ToLower();
        }

        /// <summary>
        /// Evaluates if the HttpResponse sent from a given
        /// Application/Server-Api/ controller is successful. If not, it
        /// deserializes the Http response and passes it as the message
        /// property value of an <see cref="HttpRequestException"/>.
        /// </summary>
        /// <remarks>
        /// Every controller action should customize any exception message
        /// passed when an error is encountered.
        /// <para>
        /// The <see cref="ApiConnector"/> class is the bridge between the
        /// back-end code (Application/Server-Api controllers) and the
        /// front-end code (Application/Client). 
        /// </para>
        /// </remarks>
        /// <param name="response">The HttpResponseMessage including the
        /// StatusCode and data.</param>
        /// <returns>A representation of an async operation. If the
        /// HttpResponse was unsuccessful, it throws an HttpRequestException
        /// with its HttpContent serialized as a string.</returns>
        /// <exception cref="HttpRequestException">Describes the current
        /// exception. </exception>
        private static async Task HandleHttpRequestErrorAsync(
            HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                /// The HttpResponseMessage.Content property gets or sets
                /// the content of an HTTP response message. The contents 
                /// of an HTTP response message correspond to the entity
                /// body (response body).
                /// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpresponsemessage.content?view=net-6.0#system-net-http-httpresponsemessage-content
                /// 
                /// The ReadAsStringAsync() method serializes the Http
                /// content to a string.
                string error = await response.Content.ReadAsStringAsync();

                /// Instantiates an HttpRequestException with a specific
                /// message that describes the current exception. 
                throw new HttpRequestException(error);
            }
        }
    }
}



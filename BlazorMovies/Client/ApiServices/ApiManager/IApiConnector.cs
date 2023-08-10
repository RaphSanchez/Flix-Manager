
using BlazorMovies.Shared.Helpers;

namespace BlazorMovies.Client.ApiServices.ApiManager
{
    /// <summary>
    /// The IApiConnector interface establishes a protocol that defines 
    /// methods that are equivalent to Http verbs that can be mapped to
    /// the Application/Server-RESTful-Api/Controllers/EntityControllers.
    /// <para>
    /// These methods are generic; i.e., they can be used to reach any
    /// resource type and receive any response (data entity type and
    /// Status Code). 
    /// </para>
    /// </summary>
    /// <remarks>
    /// The controllers model the application's functionality as a set
    /// of resources (e.g., data entities) where operations are represented
    /// by HTTP verbs. 
    /// <para>
    /// The generic methods defined here encapsulate the <em>resource
    /// methods</em> (.Net's JSON helper methods) that build the Http
    /// requests/responses and serialize/deserialize .Net objects to JSON
    /// format so they can travel through the internet to the
    /// Application/Server-Api/Controllers and back. They can be consumed
    /// to reach any resource type.
    /// </para>
    /// <para>   
    /// IApiConnector's implementation consumes an instance of HttpClient.
    /// HttpClient is intended to be instantiated once and re-used
    /// throughout the life of an application because instantiating an 
    /// HttpClient class for every request will exhaust the number of
    /// sockets available under heavy loads. 
    /// <strong> The IApiConnector service and its implementation must
    /// be configured with a "scoped" lifecycle to prevent exhausting
    /// available sockets.<br/>
    /// </strong> 
    /// <a href="https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-6.0">
    /// HttpClient class</a>
    /// </para>
    /// </remarks>
    public interface IApiConnector
    {
        #region Post-Create methods

        /// <summary>
        /// Encapsulates an Api <em>resource method</em> and the details
        /// of building an HTTP POST request for the specified Uri
        /// (controller endpoint).
        /// It contains the value serialized as JSON (JavaScript Object 
        /// Notation) in the body of the Http request. 
        /// </summary>
        /// <remarks>
        /// Its implementation is consumed by the 
        /// ApiRepository<typeparam name="T">&lt;TEntity&gt;</typeparam> 
        /// class with general functionality and by the ApiEntityName 
        /// classes with data entity specific operations. 
        /// <para>
        /// Its "internal" access modifier permits its access only to
        /// elements that reside in this same assembly (project). 
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The target type to deserialize to; e.g.,
        /// Genre.
        /// </typeparam>
        /// <param name="objectToPost">The entity object to insert into 
        /// the database.</param>
        /// <param name="controllerName">The name of the controller of the 
        /// resource (data entity) of interest; e.g., "genres".</param>
        /// <param name="routeTemplateComplement">Any additional route
        /// segments to add to the route template to build the URL that
        /// maps to a particular controller action (endpoint). It can
        /// include simple route segments (strings) and/or route parameters
        /// (inside curly braces) with or without constraints. For
        /// example: $"filter?textToSearch={movieTitle}".
        /// </param>
        /// <param name="jwtOptions">Enumeration determines if security JWTs
        /// (JSON web tokens) should be included in the Http request. If so,
        /// the <see cref="ApiConnector.AllocateHttpClient"/> method allocates the
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
        public Task<T> InvokePostAsync<T>(
            T objectToPost,
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions = JwtOptions.IncludeJWTs);

        /// <summary>
        /// Encapsulates an Api <em>resource method</em> and the details
        /// of building an HTTP POST request for the specified Uri
        /// (controller endpoint).
        /// </summary>
        /// <remarks>
        /// It has two types of type parameters. First type parameter
        /// <typeparamref name="TEntityDto"/> takes a combination of one or
        /// more data entities that have a relationship and one or more
        /// properties of each entity encapsulated into a single model (class)
        /// to build a request from the client (data type sent to the server
        /// by the client).
        /// <para>
        /// <para>The other type parameter <typeparamref name="T"/>
        /// is the target type after deserializing JSON content from the Http
        /// response message (data type received by the client from the
        /// server).</para>
        /// The <typeparamref name="TEntityDto"/> is serialized as JSON (JavaScript Object
        /// Notation) in the body of the Http request.
        /// </para>
        /// </remarks>
        /// <typeparam name="TEntityDto">A type that encapsulates a data
        /// entity (e.g., a type Movie) and any related data (entities) to
        /// be persisted to the <dfn>linking table</dfn> of the database.
        /// </typeparam>
        /// <typeparam name="T">The target type to deserialize to; e.g.,
        /// type Movie.</typeparam>
        /// <param name="entityDto">The entity object that encapsulates the
        /// data entity object to insert into the database and its related
        /// data (entities).
        /// </param>
        /// <param name="controllerName">The name of the controller of the
        /// resource (data entity) of interest; e.g., "movies".</param>
        /// <param name="routeTemplateComplement">Any additional route
        /// segments to add to the route template to build the URL that
        /// maps to a particular controller action (endpoint). It can
        /// include simple route segments (strings) and/or route parameters
        /// (inside curly braces) with or without constraints. For
        /// example: $"filter?textToSearch={movieTitle}".</param>
        /// <param name="jwtOptions">Enumeration determines if security JWTs
        /// (JSON web tokens) should be included in the Http request. If so,
        /// the <see cref="ApiConnector.AllocateHttpClient"/> method allocates the
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
        public Task<T> InvokePostAsync<TEntityDto, T>(
            TEntityDto entityDto,
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions = JwtOptions.IncludeJWTs);

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Encapsulates an Api <em>resource method</em> and the details
        /// of building an HTTP GET request for the specified Uri
        /// (controller endpoint).
        /// </summary>
        /// <remarks>
        /// Its implementation is consumed by the 
        /// ApiRepository<typeparam name="T">&lt;TEntity&gt;</typeparam>
        /// class with general functionality and by the ApiEntityName 
        /// classes with data entity specific operations. 
        /// <para>
        /// Its "internal" access modifier permits its access only to 
        /// elements that reside in this same assembly (project). 
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The target type to deserialize to; e.g., Genre.
        /// </typeparam>
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
        /// the <see cref="ApiConnector.AllocateHttpClient"/> method allocates the
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
        public Task<T> InvokeGetAsync<T>(
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions = JwtOptions.IncludeJWTs);
        
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
        /// Its implementation is consumed by the 
        /// ApiRepository<typeparam name="T">&lt;TEntity&gt;</typeparam>
        /// class with general functionality and by the ApiEntityName
        /// classes with data entity specific operations. 
        /// <para>
        /// Its "internal" access modifier permits its access only to
        /// elements that reside in this same assembly (project).  
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The target type to deserialize to; e.g., Genre.
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
        /// example: $"filter?textToSearch={movieTitle}".
        /// </param>
        /// <param name="jwtOptions">Enumeration determines if security JWTs
        /// (JSON web tokens) should be included in the Http request. If so,
        /// the <see cref="ApiConnector.AllocateHttpClient"/> method allocates the
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
        public Task<T> InvokePutAsync<T>(
            T objectToPut,
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions = JwtOptions.IncludeJWTs);

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
        /// the <see cref="ApiConnector.AllocateHttpClient"/> method allocates the
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
        public Task<T> InvokePutAsync<TEntityDto, T>(
            TEntityDto entityDto,
            string controllerName,
            string? routeTemplateComplement, 
            JwtOptions jwtOptions = JwtOptions.IncludeJWTs);

        #endregion

        #region Delete methods

        /// <summary>
        /// Encapsulates an Api <em>resource method</em> and the details
        /// of building an HTTP DELETE request for the specified Uri
        /// (controller endpoint).
        /// </summary>
        /// <remarks>
        /// Its implementation is consumed by the 
        /// ApiRepository<typeparam name="T">&lt;TEntity&gt;</typeparam>
        /// class with general functionality and by the ApiEntityName 
        /// classes with data entity specific operations. 
        /// <para>
        /// Its "internal" access modifier permits its access only to 
        /// elements that reside in this same assembly (project). 
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The target type to deserialize to; e.g., Genre.
        /// </typeparam>
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
        /// the <see cref="ApiConnector.AllocateHttpClient"/> method allocates the
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
        public Task<T> InvokeDeleteAsync<T>(
            string controllerName,
            string? routeTemplateComplement,
            JwtOptions jwtOptions = JwtOptions.IncludeJWTs);

        #endregion
    }
}



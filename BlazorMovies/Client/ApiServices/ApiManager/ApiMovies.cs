using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

namespace BlazorMovies.Client.ApiServices.ApiManager
{
    /// <summary>
    /// One application specific ApiEntityName class for each
    /// IEntityName interface exposed in the <see cref="IApiService"/>
    /// interface.
    /// </summary>
    /// <remarks>
    /// It is a subclass of the <see cref="ApiRepository{TEntity}"/>
    /// class which means it inherits its general functionality
    /// applicable to all data entities. 
    /// <para>
    /// This class is application specific and extends its base class
    /// with specific functionality for the type passed as type parameter.
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in
    /// the result of a query with EF's "Include" extension method.
    /// </para>
    /// <para>
    /// Its methods have an "explicit interface implementation" to hide
    /// them from unwanted consumers. 
    /// </para>
    /// </remarks>
    internal class ApiMovies : ApiRepository<Movie>, IMovies
    {
        /// <summary>
        /// The name of the Application/Sever-Api/Controller of the
        /// resource (data entity).
        /// </summary>
        private const string ControllerName = "movies";

        /// <summary>
        /// Its formal input parameter <paramref name="apiConnector"/> is not
        /// stored in a local variable because it is not consumed like that.
        /// Instead, it is passed to satisfy its base class's constructor and
        /// it is that parent class which consumes it and also makes it
        /// available to any child class through a field named
        /// <see cref="ApiConnector"/> with a "<c>protected read-only</c>"
        /// access modifier. 
        /// </summary>
        /// <remarks>
        /// This structure ensures that a complete business transaction
        /// can have multiple operations with different entity types 
        /// using a single instance of a class that implements the
        /// <see cref="IApiConnector"/> interface which in turn employs a
        /// single instance of the <see cref="HttpClient"/> class to avoid
        /// exhausting the web sockets under heavy loads.
        /// </remarks>
        /// <param name="apiConnector">Instance responsible for building
        /// the URI to map to the Application/Server-Api controller and
        /// for sending/receiving Http requests/responses.</param>
        public ApiMovies(IApiConnector apiConnector)
            : base(ControllerName, apiConnector)
        { }

        #region Post-Create methods

        /// <summary>
        /// Sends an Http request to insert an entity to the
        /// database. 
        /// </summary>
        /// <param name="movieDto">A DTO that encapsulates a data entity
        /// of type Movie and any related data entities (e.g., Genre and
        /// Person types) to be persisted to the <dfn>linking table</dfn>
        /// of the database.</param>
        /// <returns>The deserialized JSON content from the Http response
        /// message; i.e., the object value successfully inserted into the
        /// database. It includes its related data (entities).</returns>
        async Task<Movie?> IMovies.CreateAsync(MovieEssentialsDto movieDto)
        {
            try
            {
                Movie insertedMovie =
                    await ApiConnector
                        .InvokePostAsync<MovieEssentialsDto, Movie>(
                            movieDto,
                            ControllerName,
                            routeTemplateComplement: null,
                            jwtOptions: JwtOptions.IncludeJWTs);

                return insertedMovie;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Sends an Http request with a <see cref="MoviesQueryFilterDto"/>
        /// that encapsulates property values that can be directly related
        /// to one or more properties of a type <see cref="Movie"/> and
        /// property values with the pagination parameters required by the
        /// client.
        /// </summary>
        /// The property values are used as filtering criteria and as
        /// pagination parameters. 
        /// <param name="moviesQueryFilterDto">The DTO that encapsulates
        /// values that can be directly related to one or more properties of
        /// a type <see cref="Movie"/> and the parameters to paginate the
        /// response.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the collection of items successfully retrieved from the
        /// database, filtered following the filtering criteria specified,
        /// and paginated according to the pagination parameters sent by the
        /// client.</returns>
        async Task<PaginatedResponseDto<IEnumerable<Movie>>>
            IMovies.FilterPaginateMoviesAsync(
                MoviesQueryFilterDto moviesQueryFilterDto)
        {
            try
            {
                /// The Application/Server-Api/Controllers/MoviesController 
                /// decorates its FilterPaginateMoviesTask action (method)
                /// with an [HttpPost] route template that includes a
                /// "filter" route segment.
                ///
                /// The ApiConnector class responsible for building the URL for
                /// the HTTP request includes the "filter" route segment
                /// to indicate .Net Core routing middleware to dispatch the HTTP
                /// request to the the action in the MoviesController that matches.
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = "/filter";

                /// Consumes an Api <em>resource method</em> with the details
                /// of building an Http POST request for the appropriate
                /// endpoint (controller's route template).
                ///
                /// Although the ultimate purpose for the Server-Api endpoint
                /// (FilterPaginateMoviesTask) is to get-read-retrieve data,
                /// it is decorated with an [HttpPost] Http verb template
                /// because its formal input parameter is decorated with a
                /// [FromBody] data binding source attribute to delegate to
                /// ASP.Net the responsibility of populating the type's
                /// (MoviesQueryFilterDto) properties from the body of the
                /// Http request. 
                /// 
                /// The reason being that "a payload within a GET request
                /// message has no defined semantics; sending a payload body
                /// on a GET request might cause some existing implementations
                /// to reject the request". 
                PaginatedResponseDto<IEnumerable<Movie>> paginatedResponseDto =
                await ApiConnector.InvokePostAsync
                    <MoviesQueryFilterDto, PaginatedResponseDto<IEnumerable<Movie>>>
                    (
                        moviesQueryFilterDto,
                        ControllerName,
                        routeTemplateComplement,
                        /// FilterPaginateMoviesAsync only retrieves data
                        /// eventhough the controller that it targets has an
                        /// [HttpPost] verb. The controller is decorated with
                        /// an [AllowAnonymous] attribute. Refer to the
                        /// MoviesController for more info. 
                        jwtOptions: JwtOptions.OmitJWTs
                    );

                return paginatedResponseDto;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        /// <summary>
        /// Sends an Http request that encapsulates a FlixManagerDto
        /// with Movie collections required by the FlixManager routable
        /// component. 
        /// </summary>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., a FlixManagerDto successfully constructed or an empty
        /// FlixManagerDto object if unsuccessful.  
        /// </returns>
        async Task<FlixManagerDto> IMovies.GetFlixManagerDtoAsync()
        {
            try
            {
                /// The Application/Server-Api/Controllers/MoviesController 
                /// decorates its GetFlixManagerDtoTask action (method)
                /// with an HttpGet route template that includes a
                /// "flix-manager" route segment.
                ///
                /// The ApiConnector class responsible for building the URL for
                /// the HTTP request includes the "flix-manager" route segment
                /// to indicate .Net Core routing middleware to dispatch the HTTP
                /// request to the the action in the MoviesController that matches.
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = "/flix-manager";

                FlixManagerDto flixManagerDto =
                    await ApiConnector.InvokeGetAsync<FlixManagerDto>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.OmitJWTs);

                return flixManagerDto;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including its name, message, stack trace,
                /// and any inner exceptions. It employs a
                /// <see cref="StringBuilder"/> to construct the information
                /// and send it to the debugging console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        /// <summary>
        /// Sends an Http request to retrieve an entity with a primary key
        /// property value that matches the value passed to satisfy its
        /// formal input parameter <em>MovieId</em>.
        /// </summary>
        /// <remarks>
        /// This method is employed by the MovieBulletin routable component for
        /// scenarios where the current <see cref="ApplicationUser"/> is not
        /// authenticated.
        /// <para>
        /// It encapsulates a MovieBulletinDto that includes in the
        /// <em>response body</em> of the Http response.
        /// </para>
        /// </remarks>
        /// <param name="movieId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., a MovieBulletinDto successfully constructed or null if
        /// unsuccessful.
        /// </returns>
        async Task<MovieBulletinDto?> IMovies.GetMovieBulletinDtoAsync(
            int movieId)
        {
            try
            {
                /// The initial delimiter is required.
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = $"/{movieId}";

                MovieBulletinDto movieBulletinDto =
                    await ApiConnector.InvokeGetAsync<MovieBulletinDto>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.OmitJWTs);

                return movieBulletinDto;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        /// <summary>
        /// Sends an Http request to retrieve an entity with a primary key
        /// property value that matches the value passed to satisfy its 
        /// formal input parameter <em>MovieId</em>.
        /// </summary>
        /// <remarks>
        /// This method is employed by the MovieBulletin routable component for
        /// scenarios where the current <see cref="ApplicationUser"/> is 
        /// successfully authenticated.
        /// <para>
        /// It encapsulates a MovieBulletinDto that includes in the
        /// <em>response body</em> of the Http response.
        /// </para>
        /// </remarks>
        /// <param name="movieId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., a MovieBulletinDto successfully constructed or null if
        /// unsuccessful.
        /// </returns>
        async Task<MovieBulletinDto?> IMovies.GetMovieBulletinWithUserScoreDtoAsync(
            int movieId)
        {
            try
            {
                /// The initial delimiter is required.
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = $"/user-score/{movieId}";

                MovieBulletinDto movieBulletinDto =
                    await ApiConnector.InvokeGetAsync<MovieBulletinDto>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.IncludeJWTs);

                return movieBulletinDto;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        /// <summary>
        /// Sends an Http request to retrieve an entity with a primary key
        /// property value that matches the value passed to satisfy its
        /// formal input parameter <em>MovieId</em>.
        /// </summary>
        /// <param name="movieId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., a MovieEditDto successfully constructed or null if
        /// unsuccessful.
        /// </returns>
        async Task<MovieEditDto?> IMovies.GetMovieEditDtoAsync(int movieId)
        {
            try
            {
                /// It builds the route segment required to match the route
                /// template of the endpoint with the code logic to serve
                /// a MovieEditDto.
                ///
                /// Converts a string to an Html encoded string.
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = $"/edit/{movieId}";

                MovieEditDto movieEditDto =
                    await ApiConnector.InvokeGetAsync<MovieEditDto>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.OmitJWTs);

                return movieEditDto;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        #endregion

        #region Put-Update methods

        async Task<MovieEditDto?> IMovies.UpdateMovieAsync(
            int entityId, MovieEssentialsDto? dtoWithNewValues)
        {
            try
            {
                string routeTemplateComplement = $"/{entityId}";

                MovieEditDto updatedMovieDto =
                    await ApiConnector
                        .InvokePutAsync<MovieEssentialsDto, MovieEditDto>(
                            dtoWithNewValues!,
                            ControllerName,
                            routeTemplateComplement,
                            jwtOptions: JwtOptions.IncludeJWTs);

                return updatedMovieDto;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        #endregion

        #region Delete methods

        /// <summary>
        /// Sends an Http request to delete a Movie object from the
        /// database.
        /// </summary>
        /// <remarks>
        /// The root entity type Movie is decorated with the custom
        /// "IsAuditable" attribute and an IsDeletable formal input
        /// parameter; i.e., it implements soft-delete.
        /// </remarks>
        /// <param name="movieId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the Movie object successfully deleted or null if
        /// unsuccessful.
        /// </returns>
        async Task<Movie?>? IMovies.DeleteMovieAsync(int movieId)
        {
            try
            {
                /// Converts a string to an Html encoded string.
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = $"/{movieId}";

                Movie deletedMovie =
                    await ApiConnector.InvokeDeleteAsync<Movie>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.IncludeJWTs);

                return deletedMovie;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        #endregion

        #region Reset Data methods

        /// <summary>
        /// Sends an Http request to reset the database of the application and
        /// the containers responsible for serving the app with the images for
        /// <see cref="Movie"/> and <see cref="Person"/> objects.
        /// </summary>
        /// <returns>True if the reset operation is successful. Otherwise
        /// false.</returns>
        async Task<bool> IMovies.ResetDatabaseAsync()
        {
            try
            {
                /// The Application/Server-Api/Controllers/MoviesController 
                /// decorates its ResetDatabaseTask action (method)
                /// with an HttpGet route template that includes a
                /// "reset-db" route segment.
                ///
                /// The ApiConnector class responsible for building the URL for
                /// the HTTP request includes the "reset-db" route segment
                /// to indicate .Net Core routing middleware to dispatch the HTTP
                /// request to the the action in the MoviesController that matches.
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = "/reset-db";

                bool successfulDatabaseReset =
                    await ApiConnector.InvokeGetAsync<bool>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.OmitJWTs);

                return successfulDatabaseReset;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including its name, message, stack trace,
                /// and any inner exceptions. It employs a
                /// <see cref="StringBuilder"/> to construct the information
                /// and send it to the debugging console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        #endregion
    }
}




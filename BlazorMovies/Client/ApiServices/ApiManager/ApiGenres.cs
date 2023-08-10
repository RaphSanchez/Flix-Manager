using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

namespace BlazorMovies.Client.ApiServices.ApiManager
{
    /// <summary>
    /// One application specific ApiEntityName class for each
    /// IEntityName interface exposed in the IApiService interface.
    /// </summary>
    /// <remarks>
    /// It is a subclass of the 
    /// ApiRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
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
    internal class ApiGenres : ApiRepository<Genre>, IGenres
    {
        /// <summary>
        /// The name of the Application/Sever-Api/Controller for the
        /// resource (data entity).
        /// </summary>
        private const string ControllerName = "genres";

        /// <summary>
        /// Its formal input parameter (IApiConnector) is not stored
        /// in a local variable because it is not consumed like that.
        /// Instead, it is passed to satisfy its base class's 
        /// constructor and it is that parent class which consumes
        /// it and also makes it available to any child class through
        /// a field named "ApiConnector" with a "read-only 
        /// protected" access modifier. 
        /// </summary>
        /// <remarks>
        /// This structure ensures that a complete business transaction
        /// can have multiple operations with different entity types 
        /// using a single instance of a class that implements the
        /// IApiConnector interface which in turn employs a single 
        /// instance of the HttpClient class to avoid exhausting the
        /// web sockets under heavy loads.
        /// </remarks>
        /// <param name="apiConnector">Instance responsible for building
        /// the URI to map to the Application/Server-Api controller and
        /// for sending/receiving Http requests/responses.</param>
        public ApiGenres(IApiConnector apiConnector)
            : base(ControllerName, apiConnector)
        { }

        #region Post-Create methods

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Sends an Http request with a GenresQueryFilterDto that 
        /// encapsulates property values that can be directly related
        /// to one or more properties of a type Person. 
        /// </summary>
        /// <remarks>
        /// The property values are used as filtering criteria.
        /// <para>
        /// The method is case insensitive because it employs a .ToLower()
        /// extension. 
        /// </para>
        /// </remarks>
        /// <param name="genresDto">The DTO that encapsulates property values
        /// that can be directly related to one or more properties of a type
        /// Genre.
        /// </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the collection of items successfully retrieved from the
        /// database. 
        /// </returns>
        async Task<IEnumerable<Genre>> IGenres.FilterAsync(
            GenresQueryFilterDto genresDto)
        {
            try
            {
                /// The Application/Server-Api/Controllers/GenresController 
                /// decorates its FilterGenresTask action (method) with an
                /// HttpGet route template that includes a "filter" route
                /// segment.
                /// 
                /// The ApiConnector class responsible for building the URL
                /// for the HTTP request includes the "filter" route segment
                /// to indicate .Net Core routing middleware to dispatch the
                /// HTTP request to the action in the GenresController that
                /// matches.
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement =
                    "/filter?" +
                    $"id={genresDto.Id}" +
                    $"&name={genresDto.Name}";

                /// Encapsulates an Api <em>resource method</em> and the
                /// details of building an Http GET request for the 
                /// appropriate endpoint (URI).
                IEnumerable<Genre> genreItems =
                    await ApiConnector.InvokeGetAsync<IEnumerable<Genre>>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.OmitJWTs);

                return genreItems;
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

        #endregion

        #region Delete methods

        /// <summary>
        /// Sends an Http request to delete a Genre object from the
        /// database.
        /// </summary>
        /// <remarks>
        /// The root entity type Genre is decorated with the custom
        /// "IsAuditable" attribute and an IsDeletable formal input
        /// parameter; i.e., it implements soft-delete.
        /// </remarks>
        /// <param name="genreId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the Genre object successfully deleted or null if
        /// unsuccessful.
        /// </returns>
        async Task<Genre?>? IGenres.DeleteGenreAsync(int genreId)
        {
            try
            {
                /// The delimiter is necessary.
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = $"/{genreId}";

                Genre deletedGenre =
                    await ApiConnector.InvokeDeleteAsync<Genre>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.IncludeJWTs);

                return deletedGenre;
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
    }
}


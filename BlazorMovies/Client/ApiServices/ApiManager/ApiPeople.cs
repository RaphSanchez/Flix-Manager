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
    internal class ApiPeople : ApiRepository<Person>, IPeople
    {
        /// <summary>
        /// The name of the Application/Sever-Api/Controller of the
        /// resource (data entity).
        /// </summary>
        private const string ControllerName = "people";

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
        public ApiPeople(IApiConnector apiConnector)
            : base(ControllerName, apiConnector)
        { }

        #region Post-Create methods

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Sends an Http request with a PeopleQueryFilterDto that 
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
        /// <param name="peopleDto">The DTO that encapsulates property values
        /// that can be directly related to one or more properties of a type
        /// Person.
        /// </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the collection of items successfully retrieved from the
        /// database. 
        /// </returns>
        async Task<IEnumerable<Person>> IPeople.FilterAsync(
            PeopleQueryFilterDto peopleDto)
        {
            try
            {
                /// The Application/Server-Api/Controllers/PersonController 
                /// decorates its FilterPeopleTask action (method) with an
                /// HttpGet route template that includes a "filter" route
                /// segment.
                ///
                /// It also includes a [FromQuery] binding source attribute
                /// which means we need to provide its parameter values
                /// (PeopleQueryFilterDto values) in the form of a query
                /// string.
                ///
                /// The ApiConnector class responsible for building the URL
                /// for the HTTP request includes the "filter" route segment
                /// to indicate .Net Core routing middleware to dispatch the
                /// HTTP request to the action in the PersonController that
                /// matches.
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement =
                    "/filter?" +
                    $"id={peopleDto.Id}" +
                    $"&name={peopleDto.Name}" +
                    $"&moviecharactername={peopleDto.MovieCharacterName}";

                /// Consumes an Api <em>resource method</em> with the
                /// details of building an Http GET request for the 
                /// appropriate endpoint (URI).
                IEnumerable<Person> personItems =
                    await ApiConnector.InvokeGetAsync<IEnumerable<Person>>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.OmitJWTs);

                return personItems;
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
        /// Sends an Http request with a PaginationRequestDto that
        /// encapsulates the pagination parameters for the required data.
        /// </summary>
        /// <remarks>
        /// Non-generic version to serve paginated query results is <strong>
        /// NOT USED</strong> because it was replaced with the generic
        /// ApiRepository.GetPaginatedAsync() method.
        /// </remarks>
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page. </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the segment of Person objects retrieved from the database
        /// and the description and context of the paginated data (metadata).
        /// </returns>
        async Task<PaginatedResponseDto<IEnumerable<Person>>>
            IPeople.GetPeoplePaginatedAsync(
                PaginationRequestDto paginationRequestDto)
        {
            try
            {
                /// The Application/Server-Api/Controllers/PeopleController 
                /// decorates the formal input parameter of the
                /// GetPeoplePaginatedTask action (method) with a [FromQuery]
                /// binding source attribute which means we need to provide
                /// its parameter values (PaginationRequestDto values) in the
                /// form of a query string.
                ///
                /// The ApiConnector class responsible for building the URL
                /// for the HTTP request includes these values when building
                /// the absolute URL that matches the route template for the
                /// desired controller action. 
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement =
                    "?" +
                    $"pagenumber={paginationRequestDto.PageNumber}" +
                    $"&recordsperpage={paginationRequestDto.RecordsPerPage}";

                PaginatedResponseDto<IEnumerable<Person>> paginatedResponseDto =
                    await ApiConnector
                        .InvokeGetAsync<PaginatedResponseDto<IEnumerable<Person>>>(
                            ControllerName,
                            routeTemplateComplement,
                            jwtOptions: JwtOptions.OmitJWTs);

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

        #endregion

        #region Put-Update methods

        #endregion

        #region Delete methods

        /// <summary>
        /// Sends an Http request to delete a Person object from the
        /// database.
        /// </summary>
        /// <remarks>
        /// The root entity type Person is decorated with the custom
        /// "IsAuditable" attribute and an IsDeletable formal input
        /// parameter; i.e., it implements soft-delete.
        /// </remarks>
        /// <param name="personId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the Person object successfully deleted or null if
        /// unsuccessful.
        /// </returns>
        async Task<Person?>? IPeople.DeletePersonAsync(int personId)
        {
            try
            {
                /// Converts a string to an Html encoded string.
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = $"/{personId}";

                Person deletedPerson =
                    await ApiConnector.InvokeDeleteAsync<Person>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.IncludeJWTs);

                return deletedPerson;
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
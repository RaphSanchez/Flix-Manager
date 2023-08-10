using System.Security.Claims;
using System.Text;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Shared.AuthZHelpers;
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
    internal class ApiUsers : ApiRepository<ApplicationUser>, IUsers
    {
        /// <summary>
        /// The name of the Application/Sever-Api/Controller of the
        /// resource (data entity).
        /// </summary>
        private static readonly string _controllerName = "users";

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
        public ApiUsers(IApiConnector apiConnector)
            : base(_controllerName, apiConnector)
        { }

        #region Post-Create methods


        #endregion

        #region Get-Read methods

        /// <summary>
        /// Sends an Http request with a PaginationRequestDto that
        /// encapsulates the pagination parameters for the required data.
        /// </summary>
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page. </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the segment of Person objects retrieved from the database
        /// and the description and context of the paginated data (metadata).
        /// </returns>
        async Task<PaginatedResponseDto<IEnumerable<UserDto>>?> IUsers
            .GetPaginatedUsersAsync(
                PaginationRequestDto paginationRequestDto)
        {
            try
            {
                /// The Application/Server-Api/Controllers/UsersController 
                /// decorates the formal input parameter of the
                /// GetPaginatedUsersAsync action (method) with a [FromQuery]
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
                    $"pagenumber" +
                    $"={paginationRequestDto.PageNumber}" +
                    $"&recordsperpage={paginationRequestDto.RecordsPerPage}";

                PaginatedResponseDto<IEnumerable<UserDto>>? paginatedResponseDto =
                    await ApiConnector
                        .InvokeGetAsync<PaginatedResponseDto<IEnumerable<UserDto>>>(
                            _controllerName,
                            routeTemplateComplement,
                            jwtOptions: JwtOptions.IncludeJWTs);

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
        /// Sends an Http request to retrieve a collection of all the custom
        /// <see cref="AuthZClaim"/> items available for controlling access to
        /// application resources. Authorization claims assigned to the user
        /// passed to satisfy its formal input parameter are marked as
        /// selected.
        /// </summary>
        /// <param name="userId">The Id of the application user to work with.
        /// </param>
        /// <returns>The deserialized JSON content from the HTTP response
        /// message; i.e., the object values that represents the collection of
        /// customs authorization <see cref="Claim"/> items available to
        /// control access to application resources.</returns>
        async Task<UserClaimsDto?> IUsers.GetUserAuthZClaimsAsync(string userId)
        {
            try
            {
                /// The Application/Server-Api/Controllers/UsersController 
                /// decorates its GetUserAuthZClaimsTask action (method)
                /// with an [HttpGet] route template that includes an
                /// "user-claims" route segment.
                ///
                /// The ApiConnector class responsible for building the URL for
                /// the HTTP request includes the the route segment to indicate
                /// .Net Core routing middleware to dispatch the HTTP request
                /// to the the action in the UsersController that matches.
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = "/user-claims" +
                                                 $"?userId={userId}";

                UserClaimsDto? userClaimsDto =
                    await ApiConnector.InvokeGetAsync<UserClaimsDto>(
                        _controllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.IncludeJWTs);

                return userClaimsDto;
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
        /// Sends an Http request to query the data store for a given
        /// <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="userId">The Id of the application user to find.</param>
        /// <returns>The deserialized JSON content from the HTTP response
        /// message; i.e., the object value that represents the application
        /// User Id and Email.</returns>
        async Task<UserDto?> IUsers.GetUserAsync(string userId)
        {
            try
            {
                string routeTemplateComplement = "/get-user" +
                                                 $"?userId={userId}";

                UserDto? userDto =
                    await ApiConnector.InvokeGetAsync<UserDto>(
                        _controllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.IncludeJWTs);

                return userDto;
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

        /// <summary>
        /// Sends an Http request to update the custom authorization claims of
        /// a given User.
        /// </summary>
        /// <param name="userClaimsDto">A type that represents an
        /// <see cref="ApplicationUser"/>.Id and a collection of
        /// <see cref="AuthZClaimDto"/> items.</param>
        /// <returns>An ActionResult that represents the result of the action
        /// method.</returns>
        /// <param name="userClaimsDto">A type that represents an
        /// <see cref="ApplicationUser"/>.Id and a collection of
        /// <see cref="AuthZClaimDto"/> items.</param>
        /// <returns>The deserialized JSON content from the HTTP response
        /// message; i.e., the object value type <see cref="bool"/> that
        /// represents the result of the operation.</returns>
        async Task<bool> IUsers.UpdateUserClaimsAsync(
            UserClaimsDto? userClaimsDto)
        {
            try
            {
                /// The Application/Server-Api/Controllers/UsersController 
                /// decorates its AssignAuthZClaimsTask action (method)
                /// with an [HttpPost] route template that includes an
                /// "assing-claims" route segment.
                ///
                /// The ApiConnector class responsible for building the URL for
                /// the HTTP request includes the the route segment to indicate
                /// .Net Core routing middleware to dispatch the HTTP request
                /// to the the action in the UsersController that matches.
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = "/update-claims";

                /// Consumes an Api resource method with the details of
                /// building an Http POST request for the appropriate endpoint
                /// (controller's route template).
                bool result = await ApiConnector
                    .InvokePutAsync<UserClaimsDto, bool>(
                        userClaimsDto,
                        _controllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.IncludeJWTs);

                return result;
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

        #endregion
    }
}


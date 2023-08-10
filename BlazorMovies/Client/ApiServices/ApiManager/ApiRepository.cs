using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

/// <summary>
/// The application architecture has an Application/Client (UI)
/// project that employs:
/// 
/// 1. An abstract layer Application/Client/ApiServices/IApiService 
/// responsible for encapsulating the details of how to invoke 
/// Server-WebApi endpoints (send/receive Http requests/responses).
/// 
/// 2. An abstract layer Application/Client/IEFManager/IUnitOfWork
/// responsible for encapsulating the business logic of the
/// application and the details of how to communicate to the
/// database. 
/// 
/// The Application/Client makes a data request, the IApiService
/// sends the HttpRequest to the Application/Server-Api/Controllers.
/// 
/// The Application/Server-Api controller employs the
/// UnitOfWork business logic methods to 
/// query the Application/Server-Api DataStore database and, if necessary,
/// persist any changes made to in-memory objects. 
/// </summary>
/// <remarks>
/// Both abstract layers implement the Repository pattern to expose
/// to the client their higher level interfaces and hide the actual 
/// implementation.
/// <para>
/// The operations exposed to the Client (by the IApiService) mirror
/// (same signature) the operations executed by the IUnitOfWork 
/// (business logic and database operations).
/// 
/// For this reason, both abstract layers employ the IEntityName 
/// interfaces to expose and represent data entities which in turn
/// implement a single (unique)  
/// IRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
/// interface which establishes a contract for the required general
/// functionality compatible to all data entities. 
/// </para>
/// <para>
/// IEntityName interfaces not only implement 
/// IRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>,
/// they also extend their functionality with operations that are
/// specific to the entity type passed to satisfy the type parameter.
/// </para>
/// <para>
/// IEntityName interfaces are exposed, and implemented in their
/// own way, by the IApiService and IUnitOfWork interfaces. This means 
/// they need a "public" access modifier.
/// </para>
/// https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ff649690(v=pandp.10)?redirectedfrom=MSDN
/// https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
/// </remarks>

namespace BlazorMovies.Client.ApiServices.ApiManager
{
    /// <summary>
    /// Implements
    /// IRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
    /// interface with methods that map Application/Client requests to 
    /// Application/Server-Api/Controllers endpoints. It encapsulates
    /// general functionality methods that are applicable to all data 
    /// entities.
    /// </summary>
    /// <remarks>
    /// These methods have an equivalent implementation in the 
    /// EfRepository class which also implements the 
    /// IRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
    /// interface to perform these operations into the database. 
    /// <para>
    /// It employs an instance of the IApiConnector interface that
    /// builds the URIs (routes) for the required resource (controller
    /// endpoint) and encapsulates the logic (resource methods) for
    /// sending/receiving Http requests/responses. This logic can be 
    /// easily replaced by configuring a service in the
    /// Application/Client/Program.cs class with a different 
    /// implementation (class) of the IApiConnector interface. 
    /// </para>
    /// <para>
    /// The parameters for the constructor of this generic base class
    /// are satisfied by any derived class which is not generic (e.g.,
    /// ApiGenres) because it defines more specific functionality.
    /// For this reason, it can provide the controller name.
    /// </para>
    /// <para>
    /// Its methods have an "explicit interface" implementation 
    /// to hide them from unwanted consumers. 
    /// </para>
    /// </remarks>
    internal class ApiRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// The name of the Application/Sever-Api/Controller of the 
        /// resource (data entity) of interest. Each derived class
        /// (not generic) is responsible for passing the name of the
        /// controller through its constructor. 
        /// </summary>
        private readonly string _controllerName;

        /// <summary>
        /// Represents an instance of the class that implements the 
        /// IApiConnector interface responsible for building the URI 
        /// to map to the appropriate 
        /// Application/Server-Api/Controllers/EntityController/Action
        /// to send/receive Http requests/responses.
        /// </summary>
        /// <remarks>
        /// It is also responsible for serializing/deserializing the
        /// content of Http requests/responses. 
        /// <para>
        /// Its "protected" access modifier makes it accessible to any
        /// derived class (e.g., ApiGenres). This structure enables using
        /// a single ApiConnector instance which in turn employs a unique
        /// HttpClient instance to avoid exhausting the web sockets. 
        /// </para>
        /// </remarks>
        protected readonly IApiConnector ApiConnector;

        /// <summary>
        /// The ApiRepository is completely decoupled from the code logic
        /// to build the routes that match the route template to Api-endpoints
        /// and to send/receive Http requests/responses. That logic depends on
        /// the configuration of the IApiConnector as a service in the
        /// Application/Client/Program class; 
        /// i.e., depends on the class that implements its functionality
        /// and can be easily replaced. 
        /// </summary>
        /// <remarks>
        /// This base class's constructor parameters are supplied by 
        /// any derived class which is not generic because it defines
        /// functionality specific to a data entity type (e.g.,
        /// ApiGenres). 
        /// </remarks>
        /// <param name="controllerName">The name of the controller of
        /// the resource (data entity) of interest.</param>
        /// <param name="apiConnector">Represents an instance of the
        /// class that implements the IApiConnector interface</param>
        public ApiRepository(string controllerName, IApiConnector apiConnector)
        {
            _controllerName = controllerName;
            ApiConnector = apiConnector;
        }

        #region Post-Create methods

        /// <summary>
        /// Sends an Http request to insert an entity to the database.
        /// </summary>
        /// <param name="newEntity">The entity to insert into the database.
        /// </param>
        /// <returns>The deserialized JSON content from the response message; 
        /// i.e., the object value successfully inserted into the database.
        /// </returns>
        async Task<TEntity?> IRepository<TEntity>.AddAsync(TEntity? newEntity)
        {
            try
            {
                TEntity? insertedEntity =
                    await ApiConnector.InvokePostAsync<TEntity>(
                        newEntity!,
                        _controllerName,
                        routeTemplateComplement: null,
                        jwtOptions: JwtOptions.IncludeJWTs);

                return insertedEntity;
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
        /// Sends an Http request to query the database for a complete set of
        /// the entity type.
        /// </summary>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the collection of items successfully retrieved from the
        /// database. 
        /// </returns>
        async Task<IEnumerable<TEntity?>> IRepository<TEntity>.GetAllAsync()
        {
            try
            {
                IEnumerable<TEntity> entities =
                    await ApiConnector.InvokeGetAsync<IEnumerable<TEntity>>(
                        _controllerName,
                        routeTemplateComplement: null,
                        jwtOptions: JwtOptions.OmitJWTs);

                return entities;
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
        /// Sends an Http request to find an entity with a primary key
        /// property value equivalent to the entity "Id" passed as argument.
        /// </summary>
        /// <param name="id">The primary key property value for querying the 
        /// database.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the entity object value with the primary key property value 
        /// passed for querying the database.</returns>
        async Task<TEntity?> IRepository<TEntity>.GetByIdAsync(int id)
        {
            try
            { 
                /// The forward slash delimiter is necessary.
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = $"/{id}";

                TEntity entity = await ApiConnector
                    .InvokeGetAsync<TEntity>(
                        _controllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.OmitJWTs);

                return entity;
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
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page. </param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the segment of database records retrieved from the database
        /// and the description and context of the paginated data (metadata).
        /// </returns>
        async Task<PaginatedResponseDto<IEnumerable<TEntity>>>
            IRepository<TEntity>.GetPaginatedAsync(
                PaginationRequestDto paginationRequestDto)
        {
            try
            {
                /// The Application/Server-Api/Controllers/EntityController
                /// should decorate the formal input parameter of the
                /// GetEntityNamePaginatedTask action (method) with a [FromQuery]
                /// binding source attribute to make it compatible with
                /// the routeTemplateComplement built here.
                ///
                /// The values for the formal input parameter
                /// (PaginationRequestDto) of the GetEntityNamePaginatedTask
                /// action are provided here in the form of a query string.
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

                PaginatedResponseDto<IEnumerable<TEntity>> paginatedResponseDto =
                    await ApiConnector
                        .InvokeGetAsync<PaginatedResponseDto<IEnumerable<TEntity>>>(
                            _controllerName,
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

        /// <summary>
        /// Sends an Http request to update the data entity. 
        /// </summary>
        /// <remarks>
        /// It updates all the properties of an entity except for the
        /// primary key property value. 
        /// </remarks>
        /// <param name="id">The primary key property value of the entity 
        /// to update.</param>
        /// <param name="entityWithNewValues">The entity with the new
        /// property values.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the entity object value with the new property values
        /// updated.
        /// </returns>
        async Task<TEntity?> IRepository<TEntity>.UpdateAsync(
            int id, TEntity entityWithNewValues)
        {
            try
            {
                /// The forward slash delimiter is necessary.
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = $"/{id}";

                TEntity? updatedEntity =
                    await ApiConnector.InvokePutAsync<TEntity>(
                    entityWithNewValues,
                    _controllerName,
                    routeTemplateComplement,
                    jwtOptions: JwtOptions.IncludeJWTs);

                return updatedEntity;
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
        /// Sends an Http request to delete the data entity.
        /// </summary>
        /// <remarks>
        /// Executes a "soft delete" for the domain entities that have
        /// been provided an "IsActive" discriminator. Otherwise, it 
        /// completely removes the entity from the database. 
        /// </remarks>
        /// <param name="id">The primary key property value of the entity
        /// to delete.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the entity object value that was successfully removed
        /// from the database.</returns>
        async Task<TEntity> IRepository<TEntity>.DeleteAsync(int id)
        {
            try
            {
                /// The forward slash delimiter is necessary.
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = $"/{id}";

                TEntity entity =
                    await ApiConnector.InvokeDeleteAsync<TEntity>(
                        _controllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.IncludeJWTs);

                return entity;
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



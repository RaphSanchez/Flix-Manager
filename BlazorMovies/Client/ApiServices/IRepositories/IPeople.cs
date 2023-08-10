using BlazorMovies.Shared.EDM;
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
namespace BlazorMovies.Client.ApiServices.IRepositories
{
    /// <summary>
    /// Entry point for the IApiService and IUnitOfWork interfaces because
    /// they expose one IEntityName interface for each data entity in the
    /// Entity Domain Model (EDM). 
    /// </summary>
    /// <remarks>
    /// Implements the
    /// IRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
    /// interface with general functionality applicable to all data
    /// entities and also extends its functionality with operations 
    /// that are specific to the entity type passed to satisfy the type
    /// parameter. 
    /// <para>
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in
    /// the result of a query with EF's "Include" extension method. 
    /// </para>
    /// </remarks>
    public interface IPeople : IRepository<Person>
    {
        #region Post-Create methods



        #endregion

        #region Get-Read methods

        /// <summary>
        /// Queries the database for items whose property values match the
        /// values extracted from the PeopleQueryFilterDto passed to satisfy
        /// its formal input parameter. 
        /// </summary>
        /// <remarks>
        /// The property values are used as filtering criteria.
        /// <para>
        /// The method is case insensitive because it employs a .ToLower()
        /// extension. 
        /// </para>
        /// </remarks>
        /// <param name="peopleQueryFilterDto">The DTO that encapsulates 
        /// property values that can be directly related to one or more 
        /// properties of a type Person.</param>
        /// <returns>An async array with Person items that matched the filtering
        /// criteria or an empty array if no matches were found.</returns>
        Task<IEnumerable<Person>> FilterAsync(
            PeopleQueryFilterDto peopleQueryFilterDto);

        /// <summary>
        /// Retrieves a sequence of Person records in segments (or portions
        /// of data) that adhere to the specifications of the
        /// PaginationRequestDto.
        /// </summary>
        /// <remarks>
        /// Non-generic version to serve paginated query results is <strong>
        /// NOT USED</strong> because it was replaced with the generic
        /// IRepository.GetPaginatedAsync() method.
        /// </remarks>
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page. </param>
        /// <returns>
        /// An object of type PaginatedResponseDto with items of type Person
        /// after paginating the result of the database query. It includes the
        /// description and context of the paginated data (metadata). 
        /// </returns>
        Task<PaginatedResponseDto<IEnumerable<Person>>> GetPeoplePaginatedAsync(
            PaginationRequestDto paginationRequestDto);

        #endregion

        #region Put-Update methods



        #endregion

        #region Delete methods

        /// <summary>
        /// Deletes a Person object including its related data.
        /// </summary>
        /// <remarks>
        /// The application implements a soft-delete mechanism using a
        /// custom IsAuditable attribute to decorate an entity to be part
        /// of the mechanism, in which case, the state of the entity will
        /// be modified from <em>EntityState.Deleted</em> to
        /// <em>EntityState.Modified</em> and its IsDeleted shadow property
        /// value will be set to <em>true</em> before persisting any changes
        /// into the database.
        /// </remarks>
        /// <param name="genreId">The primary key property value of the
        /// entity to delete.</param>
        /// <returns>The entity object that was successfully deleted.
        /// </returns>
        Task<Person?>? DeletePersonAsync(int genreId);

        #endregion
    }
}



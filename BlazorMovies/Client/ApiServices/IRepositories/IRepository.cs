using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

namespace BlazorMovies.Client.ApiServices.IRepositories
{
    /// <summary>
    /// Establishes a contract for the required general functionality
    /// applicable to all data entities (data types).
    /// <remarks>
    /// <para>
    /// The Application/Client/ApiServices/ApiManager ApiRepository.cs and
    /// the Application/Server-Api/Repositories EfRepository.cs implement
    /// the same operations with different code logic because they are at
    /// different stages of the Http request/response. Since they implement
    /// the same operations (methods), they both implement this 
    /// IRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
    /// interface in their own way.
    /// </para>
    /// <para>
    /// A repository can contain methods like Add, Remove, or Find but
    /// it should never include the semantics of a database context such
    /// as DbContext.SaveChanges(); i.e., repositories work with
    /// in-memory objects and the UnitOfWork tracks these in-memory
    /// changes to persist them to the database when the business
    /// transaction is complete. 
    /// </para>
    /// <para>
    /// A repository should NOT return 
    /// IQueryable<typeparam name="T">&lt;T&gt;</typeparam> objects
    /// because they can be further used to build new queries which is
    /// completely against the principle of the repository pattern of
    /// encapsulating queries so they cannot be repeated or abused. 
    /// </para>
    /// <para>
    /// This interface and its implementations
    /// (ApiRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
    /// and EfRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>)
    /// are  completely generic so they can be re-used with any application.
    /// </para>
    /// </remarks>
    /// </summary>
    /// <typeparam name="TEntity">The type of the data entity
    /// to work with.</typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        #region Post-Create methods

        /// <summary>
        /// Inserts an entity to the database.
        /// </summary>
        /// <param name="entity">The entity to insert into the database.
        /// </param>
        /// <returns>The entity inserted into the database.</returns>
        Task<TEntity?> AddAsync(TEntity? entity);

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Retrieves a sequence of the given entity type.
        /// </summary>
        /// <returns>An async array of the given entity type.</returns>
        Task<IEnumerable<TEntity?>> GetAllAsync();

        /// <summary>
        /// Finds an entity with a primary key value equivalent to the
        /// entityId passed. If no entity is found, null is returned. 
        /// </summary>
        /// <param name="entityId">Primary key value to use for querying
        /// the database.</param>
        /// <returns>The entity with the primary key value or null if
        /// not found.</returns>
        Task<TEntity?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves a sequence of database records in segments (or portions
        /// of data) that adhere to the specifications of the
        /// PaginationRequestDto.
        /// </summary>
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page. </param>
        /// <returns>
        /// An object of type PaginatedResponseDto with database records after
        /// paginating the result of the database query. It includes the
        /// description and context of the paginated data (metadata). 
        /// </returns>
        Task<PaginatedResponseDto<IEnumerable<TEntity>>> GetPaginatedAsync(
            PaginationRequestDto paginationRequestDto);

        #endregion

        #region Put-Update methods

        /// <summary>
        /// Updates all the properties of an entity. 
        /// </summary>
        /// <param name="entityId">The primary key value of the entity to update.
        /// </param>
        /// <param name="entityWithNewValues">The entity with the new property
        /// values.</param>
        /// <returns>The updated entity with the new property values.</returns>
        Task<TEntity?> UpdateAsync(int entityId, TEntity entityWithNewValues);
        
        #endregion

        #region Delete methods

        /// <summary>
        /// Executes a "soft delete" for the domain entities that have
        /// been provided an "IsActive" discriminator. Otherwise, it
        /// completely removes the entity from the database. 
        /// </summary>
        /// <param name="entityId">The primary key property value of 
        /// the entity to delete.</param>
        /// <returns>The entity object that was successfully deleted.
        /// </returns>
        Task<TEntity> DeleteAsync(int id);

        #endregion
    }
}


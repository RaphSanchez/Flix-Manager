using System.Linq.Expressions;
using System.Reflection;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.Helpers;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
namespace BlazorMovies.Server.Repositories
{
    /// <summary>
    /// Implements
    /// IRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam> with
    /// methods that map Application/Server-Api/Controller requests to
    /// database operations through the DbContext derived class that resides
    /// in the Application/Server-Api/DataStore directory. It encapsulates the
    /// business logic that is applicable to all data entities.
    /// </summary>
    /// <remarks>
    /// A repository can contain methods like Add, Remove, or Find but it
    /// should <strong>never include</strong> the semantics of a database
    /// context such as DbContext.SaveChangesAsync(); i.e., repositories work
    /// with in-memory objects and the Unit Of Work tracks these in-memory
    /// changes to persist them to the database when the business transaction
    /// is complete.
    /// <para>
    /// A repository should <strong>not</strong> return
    /// IQueryable<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
    /// objects because they can be further used to build new queries which
    /// is completely against the principle of the repository pattern of
    /// encapsulating queries so they cannot be repeated or abused.
    /// </para>
    /// <para>
    /// This class is completely generic because it is decoupled from the type
    /// of persistence framework. It employs a DbContext to query or
    /// manipulate data and that context can represent any type of Database
    /// Management System (DBMS); e.g., SqlServer, MySql, Oracle, etc.
    /// </para>
    /// <para>
    /// TEntity represents the <strong>type</strong> of an in-memory
    /// collection and each TEntity type is mapped to a table in the database.
    /// These collections are constructed using the
    /// DbContext.Set<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>()
    /// method.
    /// </para>
    /// <para>
    /// Its <em>internal</em> access modifier makes it available only to
    /// elements that reside in the same assembly (project):
    /// Application/Server-Api.
    /// </para>
    /// <para>
    /// Its methods have an <em>explicit interface implementation</em> to hide
    /// them from unwanted consumers.
    /// </para>
    /// <para>
    /// It does not have an exception handling mechanism (try-catch blocks)
    /// because exceptions propagate up the stack until a catch statement for
    /// the exception is found. The Application/Server-Api/Controllers
    /// controller that calls method(s) in this repository has an exception
    /// handling mechanism. 
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntity">The type of the data entity to work with.
    /// </typeparam>
    internal class EfRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// Represents a session with the database. It has a "protected"
        /// access modifier because any classes that derive from this
        /// EfRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
        /// class (e.g., EfGenre) consume it.
        /// <remarks>
        /// Each derived class will employ this unique DbContext instance
        /// that represents a single session with the database. This
        /// architecture allows a single business transaction (or Unit of
        /// Work) to have multiple operations with different entity types
        /// before persisting the changes to the database.
        /// <para>
        /// The read-only modifier indicates that the assignment to the
        /// field can only occur as part of the declaration or in the
        /// constructor of the same class. 
        /// </para>
        /// </remarks>
        /// </summary>
        protected readonly DbContext Context;

        /// <summary>
        /// Encapsulates all Http-specific information about an individual
        /// HTTP request-response.
        /// </summary>
        protected readonly IHttpContextAccessor HttpContextAccessor;

        /// <summary>
        /// A DbSet<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
        /// represents a type (in-memory object) that was included in the
        /// model mapped to a database table. It is an in-memory
        /// representation of a collection of items of a given type which
        /// can be used to perform CRUD operations on the database.
        /// </summary>
        /// <remarks>
        /// The Application/DataStore/AppDbContext.cs derives from
        /// DbContext which is the  main class that coordinates Entity
        /// Framework functionality. 
        /// <para>
        /// The AppDbContext class exposes a
        /// DbSet<typeparam name="T">&lt;T&gt;</typeparam> for each type
        /// (data entity) to include in the model to be mapped to a database
        /// table. 
        /// </para>
        /// </remarks>
        private readonly DbSet<TEntity> _dbSet;

        /// <summary>
        /// The PrimaryKey.Name value of the CLR type for
        /// <typeparamref name="TEntity"/>; i.e., the name of the primary
        /// key for the type of TEntity at runtime.
        /// </summary>
        private readonly string? _primaryKeyName;

        /// <summary>
        /// Initialization of the DbContext instance. It has nothing to do
        /// with the application's specific context; i.e., it is not the
        /// AppDbContext custom class. 
        /// </summary>
        /// <param name="context">The DbContext derived class to employ for
        /// querying the database and persisting any modifications.</param>
        /// <param name="httpContextAccessor">The IHttpContextAccessor instance
        /// to employ to have access to the specific Http specific information
        /// about this individual Http request-response (business transaction).
        /// </param>
        public EfRepository(
            DbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            Context = context;

            /// The DbContext.Set<TEntity> method returns a DbSet<TEntity>
            /// instance that can be used to perform in-memory operations
            /// whith CLR objects such as querying or saving.
            /// 
            /// The EfRepository<TEntity> class expects a DbContext type that
            /// can represent any type of Database Management System (DBMS). 
            /// This is why it is required to produce a DbSet<TEntity>;
            /// i.e., produce an in-memory representation of a collection
            /// of a given type. 
            _dbSet = Context.Set<TEntity>();

            HttpContextAccessor = httpContextAccessor;

            /// Obtains the PrimaryKey.Name value of the CLR type for TEntity.
            _primaryKeyName = GetPrimaryKeyName();
        }

        #region Post-Create methods

        /// <summary>
        /// Inserts an entity to the database. It does not include any
        /// related data (data entities). 
        /// </summary>
        /// <param name="newEntity">The entity to insert into the database.
        /// </param>
        /// <returns>The entity inserted into the database.</returns>
        async Task<TEntity?> IRepository<TEntity>.AddAsync(TEntity? newEntity)
        {
            /// The generic implementation allows re-using the code logic with
            /// any application. 
            EntityEntry<TEntity> result = await _dbSet
                .AddAsync(newEntity!);
            return result.Entity;
        }

        /// <summary>
        /// Inserts a range of entities to the database. It does not include
        /// any related data (data entities).
        /// </summary>
        /// <param name="entities">The sequence that contains the entities
        /// to insert into the database.</param>
        /// <returns>A task that represents the asynchronous operation.
        /// </returns>
        async Task AddRangeAsync(IEnumerable<TEntity?> entities)
        {
            await _dbSet
                    .AddRangeAsync(entities!);
        }

        #endregion

        #region Get-Read methods 

        /// <summary>
        /// Queries the database for a complete set of the entity type
        /// used to invoke this method. It does not include any related
        /// data (data entities).
        /// </summary>
        /// <returns>An async array of the given entity type.</returns>
        async Task<IEnumerable<TEntity?>> IRepository<TEntity>.GetAllAsync()
        {
            return await _dbSet.ToArrayAsync();
        }

        /// <summary>
        /// Finds an entity with a primary key value equivalent to the
        /// entityId passed as argument. If no entity is found, null is 
        /// returned. It does not include (or load) any related data
        /// (data entities).
        /// </summary>
        /// <param name="entityId">Primary key value to use for querying
        /// the database.</param>
        /// <returns>The entity with the primary key value or null if
        /// not found. It does not include any related data.</returns>
        async Task<TEntity?> IRepository<TEntity>.GetByIdAsync(int entityId)
        {
            return await _dbSet.FindAsync(entityId);
        }

        /// <summary>
        /// Extracts the only element that satisfies a condition. It
        /// does not include any related data (data entities).
        /// </summary>
        /// <param name="predicate">The predicate delegate to filer the
        /// sequence.
        /// </param>
        /// <returns>The only element of a sequence that satisfies the
        /// specified condition or a default value is no such element
        /// exists. Throws an exception is more than one element satisfies
        /// the condition.
        /// </returns>
        async Task<TEntity?> FindSingleOrDefaultAsync(
            Expression<Func<TEntity?,
                bool>> predicate)
        {
            return await _dbSet
                    .SingleOrDefaultAsync(predicate);
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate delegate.
        /// It does not include any related data (data entities).
        /// </summary>
        /// <param name="predicate">The predicate delegate to filter the
        /// sequence.
        /// </param>
        /// <returns>An async array that contains the elements that satisfy
        /// the condition set with the predicate delegate.</returns>
        async Task<IEnumerable<TEntity?>> FindAllAsync(
            Expression<Func<TEntity?,
            bool>> predicate)
        {
            return await _dbSet
                    .Where(predicate)
                    .ToArrayAsync();
        }

        /// <summary>
        /// Retrieves a sequence of database records in segments (or portions
        /// of data) that adhere to the specifications of the
        /// PaginationRequestDto. 
        /// </summary>
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page.</param>
        /// <returns>
        /// An object of type PaginatedResponseDto with database records after
        /// paginating the result of the database query. It includes the
        /// description and context of the paginated data (metadata). 
        /// </returns>
        /// <exception cref="ArgumentNullException">Exception thrown if the
        /// argument to satisfy its formal input parameter is null.
        /// </exception>
        async Task<PaginatedResponseDto<IEnumerable<TEntity>>> IRepository<TEntity>
            .GetPaginatedAsync(PaginationRequestDto paginationRequestDto)
        {
            if (paginationRequestDto is null)
                throw new ArgumentNullException(nameof(paginationRequestDto),
                    $"EfPeople {nameof(paginationRequestDto)} cannot be null.");

            /// IQueryable<TEntity> and IEnumerable<TEntity> execute until they
            /// are consumed. However, IEnumerable cannot compose 'Expressions'
            /// and store them in an 'Expression Tree' to consume; i.e., with
            /// IEnumerable the system loads the results into memory for each
            /// query (or filter). 
            /// https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-6.0#iqueryable-vs-ienumerable
            IQueryable<TEntity> query = _dbSet;

            /// The "Key" value for the custom HttpHeader to add to the
            /// HttpResponse with the pagination metadata. 
            const string metadataHttpHeaderTitle = "pagination-metadata";

            /// Custom extension method for IHttpContextAccessor interface
            /// formulates, serializes, and inserts the pagination metadata
            /// into the Http response in the form of a custom header.
            ///
            /// Metadata values must be captured before paginating the
            /// results. Otherwise the captured values (e.g.,
            /// totalExistingRecords) are based on the paginated results,
            /// not on the data source. 
            PaginationMetadata metadata = await HttpContextAccessor
                .InsertPaginationMetadataInResponse(
                    query,
                    paginationRequestDto,
                    metadataHttpHeaderTitle);

            /// If sorting the collection in any shape or form is a must,
            /// regardless of the pagination method used, always make sure
            /// that the ordering is fully unique. If there can be multiple
            /// results with the same ordering value, then results could be
            /// skipped when paginating as they are ordered differently
            /// accross two paginating queries.
            /// https://docs.microsoft.com/en-us/ef/core/querying/pagination
            /// 
            /// This method executes sorting for illustrative purposes only.
            /// It is a primary ordering based on the object's unique primary
            /// Id to ensure consistent ordering on every query result. Subsequent
            /// sorting should be performed in the client layer.
            ///
            /// Note that the order of Linq methods matter; i.e., sorting the
            /// collection should be executed before paginating the query
            /// results. What's more, in
            /// this example, the whole query result is traversed to order the
            /// elements based on their primary key. If the collection has a
            /// lot of data, it can become a performance issue. You should consider
            /// using PLinq.
            /// https://stackoverflow.com/questions/7499384/does-the-order-of-linq-functions-matter
            if (_primaryKeyName is not null)
            {
                query = query
                    .OrderBy(x => _primaryKeyName);
            }

            /// Custom extension method for IQueryable<T> interface determines
            /// which objects to include in the query result based on the
            /// expected number of records per page and the page number
            /// requesting the data.
            query = query.Paginate(paginationRequestDto);

            /// Paginated response that includes the actual data and its metadata
            /// (description and context). 
            PaginatedResponseDto<IEnumerable<TEntity>> paginatedResponseDto =
                new(query, metadata);

            #region How to deserialize Http Header content

            /// Captures the "string values" stored in the custom Http header
            /// with a Key: "pagination-metadata". 
            //string jsonPaginationMetadata = HttpContextAccessor.HttpContext.
            //    Response.Headers[metadataHttpHeaderTitle];

            /// Parses the text representing a single JSON value into an
            /// instance of the type specified by a generic type parameter.
            /// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to?pivots=dotnet-6-0
            //PaginationMetadata? deserializedMetadata = JsonSerializer
            //    .Deserialize<PaginationMetadata>(jsonPaginationMetadata);

            #endregion

            return paginatedResponseDto;
        }

        #endregion

        #region Put-Update methods

        /// <summary>
        /// Updates the properties of a root entity. It <strong>does
        /// not update</strong> relationship (navigation) properties
        /// because it does not include any related data (data entities).
        /// </summary>
        /// <remarks>
        /// There is a .Net library named <see href="https://automapper.org/">
        /// AutoMapper</see> built to map one object to another. For the most
        /// part, it makes things much simpler because it handles the mapping
        /// automatically. Nevertheless, I prefer doing my mapping manually to
        /// have an exact control of what goes where.
        /// <para>
        /// "Episode 76. Updating People" of the Programming in Blazor -
        /// ASP.Net Core 5 course in
        /// <see href="https://www.udemy.com/share/102l0i3@vvD3iCwvDBzUqrFpZILkPYAUbCLeMg6JJFb7ppSQbTmXv2K0y0Ham4u3FaGAmdR9/">
        /// Udemy
        /// </see> by Felipe Gavilan, and
        /// Episodes "39. What is AutoMapper and using it in ASP Net Core"
        /// and "40. Edit and Update in Blazor" of the
        /// <see href="https://youtube.com/playlist?list=PL6n9fhu94yhVowClAs8-6nYnfsOTma14P">
        /// Blazor tutorial for beginners
        /// </see> by Kudvenkat, have an example on how to download and
        /// implement Automapper.
        /// </para>
        /// </remarks>
        /// <param name="entityId">The primary key value of the entity
        /// to update.
        /// </param>
        /// <param name="entityWithNewValues">The entity with the new
        /// property values.
        /// </param>
        /// <returns>The updated entity with the new property values.
        /// </returns>
        async Task<TEntity?> IRepository<TEntity>.UpdateAsync(
            int entityId, TEntity entityWithNewValues)
        {
            TEntity? localEntityWithNewValues = entityWithNewValues;

            TEntity? entityToUpdate = await _dbSet
                .FindAsync(entityId);

            if (entityToUpdate != null)
            {
                foreach (PropertyInfo tEntityProperty in
                         typeof(TEntity).GetProperties())
                {
                    /// Jumps out of the iteration if the current
                    /// tEntityProperty name is the primary key.
                    ///
                    /// Avoids overwriting the primary key and also
                    /// optimizes performance because avoids unnecessary
                    /// iterations.
                    if (tEntityProperty.Name
                        .Equals(_primaryKeyName, StringComparison.Ordinal))
                        continue;

                    foreach (PropertyInfo newValueProperty in
                             localEntityWithNewValues.GetType().GetProperties())
                    {
                        /// Jumps out of the iteration if the current
                        /// property name is the primary key.
                        ///
                        /// Avoids overwriting the primary key and also
                        /// optimizes performance because avoids unnecessary
                        /// iterations.
                        if (newValueProperty.Name
                            .Equals(_primaryKeyName, StringComparison.Ordinal))
                            continue;

                        /// Prevents overposting by updating only matched
                        /// property types.
                        ///
                        /// It validates if property to update can be written
                        /// to. E.g., Movie.TitleSummary is a read-only property
                        /// and would throw an exception. 
                        if (tEntityProperty.PropertyType
                            == newValueProperty.PropertyType
                            && tEntityProperty.Name
                                .Equals(newValueProperty.Name,
                                    StringComparison.Ordinal)
                            && tEntityProperty.CanWrite)
                        {
                            entityToUpdate.GetType()?
                                .GetProperty(tEntityProperty.Name)?
                                .SetValue(entityToUpdate,
                                    newValueProperty
                                        .GetValue(localEntityWithNewValues));
                        }
                    }
                }

                return entityToUpdate;
            }

            return null;
        }

        #endregion

        #region Delete Methods 

        /// <summary>
        /// It begins tracking the given entity with a
        /// <em>EntityState.Deleted</em> state such that it will be
        /// removed from the database when the DbContext.SaveChanges()
        /// is called. It does <strong>not</strong> include any related
        /// data (entities). 
        /// </summary>
        /// <remarks>
        /// The application implements a soft delete mechanism using
        /// an IsAuditable attribute to decorate an entity to be part
        /// of the mechanism in which case, the state of the entity
        /// will be modified from <em>EntityState.Deleted</em> to
        /// <em>EntityState.Modified</em> and its IsDeleted shadow
        /// property will be set to "true" before persisting changes
        /// to the database. 
        /// </remarks>
        /// <param name="entityId">The primary key property value of 
        /// the entity to delete.</param>
        /// <returns>The entity object that was successfully deleted.
        /// </returns>
        async Task<TEntity> IRepository<TEntity>.DeleteAsync(int entityId)
        {
            TEntity? entityToRemove = await _dbSet
                    .FindAsync(entityId);

            if (entityToRemove != null)
            {
                _dbSet.Remove(entityToRemove);
                return entityToRemove;
            }
            return null!;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Obtains the PrimaryKey.Name value of the CLR type for
        /// <typeparamref name="TEntity"/>.
        /// </summary>
        /// <remarks>
        /// For more info visit 
        /// <see href="https://stackoverflow.com/questions/30688909/how-to-get-primary-key-value-with-entity-framework-core">
        /// How to get primary key value 
        /// </see> and
        /// <see href="https://stackoverflow.com/questions/47098782/how-to-get-primary-keys">
        /// How to get primary keys
        /// </see>.
        /// </remarks>
        /// <returns>The PrimaryKey.Name of the CLR type for
        /// <typeparamref name="TEntity"/>.
        /// </returns>
        private string? GetPrimaryKeyName()
        {
            /// https://stackoverflow.com/questions/30688909/how-to-get-primary-key-value-with-entity-framework-core
            string? primaryKeyName = Context.Model
                .FindEntityType(typeof(TEntity))?
                .FindPrimaryKey()?
                .Properties
                .Select(key => key.Name)
                .SingleOrDefault();

            return primaryKeyName;
        }

        #endregion
    }
}

// https://stackoverflow.com/questions/30688909/how-to-get-primary-key-value-with-entity-framework-core
//string primaryKeyName = AppDbContext.Model
//    .FindEntityType(typeof(TEntity))
//    .FindPrimaryKey()
//    .Properties
//    .Select(key => key.Name)
//    .SingleOrDefault();

//int entityToRemovePrimaryKeyValue = (int)entity
//    .GetType()
//    .GetProperty(primaryKeyName)
//    .GetValue(entity, null);

//TEntity dataSourceMatchedEntity = await AppDbContext.Set<TEntity>()
//    .FirstOrDefaultAsync(e => e.GetType()
//    .GetProperty(primaryKeyName)
//    .GetValue(entity, null)
//    .Equals(entityToRemovePrimaryKeyValue));

/// Does not need the DatabaseContext:
// https://stackoverflow.com/questions/47098782/how-to-get-primary-keys
//string primaryKeyName = string.Empty;
//PropertyInfo[] properties = typeof(TEntity).GetProperties();

//foreach (PropertyInfo property in properties)
//{

//    IEnumerable<Attribute> keyAttribute = property
//        .GetCustomAttributes(typeof(KeyAttribute));
//    if (!keyAttribute.Any()) continue;

//    primaryKeyName = property.Name;
//    break;
//}



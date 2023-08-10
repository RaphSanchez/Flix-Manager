using System.Reflection;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.DataStore;
using BlazorMovies.Server.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BlazorMovies.Server.Repositories
{
    /// <summary>
    /// One application specific EfEntityName class for each 
    /// IEntityName interface exposed in the IUnitOfWork interface.
    /// </summary>
    /// <remarks>
    /// It is a subclass of the
    /// EfRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
    /// class which means it inherits its general functionality 
    /// applicable to all entities. 
    /// <para>
    /// This class is application specific and extends its base class
    /// with specific functionality for the type passed as type parameter.
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in 
    /// the result of a query with EF's "Include" extension method.
    /// </para>
    /// <para>
    /// Its "internal" access modifier makes it available only to elements
    /// that reside in the same assembly (project): Application/Server-Api.
    /// </para>
    /// <para>
    /// Its methods have an "explicit interface implementation" to hide
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
    internal class EfPeople : EfRepository<Person>, IPeople
    {
        /// <summary>
        /// Property designed to have access to the unique DbContext inherited
        /// from its generic Repository parent class. This technique allows you 
        /// to use the property for querying the DB instead of having to make
        /// an explicit conversion on each query (on each method). 
        /// </summary>
        /// <remarks>
        /// 'Context' is a read-only protected field inherited from the parent
        /// class. This derived class employs its base class's DbContext
        /// instance as opposed to storing its constructor argument in a local
        /// variable to consume it. 
        /// <para>
        /// Every EfEntityName class that derives from the generic 
        /// EfRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
        /// class employs the base class's DbContext protected field which is
        /// unique and represents a single session with the database that can
        /// have multiple operations with different entity types in a single
        /// business transaction.
        /// </para>
        /// </remarks>
        private AppDbContext? AppDbContext => Context as AppDbContext;

        /// <summary>
        /// Its formal input parameter (DbContext) is not stored in a local
        /// variable because it is not consumed like that. Instead, it is
        /// passed to satisfy its base class's constructor (EfRepository) and
        /// it is the parent class which consumes it and also makes it available
        /// to any child class through a field named "Context" with a "read-only
        /// protected" access modifier.
        /// </summary>
        /// <remarks>
        /// It is the same case for its IHttpContextAccessor. Its base class
        /// (EfRepository) makes the IHttpContextAccessor available through
        /// a field named "HttpContextAccessor" with a "read-only protected"
        /// access modifier.
        /// </remarks>
        /// <param name="context">The application specific DbContext that
        /// represents a session with the database.</param>
        /// <param name="httpContextAccessor">The IHttpContextAccessor
        /// instance that provides access to the Http specific information
        /// about this individual Http request-response (business transaction).
        /// </param>
        public EfPeople(DbContext context, IHttpContextAccessor httpContextAccessor)
                : base(context, httpContextAccessor)
        { }

        #region Post-Create methods

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Queries the database for items whose property values match the
        /// values extracted from the PeopleQueryFilterDto passed in the
        /// query string.
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
        /// Person.</param>
        /// <returns>An async array with Person items that matched the filtering
        /// criteria or an empty array if no matches were found.</returns>
        async Task<IEnumerable<Person>> IPeople.FilterAsync(
            PeopleQueryFilterDto peopleDto)
        {
            /// <remarks>
            /// Always strive for creating efficient queries because they can
            /// have a significant impact in the applicaiton performance. Test
            /// databases usually have a small sample of data and problems may
            /// arise with real world applications with considerably larger
            /// amounts of data. 
            /// https://docs.microsoft.com/en-us/ef/core/performance/efficient-querying#beware-of-lazy-loading
            /// </remarks>

            /// <remarks>
            /// Adding a "StringComparison.CurrentCultureIgnoreCase throws
            /// an exception because at this point, EF core fails to translate
            /// the sort rule into an SQL command.
            ///
            /// The method is case insensitive by default because the
            /// String.Contains() method is run on the database not in the
            /// CSharp code. The case sensitivity on the query depends on the
            /// database and the collation. On SQL Server, Contains maps to
            /// SQL LIKE which is case insensitive. In SQLite, with the
            /// default collation, it is case sensitive.
            /// https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/search?view=aspnetcore-6.0
            /// https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-6.0#iqueryable-vs-ienumerable
            /// </remarks>

            /// Conditional skips unnecessary work if search criteria parameter
            /// is null.
            ///
            /// The application employs a TestCollectionNullOrEmpty component
            /// to render collections of items to the client. This component
            /// has RenderFragment parameters for situations where a collection
            /// is either "null" or "empty". In any case, the component expects
            /// a collection of items (empty or not). Otherwise, it will most
            /// likely throw an exception. 
            if (peopleDto is null) return Array.Empty<Person>();

            /// Prevents the code logic from executing in case there is an error
            /// with the query string. For example, if the query string has a
            /// spelling error in one or more of its parameter values, this
            /// FilterAsync() method would end up returning the query result
            /// without any filters.
            ///
            /// The App/Server/Controllers/PeopleController/FilterPeopleTask
            /// action handles this exception.
            if (!peopleDto.Id.HasValue &&
                peopleDto.Name is null &&
                peopleDto.MovieCharacterName is null)
                throw new InvalidFilterCriteriaException(
                    "Route template not valid for the filtering criteria.");

            /// IQueryable<typeparam name="T">&lt;T&gt;</typeparam> and 
            /// IEnumerable<typeparam name="T">&lt;T&gt;</typeparam> execute
            /// until they are consumed. However, IEnumerable cannot compose
            /// 'Expressions' and store them in an 'Expression Tree' to 
            /// consume; i.e., with IEnumerable the system loads the results
            /// into memory for each query (or filter). 
            /// https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-6.0#iqueryable-vs-ienumerable
            IQueryable<Person>? query = AppDbContext?.People;

            /// First query filter criteria is with the Person.Id value 
            /// extracted from the peopleDto.
            if (peopleDto.Id.HasValue)
                query = query?
                    .Where(p => p.Id == peopleDto.Id);

            /// Second query filter criteria is with the Person.Name value
            /// extracted from the peopleDto.
            ///
            /// Using .ToLower() could cause some issues under special
            /// circumstances. E.g., the client's collation is Turkish and
            /// your DB collation not.
            ///
            /// Whats's more, it is highly inefficient. Whenever possible
            /// you should use an "Equals" or "StartsWith" method and a
            /// StringComparison enum option. However, in our example, we
            /// want an item match with any of the letters of a given
            /// parameter.
            ///
            /// In this particular case, the method is case insensitive by
            /// default because the String.Contains() method is run on the
            /// database not in the CSharp code. The case sensitivity on the
            /// query depends on the database and the collation. On SQL Server,
            /// Contains maps to SQL LIKE which is case insensitive. In SQLite,
            /// with the default collation, it would be case sensitive.
            /// https://stackoverflow.com/questions/2431908/linq-to-entities-using-tolower-on-ntext-fields
            /// https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/search?view=aspnetcore-6.0
            if (!string.IsNullOrEmpty(peopleDto.Name))
                query = query?
                    .Where(p => p.Name
                        .ToLower()
                    .Trim()
                    .Contains(peopleDto.Name!
                            .ToLower()));

            /// Third query filter criteria is with the 
            /// Person.MovieCharacterName value extracted from the peopleDto.
            if (!string.IsNullOrEmpty(peopleDto.MovieCharacterName))
                query = query?
                    .Where(p => p.MovieCharacters!
                        .Select(mC => mC.CharacterName!
                            .ToLower())
                    .Contains(peopleDto.MovieCharacterName
                            .ToLower()));

            /// This query limits the result set size to 05 items and eager
            /// loads the Person's related data.
            /// 
            /// It also returns a no-tracking query result which is quicker
            /// to execute because it does not set up change tracking info.
            /// https://docs.microsoft.com/en-us/ef/core/querying/tracking
            query = query?
                .Include(p => p.Movies)
                .Include(p => p.MovieCharacters)
                .Take(5)
                .AsNoTracking();

            /// Creates new Person objects that do not include the Audit
            /// properties such as "CreatedOn", "UpdatedBy", etc.
            ///
            /// It includes its related data entities.
            ///
            /// Much more efficient than:
            /// query.ForEachAsync(p => new Person{...});
            query = query?
                .Select(p =>
                    new Person
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Biography = p.Biography,
                        PictureUrl = p.PictureUrl,
                        DateOfBirth = p.DateOfBirth,
                        TempCharacterName = p.TempCharacterName,
                        Movies = p.Movies,
                        MovieCharacters = p.MovieCharacters
                    });

            /// Repositories should NOT return IQueryable types. Always
            /// remember to execute the query to return the results.
            /// 
            /// The application employs a TestCollectionNullOrEmpty component
            /// to render collections of items to the client. This component
            /// has RenderFragment parameters for situations where a collection
            /// is either "null" or "empty". In any case, the component expects
            /// a collection of items (empty or not). Otherwise, it will most
            /// likely throw an exception. 
            return await query!.ToArrayAsync();
        }

        /// <summary>
        /// Retrieves a sequence of Person records in segments (or portions
        /// of data) that adhere to the specifications of the
        /// PaginationRequestDto. 
        /// </summary>
        /// <remarks>
        /// Non-generic version to serve paginated query results is <strong>
        /// NOT USED</strong> because it was replaced with the generic
        /// EfRepository.GetPaginatedAsync() method.
        /// </remarks>
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page.</param>
        /// <returns>
        /// An object of type PaginatedResponseDto with items of type Person
        /// after paginating the result of the database query. It includes the
        /// description and context of the paginated data (metadata). 
        /// </returns>
        /// <exception cref="ArgumentNullException">Exception thrown if the
        /// argument to satisfy its formal input parameter is null.
        /// </exception>
        async Task<PaginatedResponseDto<IEnumerable<Person>>> IPeople
            .GetPeoplePaginatedAsync(PaginationRequestDto paginationRequestDto)
        {
            if (paginationRequestDto is null)
                throw new ArgumentNullException(nameof(paginationRequestDto),
                    $"EfPeople {nameof(paginationRequestDto)} cannot be null.");

            /// IQueryable<Person> and IEnumerable<Person> execute until they
            /// are consumed. However, IEnumerable cannot compose 'Expressions'
            /// and store them in an 'Expression Tree' to consume; i.e., with
            /// IEnumerable the system loads the results into memory for each
            /// query (or filter). 
            /// https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-6.0#iqueryable-vs-ienumerable
            IQueryable<Person> peopleQuery = AppDbContext?.People?
                .AsQueryable()!;

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
                    peopleQuery,
                    paginationRequestDto,
                    metadataHttpHeaderTitle);

            /// Custom extension method for IQueryable<T> interface determines
            /// which objects to include in the query result based on the
            /// expected number of records per page and the page number
            /// requesting the data.
            ///
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
            /// Note that the order of Linq methods matter. What's more, in
            /// this example, the whole query result is traversed to order the
            /// elements based on their primary key. If the collection has a
            /// lot of data, it can become a performance issue. You should consider
            /// using PLinq.
            /// https://stackoverflow.com/questions/7499384/does-the-order-of-linq-functions-matter
            peopleQuery = peopleQuery
                .OrderBy(p => p.Id)
                .Paginate(paginationRequestDto);

            /// Paginated response that includes the actual data and its metadata
            /// (description and context).  
            PaginatedResponseDto<IEnumerable<Person>> paginatedResponseDto =
                new(peopleQuery, metadata);

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

        #endregion

        #region Delete methods

        /// <summary>
        /// Deletes a Person object including its related data.
        /// </summary>
        /// <remarks>
        /// The application implements a soft-delete mechanism using a
        /// custom IsAuditable attribute to decorate an entity to be part
        /// of the mechanism in which case, the state of the entity will
        /// be modified from <em>EntityState.Deleted</em> to
        /// <em>EntityState.Modified</em> and its IsDeleted shadow property
        /// value will be set to <em>true</em> before persisting any changes
        /// into the database.
        /// </remarks>
        /// <param name="personId">The primary key property value of the
        /// entity to delete.</param>
        /// <returns>The entity object that was successfully deleted.
        /// </returns>
        async Task<Person?>? IPeople.DeletePersonAsync(int personId)
        {
            if (AppDbContext?.People is null) return null;

            /// Returns the first Person object that matches the personId
            /// primary key value. If an entity with the given primary
            /// key value is being tracked by the context, then it is
            /// returned immediately.
            ///
            /// The query result includes the related data (entities); i.e.,
            /// the included entities are also tracked (loaded) by the
            /// DbContext.
            Person? personToRemove = await AppDbContext?.People?
                .Include(p => p.Movies)
                .Include(p => p.MovieCharacters)
                .FirstOrDefaultAsync(p => p.Id == personId)!;

            if (personToRemove == null) return null;

            /// Begins tracking the Person entity in the EntityState.Deleted
            /// such that it will be removed (soft deleted) from the database
            /// when DbContext.SaveChangesAsync() is called.
            ///
            /// The database model has the Person configuration under a
            /// "cascade delete referential action". This means that as long
            /// as the related entities are being tracked by the DbContext,
            /// its relationships will be automatically deleted too.
            ///
            /// In other words, any relationships that the current Person
            /// object holds with a Movie object will be removed from the
            /// MoviePerson "join" table. 
            EntityEntry? removedPersonEntry =
                AppDbContext?.People?
                    .Remove(personToRemove);

            /// All in-memory operations, including operations with any
            /// related data, are being tracked by the database context
            /// (AppDbContext) and returned by this DeletePersonAsync method.
            ///
            /// Its consumer (PeopleController) is responsible for calling
            /// the Application/Repository/IUnitOfWork/PersistToDatabaseAsync()
            /// method that indicates the end of a Unit of Work (business
            /// transaction) and updates the database; i.e., persists any
            /// modifications to in-memory objects. 
            return personToRemove;
        }

        #endregion
    }
}



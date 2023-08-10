using System.Reflection;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.DataStore;
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
    internal class EfGenres : EfRepository<Genre>, IGenres
    {
        /// <summary>
        /// Property designed to have access to the unique DbContext
        /// inherited from its generic Repository parent class. This
        /// technique allows you to use the property for querying the DB
        /// instead of having to make an explicit conversion on each query
        /// (on each method). 
        /// </summary>
        /// 
        /// <remarks>
        /// 'Context' is a read-only protected field inherited from the
        /// parent class. This derived class employs its base class's
        /// DbContext instance as opposed to storing its constructor
        /// argument in a local variable to consume it. 
        /// 
        /// Every EfEntityName class that derives from the generic 
        /// EfRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
        /// class employs the base class's DbContext protected field which is
        /// unique and represents a single session with the database that can
        /// have multiple operations with different entity types in a single
        /// business transaction. 
        /// </remarks>
        private AppDbContext? AppContext => Context as AppDbContext;

        /// <summary>
        /// Its formal input parameter (DbContext) is not stored in a
        /// local variable because it is not consumed like that. Instead, it
        /// is passed to satisfy its base class's constructor and it is the
        /// parent class which consumes it and also makes it available to
        /// any child class through a field named "Context" with a "read-only
        /// protected" access modifier.
        /// </summary>
        /// <param name="context">The application specific DbContext that
        /// represents a session with the database.</param>
        public EfGenres(DbContext context, IHttpContextAccessor httpContextAccessor)
            : base(context, httpContextAccessor)
        { }

        #region Post-Create methods

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Queries the database for items whose property values match the
        /// values extracted from the GenresQueryFilterDto passed in the
        /// query string.
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
        /// Genre.</param>
        /// <returns>An async array with Genre items that matched the filtering
        /// criteria or an empty array if no matches were found.</returns>
        async Task<IEnumerable<Genre>> IGenres.FilterAsync(
            GenresQueryFilterDto genresDto)
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
            /// The method is case insensitive by default because the Contains
            /// method is run on the database not in the CSharp code. The case
            /// sensitivity on the query depends on the database and the
            /// collation. On SQL Server, Contains maps to SQL LIKE which is
            /// case insensitive. In SQLite, with the default collation, it
            /// is case sensitive.
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
            if (genresDto is null) return Array.Empty<Genre>();

            /// Prevents the code logic from executing in case there is an error
            /// with the query string. For example, if the query string has a
            /// spelling error in one or more of its parameter values, this
            /// FilterAsync() method would end up returning the query result
            /// without any filters.
            ///
            /// The App/Server/Controllers/GenresController/FilterGenresTask
            /// action handles this exception.
            if (!genresDto.Id.HasValue &&
                genresDto.Name is null)
                throw new InvalidFilterCriteriaException(
                    "Route template not valid for the filtering criteria.");

            /// IQueryable<typeparam name="T">&lt;T&gt;</typeparam> and 
            /// IEnumerable<typeparam name="T">&lt;T&gt;</typeparam> execute
            /// until they are consumed. However, IEnumerable cannot compose
            /// 'Expressions' and store them in an 'Expression Tree' to 
            /// consume; i.e., with IEnumerable the system loads the results
            /// into memory for each query (or filter). 
            /// https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-6.0#iqueryable-vs-ienumerable
            IQueryable<Genre>? query = AppContext?.Genres;

            /// First query filter criteria is with the Genre.Id value 
            /// extracted from the genresDto.
            if (genresDto.Id.HasValue)
                query = query?
                    .Where(g => g.Id == genresDto.Id);

            /// Second query filter criteria is with the Genre.Name value
            /// extracted from the genresDto.
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
            /// default because the Contains method is run on the database
            /// not in the CSharp code. The case sensitivity on the query
            /// depends on the database and the collation. On SQL Server,
            /// Contains maps to SQL LIKE which is case insensitive. In SQLite,
            /// with the default collation, it would be case sensitive.
            /// https://stackoverflow.com/questions/2431908/linq-to-entities-using-tolower-on-ntext-fields
            /// https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/search?view=aspnetcore-6.0
            if (!string.IsNullOrEmpty(genresDto.Name))
                query = query?
                    .Where(g => g.Name
                        .ToLower()
                        .Trim()
                        .Contains(genresDto.Name!
                            .ToLower()));

            /// This query limits the result set size to 05 items and eager
            /// loads the Person's related data.
            /// 
            /// It also returns a no-tracking query result which is quicker
            /// to execute because it does not set up change tracking info.
            /// https://docs.microsoft.com/en-us/ef/core/querying/tracking
            query = query?
                .Include(g => g.Movies)
                .Take(5)
                .AsNoTracking();

            /// Creates new Genre objects that do not include the Audit
            /// properties such as "CreatedOn", "UpdatedBy", etc.
            ///
            /// It includes its related data entities.
            ///
            /// Much more efficient than:
            /// query.ForEachAsync(g => new Genre{...});
            query = query?
                .Select(g =>
                    new Genre
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Movies = g.Movies
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
            return await query?.ToArrayAsync()!;
        }

        #endregion

        #region Put-Update methods

        #endregion

        #region Delete methods

        /// <summary>
        /// Deletes a Genre object and its relationship(s) with related
        /// Movie objects. 
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
        /// <param name="entityId">The primary key property value of the
        /// entity to delete.</param>
        /// <returns>The entity object that was successfully deleted.
        /// </returns>
        async Task<Genre?>? IGenres.DeleteGenreAsync(int genreId)
        {
            if (AppContext?.Genres is null) return null;

            /// Returns the first Genre object that matches the genreId
            /// primary key value. If an entity with the given primary
            /// key value is being tracked by the context, then it is
            /// returned immediately.
            ///
            /// The query result includes the related data (entities); i.e.,
            /// the included entities are also tracked (loaded) by the
            /// DbContext.
            Genre? genreToRemove = await AppContext?.Genres?
                .Include(g => g.Movies)
                .FirstOrDefaultAsync(g => g.Id == genreId)!;

            if (genreToRemove == null) return null;

            /// Begins tracking the Genre entity in the EntityState.Deleted
            /// state such that it will be removed (soft deleted) from the
            /// database when DbContext.SaveChangesAsync() is called.
            ///
            /// The database model has the Genre configuration under a
            /// "cascade delete referential action". This means that as long
            /// as the related entities are being tracked by the DbContext,
            /// its relationships will be automatically deleted too. 
            EntityEntry? removedGenreEntry =
                AppContext?.Genres?.Remove(genreToRemove);

            /// All in-memory operations, including operations with any
            /// related data, are being tracked by the database context
            /// (AppDbContext) and returned by this DeleteGenreAsync method.
            ///
            /// Its consumer (GenresController) is responsible for calling
            /// the Application/Repository/IUnitOfWork/PersistToDatabaseAsync()
            /// method that indicates the end of a Unit of Work (business
            /// transaction) and updates the database; i.e., persists any
            /// modifications to in-memory objects. 
            return genreToRemove;
        }

        #endregion
    }
}



using System.Security.Claims;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.DataStore;
using BlazorMovies.Server.FileStorageManager;
using BlazorMovies.Server.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace BlazorMovies.Server.Repositories
{
    /// <summary>
    /// One application specific EfEntityName class for each 
    /// IEntityName interface exposed in the IUnitOfWork interface.
    /// </summary>
    /// <remarks>
    /// It is a subclass of the <see cref="EfRepository{TEntity}"/> class
    /// which means it inherits its general functionality applicable to all
    /// entities. 
    /// <para>
    /// This class is application specific and extends its base class
    /// with specific functionality for the type passed as type parameter.
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in 
    /// the result of a query with EF's "Include" extension method.
    /// </para>
    /// <para>
    /// Its "<c>internal</c>" access modifier makes it available only to
    /// elements that reside in the same assembly (project):
    /// Application/Server-Api
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
    internal class EfMovies : EfRepository<Movie>, IMovies
    {
        /// <summary>
        /// Property designed to have access to the unique DbContext inherited
        /// from its generic Repository parent class. This technique allows you 
        /// to use the property for querying the DB instead of having to make an
        /// explicit conversion on each query (on each method). 
        /// </summary>
        /// 
        /// <remarks>
        /// 'Context' is a read-only protected field inherited from the parent
        /// class. This derived class employs its base class's DbContext instance
        /// as opposed to storing its constructor argument in a local variable
        /// to consume it. 
        /// 
        /// Every EfEntityName class that derives from the generic 
        /// EfRepository<typeparam name="TEntity">&lt;TEntity&gt;</typeparam>
        /// class employs the base class's DbContext protected field which is unique
        /// and represents a single session with the database that can have multiple
        /// operations with different entity types in a single business transaction. 
        /// </remarks>
        private AppDbContext? AppContext => Context as AppDbContext;

        /// <summary>
        /// Provides the built-in APIs for managing User in a persistence
        /// store.
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Encapsulates functionality to upload, download, and delete
        /// data objects from a container (Azure storage container or
        /// local directory).
        /// </summary>
        private readonly IFileStorageService _fileStorageService;

        /// <summary>
        /// Provides the built-in APIs for user sign-in/sign-out.
        /// </summary>
        private readonly SignInManager<ApplicationUser> _signInManager;

        /// <summary>
        /// Its formal input parameter (DbContext) is not stored in a local
        /// variable because it is not consumed like that. Instead, it is
        /// passed to satisfy its base class's constructor and it is the
        /// parent class which consumes it and also makes it available to 
        /// any child class through a field named "Context" with a "read-only
        /// protected" access modifier.
        /// </summary>
        /// <param name="context">The application specific DbContext that
        /// represents a session with the database.</param>
        /// <param name="httpContextAccessor">The interface that provides
        /// access to the current HttpContext.</param>
        /// <param name="userManager">The API that allows managing application
        /// users in a persistence store.</param>
        /// <param name="signInManager">The API that allows managing
        /// authentication state for an application user.</param>
        /// <param name="fileStorageService">Service to upload, download,
        /// delete, or copy data objects from a cloud service or a local
        /// directory.</param>
        public EfMovies(DbContext context, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFileStorageService fileStorageService)
                : base(context, httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fileStorageService = fileStorageService;
        }

        #region Post-Create methods

        /// <summary>
        /// Begins tracking the given entity and other reachable entities
        /// (related data) that are not being tracked, in the
        /// EntityState.Added such that they will be inserted into the
        /// database when DbContext.SaveChangesAsync() is called.
        /// </summary>
        /// <remarks>
        /// It is the responsibility of the MoviesController to call the
        /// DbContext.SaveChangesAsync() method to persist in-memory changes
        /// to the database.
        /// <para>
        /// Creating a Movie object is a two step process. First, it uses
        /// the EF core AddAsync method to begin tracking the Movie object
        /// in the EntityState.Added. This step "adds" the entity's primary
        /// member (property) values. It does not include the entity's
        /// related data; i.e., it does not add any related data into the
        /// EntityState.Added.
        /// </para>
        /// <para>
        /// The second step adds related data by targeting each navigation
        /// property individually. 
        /// </para>
        /// </remarks>
        /// <param name="movieDto">The DTO that encapsulates the Movie entity
        /// to insert into the database and applicable collections of related
        /// data (related entities).
        /// </param>
        /// <returns>The entity being tracked by the current database context
        /// including its related data entities. <strong>Its related data has
        /// not been persisted to the database.</strong>
        /// </returns>
        async Task<Movie?> IMovies.CreateAsync(MovieEssentialsDto movieDto)
        {
            if (movieDto == null || AppContext?.Movies == null)
                return null;

            /// Local reference of the new Movie object extracted from the DTO.
            Movie? movie = movieDto.Movie;

            /// Local reference of the Genre.Ids related to the Movie object.
            int[]? genreIds = movieDto.RelatedGenreIds;

            /// Local reference of type Dictionary<Person.Id, CharacterName>
            /// related to the Movie object.
            Dictionary<int, string?>? actorsDictionary =
                movieDto.RelatedActorsDictionary;

            /// Begins tracking the given entity and other reachable
            /// entities that are not being tracked, in the
            /// EntityState.Added such that they will be inserted into
            /// the database when DbContext.SaveChangesAsync() is called.
            ///
            /// Returns an EntityEntry that provides access to change
            /// tracking information.
            /// https://docs.microsoft.com/en-us/ef/core/change-tracking/
            EntityEntry<Movie> insertedMovieEntry = await AppContext.Movies
                .AddAsync(movie!);

            /// Gets the Movie entity being tracked by this EntityEntry.
            Movie insertedMovie = insertedMovieEntry.Entity;

            /// Maps the Genre Ids passed by the user to a Genre object
            /// from the database that is then added (or related) to
            /// the current Movie object. 
            if (genreIds?.Length > 0)
            {
                /// Highly inefficient query because it loads the complete
                /// Genre data into memory. It was left here for illustrative
                /// purposes to compare with the approach followed later with
                /// moviesDto.RelatedActorsDictionary.
                /// 
                /// Genre items table is expected to always have very few
                /// items but this is rarely the case with database data in
                /// real world applications.
                /// https://docs.microsoft.com/en-us/ef/core/performance/efficient-querying#beware-of-lazy-loading
                ///
                /// Calling an .AsNoTracking() method will throw an exception
                /// because the ChangeTracker does not work as expected while
                /// adding a new Movie object. 
                List<Genre> dbGenres = await AppContext?.Genres?
                    .ToListAsync()!;

                foreach (int relatedGenreId in genreIds)
                {
                    dbGenres.ForEach(dbGenre =>
                    {
                        if (relatedGenreId == dbGenre.Id)
                        {
                            /// Allocation of the related Movie.Genre object.
                            insertedMovie?.Genres?.Add(dbGenre);
                        }
                    });
                }
            }

            /// Maps the related data passed by the user to the related
            /// entities represented as navigational properties in the
            /// EDM.
            ///
            /// Maps the Person primary keys passed by the user to a
            /// Person object from the database that is then added (or
            /// related) to the current Movie object.
            ///
            /// Creates a MovieCharacter object which is related to
            /// the current Movie and Person objects through their
            /// primary key values and then added (or related) to the
            /// current Movie object. 
            if (actorsDictionary?.Count > 0)
            {
                /// Allows to provide a MovieCharacter.DesignatedOrder
                /// property value based on its index. The UpdateMovieAsync
                /// method has a different approach with IndexOf.
                for (int i = 0; i < actorsDictionary.Count; i++)
                {
                    /// Projects each element into an Int32. Otherwise
                    /// the Dictionary collection returns the value at
                    /// the specified index [i].
                    int selectedActorKey = actorsDictionary
                        .Select(rA => rA.Key)
                        .ToList()
                        [i];

                    /// Much more efficient query than the one used above
                    /// with moviesDto.RelatedGenreIds!!!
                    ///
                    /// Do not include an .AsNoTracking extension method
                    /// because it might cause problems when relating the
                    /// created MovieCharacter object with the Person object.
                    /// https://docs.microsoft.com/en-us/ef/core/performance/efficient-querying#beware-of-lazy-loading
                    Person? dbActorToInclude = await AppContext.People!
                        .FindAsync(selectedActorKey);

                    /// Jumps out of the iteration loop and starts a new
                    /// iteration if the criteria is met; i.e., if the
                    /// dbActorToInclude is null.  
                    if (dbActorToInclude! == null) continue;

                    /// Allocation of the related Movie.Person object.
                    insertedMovie.Actors?.Add(dbActorToInclude);

                    /// Creates a new MovieCharacter object that is
                    /// related to the Movie and Person objects through
                    /// their primary keys. 
                    MovieCharacter newMovieCharacter = new()
                    {
                        CharacterName = actorsDictionary
                            .Select(rAD => rAD.Value)
                            .ToList()
                            [i],
                        DesignatedOrder = i + 1,
                        PersonId = dbActorToInclude.Id,
                        Person = dbActorToInclude,
                        MovieId = insertedMovie!.Id,
                        Movie = insertedMovie
                    };

                    /// Allocation of the created MovieCharacter object.
                    insertedMovie.MovieCharacters
                        ?.Add(newMovieCharacter);
                }
            }

            /// All in-memory operations, including operations with any
            /// related data, are being tracked by the database context
            /// (AppDbContext) and returned by this CreateAsync()
            /// method.
            ///
            /// Its consumer (MoviesController) is responsible for calling
            /// the Application/Repository/IUnitOfWork/PersistToDatabaseAsync()
            /// method that indicates the end of a Unit of Work (business
            /// transaction) and updates the database; i.e., persists any
            /// modifications to in-memory objects. 
            return insertedMovie;
        }

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Queries the database for items whose property values match the
        /// values extracted from the <see cref="MoviesQueryFilterDto"/>
        /// passed to satisfy its formal input parameter and paginates the
        /// query result adhering to the pagination parameters sent by the
        /// client.
        /// </summary>
        /// <remarks>
        /// The property values are used as filtering criteria.
        /// <para>
        /// The method is case insensitive because it employs a
        /// <c>.ToLower()</c> extension. 
        /// </para>
        /// </remarks>
        /// <param name="moviesDto">The DTO that encapsulates values that can
        /// be directly related to one or more properties of a type
        /// <see cref="Movie"/> and the parameters to paginate the response.
        /// </param>
        /// <returns>A type that wraps the collection of items after applying
        /// the filtering criteria and the pagination parameters.</returns>
        async Task<PaginatedResponseDto<IEnumerable<Movie>>>
            IMovies.FilterPaginateMoviesAsync(MoviesQueryFilterDto moviesDto)
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

            /// IQueryable<T> and IEnumerable<T> execute until they are
            /// consumed. However, IEnumerable cannot compose 'Expressions'
            /// and store them in an 'Expression Tree' to consume; i.e., with
            /// IEnumerable the system loads the results into memory for each
            /// query (or filter). 
            /// https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-6.0#iqueryable-vs-ienumerable
            IQueryable<Movie>? query = AppContext?.Movies?
                .AsNoTracking();

            /// First query filter criteria is with the Movie.Id value 
            /// extracted from the moviesDto.
            if (moviesDto.Id.HasValue &&
                moviesDto.Id.GetValueOrDefault(0) > 0)
                query = query?
                    .Where(m => m.Id == moviesDto.Id);

            /// Second query filter criteria is with the Movie.Title value
            /// extracted from the moviesDto.
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
            /// query depends on the database and the collation. On SQL
            /// Server, Contains maps to SQL LIKE which is case insensitive.
            /// In SQLite, with the default collation, it would be case
            /// sensitive.
            /// https://stackoverflow.com/questions/2431908/linq-to-entities-using-tolower-on-ntext-fields
            /// https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-mvc-app/search?view=aspnetcore-6.0
            if (!string.IsNullOrEmpty(moviesDto.Title))
                query = query?
                    .Where(m => m.Title!
                        .ToLower()
                        .Trim()
                        .Contains(moviesDto.Title!
                            .ToLower()));

            /// Third query filter criteria is with the
            /// Movie.Genre.Name value extracted from the moviesDto.
            if (!string.IsNullOrEmpty(moviesDto.Genre))
            {
                query = query?
                    .Where(m => m.Genres!
                        .Select(mG => mG.Name
                            .ToLower())
                        .Contains(moviesDto.Genre
                            .ToLower()));
            }

            /// Fourth query filter criteria is with the
            /// Movie.UpcomingReleases value extracted from the moviesDto.
            ///
            /// The "is" operator returns "true" when the expression result is
            /// not "null" and the run-time type Nullable<bool>.HasValue is
            /// "true".
            /// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/type-testing-and-cast#is-operator
            /// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/is
            if (moviesDto.UpComingReleases is true)
            {
                DateTime currentDate = DateTime.Today;
                query = query?
                    .Where(m => m.ReleaseDate > currentDate);
            }

            /// Fifth query filter criteria is with the
            /// Movie.InTheaters value extracted from the moviesDto.
            ///
            /// The "is" operator returns "true" when the expression result is
            /// not "null" and the run-time type Nullable<bool>.HasValue is
            /// "true".
            /// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/type-testing-and-cast#is-operator
            /// https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/is
            if (moviesDto.InTheaters is true)
            {
                query = query?
                    .Where(m => m.InTheaters);
            }

            /// Sixth query filter criteria is with the Movie.Actor.Name
            /// value extracted from the moviesDto.
            if (!string.IsNullOrEmpty(moviesDto.ActorName))
                query = query?
                    .Where(m => m.Actors!
                        .Select(p => p.Name
                            .ToLower())
                        .Contains(moviesDto.ActorName
                            .ToLower()));

            /// Seventh query filter criteria is with the
            /// Movie.MovieCharacters.CharacterName value extracted from
            /// the moviesDto.
            if (!string.IsNullOrEmpty(moviesDto.CharacterName))
                query = query?
                    .Where(m => m.MovieCharacters!
                        .Select(mC => mC.CharacterName!
                            .ToLower())
                        .Contains(moviesDto.CharacterName
                            .ToLower()));

            /// The query eager loads the Movie's related data and returns a
            /// no-tracking query result which is quicker to execute because
            /// it does not set up change tracking info.
            /// https://docs.microsoft.com/en-us/ef/core/querying/tracking
            query = query?
                .Include(m => m.Genres)
                .Include(m => m.Actors)
                .Include(m => m.MovieCharacters)
                .AsNoTracking();

            /// Creates new Movie objects that do not include the Audit
            /// properties such as "CreatedOn", "UpdatedBy", etc.
            ///
            /// It includes its related data entities.
            ///
            /// Much more efficient than:
            /// query.ForEachAsync(p => new Movie{...});
            query = query?
                .Select(m =>
                    new Movie
                    {
                        Id = m.Id,
                        Title = m.Title,
                        Summary = m.Summary,
                        InTheaters = m.InTheaters,
                        TrailerUrl = m.TrailerUrl,
                        ReleaseDate = m.ReleaseDate,
                        PosterPath = m.PosterPath,
                        Genres = m.Genres,
                        Actors = m.Actors,
                        MovieCharacters = m.MovieCharacters
                    });

            /// The "Key" value for the custom HttpHeader to add to the
            /// HttpResponse with the pagination metadata. 
            const string metadataHttpHeaderTitle = "pagination-metadata";

            /// Custom extension method for the built-in IHttpContextAccessor
            /// interface formulates, serializes, and inserts the pagination
            /// metadata into the Http response in the form of a custom header.
            ///
            /// Metadata values must be captured before paginating the
            /// results. Otherwise the captured values (e.g.,
            /// totalExistingRecords) are based on the paginated results,
            /// not on the data source.
            ///
            /// PaginationRequestDto type has default values for pagination
            /// parmaeters in case its consumer does not provide any values.
            PaginationMetadata paginationMetadata = await HttpContextAccessor
                .InsertPaginationMetadataInResponse(
                    query,
                    moviesDto.PaginationRequestDto,
                    metadataHttpHeaderTitle);

            /// Custom extension method for the built-in IQueryable<T>
            /// interface determines which objects to include in the query
            /// result based on the pagination parameters (expected number
            /// of records per page and the page number requesting the data).
            ///
            /// The application employs a TestCollectionNullOrEmpty component
            /// to render collections of items to the client. This component
            /// has RenderFragment parameters for situations where a collection
            /// is either "null" or "empty". In any case, the component expects
            /// a collection of items (empty or not). Otherwise, it will most
            /// likely throw an exception. For this reason, if no filters were
            /// applied, it returns the paginated results applied to the full
            /// collection of database records. 
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
            /// key to ensure consistent ordering on every query result.
            /// Subsequent sorting should be performed in the client layer.
            ///
            /// Note that the order of Linq methods matter. What's more, in
            /// this example, the whole query result is traversed to order the
            /// elements based on their primary key. If the collection has a
            /// lot of data, it can become a performance issue. You should
            /// consider using PLinq.
            /// https://stackoverflow.com/questions/7499384/does-the-order-of-linq-functions-matter
            List<Movie> paginatedMovies = await (query ?? throw
                    new InvalidOperationException($"{nameof(query)} cannot be null."))
                .OrderBy(m => m.Id)
                .Paginate(moviesDto.PaginationRequestDto)
                .ToListAsync();

            /// Pagination processing logic must be executed after the
            /// filtering criteria has been applied and when the state for
            /// the Movie objects will not be modified any more.
            PaginatedResponseDto<IEnumerable<Movie>> paginatedResponseDto =
                new(paginatedMovies, paginationMetadata);

            /// Repositories should NOT return IQueryable types. Always
            /// remember to execute the query to return the results.
            /// 
            /// The application employs a TestCollectionNullOrEmpty component
            /// to render collections of items to the client. This component
            /// has RenderFragment parameters for situations where a collection
            /// is either "null" or "empty". In any case, the component expects
            /// a collection of items (empty or not). Otherwise, it will most
            /// likely throw an exception. 
            return paginatedResponseDto;
        }

        /// <summary>
        /// Builds two collections with Movie items and encapsulates
        /// them in a FlixManagerDto object for the FlixManager
        /// routable component.
        /// </summary>
        /// <returns>
        /// A DTO that encapsulates a collection with Movie items
        /// currently in theaters and one with Movie items that will be
        /// released to theaters.
        /// <para>
        /// It is limited to return a maximum of 4 items per collection
        /// and it does not include any related data. 
        /// </para>>
        /// </returns>
        async Task<FlixManagerDto> IMovies.GetFlixManagerDtoAsync()
        {
            /// Always strive for creating efficient queries because they can
            /// have a significant impact on the application performance. Test
            /// databases usually have a small sample of data and problems may
            /// arise with real world applications with considerably larger
            /// amounts of data. 
            /// https://docs.microsoft.com/en-us/ef/core/performance/efficient-querying#beware-of-lazy-loading
            const int moviesLimit = 8;

            DateTime currentDate = DateTime.Now.Date;

            /// Items in descending order. Collection items do not include
            /// related data nor "audit" properties.
            /// https://docs.microsoft.com/en-us/ef/core/querying/tracking
            List<Movie> moviesInTheaters = await AppContext?.Movies!
                .AsNoTracking()
                .Where(m => m.InTheaters)
                .Take(moviesLimit)
                .OrderByDescending(m => m.ReleaseDate)
                .Select(m => new Movie()
                {
                    Id = m.Id,
                    Title = m.Title,
                    Summary = m.Summary,
                    InTheaters = m.InTheaters,
                    TrailerUrl = m.TrailerUrl,
                    ReleaseDate = m.ReleaseDate,
                    PosterPath = m.PosterPath
                })
                .ToListAsync()!;

            /// Items in ascending order. Collection items do not include
            /// related data nor "audit" properties.
            /// https://docs.microsoft.com/en-us/ef/core/querying/tracking
            List<Movie> moviesUpcomingReleases = await AppContext?.Movies!
                .AsNoTracking()
                .Where(m => m.ReleaseDate > currentDate)
                .Take(moviesLimit)
                .OrderBy(m => m.ReleaseDate)
                .Select(m => new Movie()
                {
                    Id = m.Id,
                    Title = m.Title,
                    Summary = m.Summary,
                    InTheaters = m.InTheaters,
                    TrailerUrl = m.TrailerUrl,
                    ReleaseDate = m.ReleaseDate,
                    PosterPath = m.PosterPath
                })
                .ToListAsync()!;

            /// Flattening of Movie data to include in the Http response.
            /// 
            /// The DTO object allows optimization of the data transfer,
            /// prevents passing sensitive information, and makes it easier
            /// for the client to access specific data. Most of the processing
            /// logic takes places server-side.
            return new FlixManagerDto(moviesInTheaters, moviesUpcomingReleases);
        }

        /// <summary>
        /// Queries the database for a Movie object whose identity property
        /// value matches the value passed to satisfy its formal input
        /// parameter and calculates its <see cref="MovieScore"/> average if
        /// any.
        /// </summary>
        /// <remarks>
        /// This method is employed by the MovieBulletin routable component for
        /// scenarios where the current <see cref="ApplicationUser"/> is not
        /// authenticated.
        /// <para>
        /// It creates an object of type MovieBulletinDto that optimizes data
        /// transfer, prevents passing sensitive information, and makes it
        /// easier for the client to access specific data (flattening). 
        /// </para>
        /// </remarks>
        /// <param name="movieId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>A MovieBulletinDto with a combination of properties of
        /// an object of type Movie that include its movie score average
        /// optimized for data transferring and turned into a single
        /// <em>model</em>.
        /// </returns>
        async Task<MovieBulletinDto?> IMovies.GetMovieBulletinDtoAsync(
            int movieId)
        {
            if (AppContext?.Movies is null) return null;

            /// Queries the database to retrieve the Movie item that matches the
            /// movieId primary key value. It includes the object's related data.
            Movie? movie = await AppContext?.Movies?
                .AsNoTracking()
                .Where(m => m.Id == movieId)
                .Include(m => m.Genres)
                .Include(m => m.Actors)
                .Include(m => m.MovieCharacters)
                .FirstOrDefaultAsync()!;

            if (movie == null) return null;

            #region MovieScore Average:

            double movieScoreAverage = 0.0;

            /// Evaluates if there are any movie ratings in the data store for
            /// the current Movie object. 
            if (await AppContext.MovieScores
                    .AnyAsync(mS => mS.MovieId == movieId))
            {
                /// Retrieves the movie ratings in the data store for the
                /// current Movie object to compute their average.
                movieScoreAverage = await AppContext.MovieScores
                    .Where(mS => mS.MovieId == movieId)
                    .AverageAsync(mS => mS.Score);
            }

            #endregion

            /// Flattening of Movie data and related entities to include
            /// in the Http response.
            /// 
            /// The DTO object allows optimization of the data transfer,
            /// prevents passing sensitive information, and makes it easier
            /// for the client to access specific data. Most of the processing
            /// logic takes places server-side.
            MovieBulletinDto movieBulletinDto = new()
            {
                /// A Movie object stripped from any related data. It only
                /// contains primary member values.
                Movie = new Movie()
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    Summary = movie.Summary,
                    InTheaters = movie.InTheaters,
                    TrailerUrl = movie.TrailerUrl,
                    ReleaseDate = movie.ReleaseDate,
                    PosterPath = movie.PosterPath,
                },
                /// A collection of Genre items related to the current
                /// Movie object. Each item is stripped from any related
                /// data.
                Genres = movie?.Genres?
                    .Select(g => new Genre()
                    {
                        Id = g.Id,
                        Name = g.Name
                    }).ToList(),
                /// A collection of Person items related to the current
                /// Movie object. Each item is stripped from any related
                /// data.
                ///
                /// The TempCharacterName property is temporary because
                /// it is not mapped to the people table in the database.
                Actors = movie?.Actors?
                    .Select(p => new Person()
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Biography = p.Biography,
                        PictureUrl = p.PictureUrl,
                        DateOfBirth = p.DateOfBirth,
                        TempCharacterName = movie.MovieCharacters?
                            .FirstOrDefault(mC => mC.PersonId == p.Id)?
                            .CharacterName
                    })
                    .OrderBy(p =>
                    {
                        int? order = movie.MovieCharacters?
                            .FirstOrDefault(mC => mC.PersonId == p.Id)?
                            .DesignatedOrder;

                        return order;
                    }).ToList(),
                /// The computed average of the MovieScore records in the data
                /// store for the current Movie object.
                MovieScoreAverage = movieScoreAverage,
            };

            return movieBulletinDto;
        }

        /// <summary>
        /// Recycles the <see cref="IMovies.GetMovieBulletinDtoAsync"/> method
        /// to query the database for a Movie object whose identity property
        /// value matches the value passed to satisfy its formal input
        /// parameter and calculate its <see cref="MovieScore"/> average if
        /// any. Additionally, this method retrieves the
        /// <see cref="MovieScore"/> record value selected by the current
        /// <see cref="ApplicationUser"/>.
        /// </summary>
        /// <remarks>
        /// This method is employed by the MovieBulletin routable component for
        /// scenarios where the current <see cref="ApplicationUser"/> is 
        /// authenticated.
        /// <para>
        /// It creates an object of type MovieBulletinDto that optimizes data
        /// transfer, prevents passing sensitive information, and makes it
        /// easier for the client to access specific data (flattening). 
        /// </para>
        /// </remarks>
        /// <param name="movieId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>A MovieBulletinDto with a combination of properties of
        /// an object of type Movie that includes its movie score average and
        /// the current user's movie score optimized for data transferring
        /// and turned into a single <em>model</em>.
        /// </returns>
        async Task<MovieBulletinDto?> IMovies.GetMovieBulletinWithUserScoreDtoAsync(
            int movieId)
        {
            /// Recycles previously created code to query the database for a
            /// Movie object whose identity property value matches the value
            /// passed to satisfy its formal input parameter and calculates
            /// its <see cref="MovieScore"/> average if any.
            MovieBulletinDto? movieBulletinDto = await ((IMovies)this)
                .GetMovieBulletinDtoAsync(movieId);

            /// Verifies if current user is autheticated.
            if (HttpContextAccessor.HttpContext?.User.Identity is null
                || !HttpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
                return movieBulletinDto;

            /// Retrieves the primary key for the application user.
            string? dbUserId = HttpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            /// Retrieves the application user associatd with the specified
            /// ApplicationUser.Id
            ApplicationUser dbUser = await _userManager
                .FindByIdAsync(dbUserId);

            /// Queries the database for a movie score record that matches the
            /// current application user id and the current Movie id. 
            MovieScore? existingMovieScore = await AppContext.MovieScores
                .FirstOrDefaultAsync(mS => mS.MovieId == movieId
                                           && mS.UserId == dbUserId);

            /// If a MovieRating record is found, extract its rating value.
            /// Otherwise, allocate a value of zeroe.
            movieBulletinDto.UserScore = existingMovieScore?.Score ?? 0;

            return movieBulletinDto;
        }

        /// <summary>
        /// Queries the database for a Movie object whose identity property
        /// value matches the value passed to satisfy its formal input
        /// parameter.
        /// </summary>
        /// <remarks>
        /// It consumes the GetMovieBulletinDtoAsync method to create an
        /// object of type MovieEditDto that optimizes data transfer,
        /// prevents passing sensitive information, and makes it easier
        /// for the client to access specific data (flattening).
        /// </remarks>
        /// <param name="movieId">The identity key value to use for
        /// querying the database.</param>
        /// <returns>A MovieEditDto with a combination of properties
        /// of an object of type Movie optimized for data transferring
        /// and turned into a single <em>model</em>.</returns>
        async Task<MovieEditDto?> IMovies.GetMovieEditDtoAsync(int movieId)
        {
            if (AppContext?.Movies is null) return null;

            /// Recycles previously created code to obtain a Movie object
            /// and its related data (Genres and Actors) easily available
            /// in special purpose collections.
            MovieBulletinDto? movieBulletinDto = await ((IMovies)this)
                .GetMovieBulletinDtoAsync(movieId);

            /// Flattening of Movie data and related entities to include
            /// in the Http response.
            /// 
            /// The DTO object allows optimization of the data transfer,
            /// prevents passing sensitive information, and makes it easier
            /// for the client to access specific data. Most of the processing
            /// logic takes places server-side.
            MovieEditDto movieEditDto = new()
            {
                Movie = movieBulletinDto?.Movie,

                SelectedGenres = movieBulletinDto?.Genres!,

                AvailableGenres = AppContext?.Genres!
                    .Where(dbGenre =>
                        /// Note the negation operator (!)
                        !movieBulletinDto!.Genres!
                            .Select(mG => mG.Id)
                            .Contains(dbGenre.Id))
                    .ToList(),

                /// Captures CharacterName in its TempCharactername property
                /// because the MovieBulletinDto allocates it during creation.
                Actors = movieBulletinDto!.Actors
            };

            return movieEditDto;
        }

        #endregion

        #region Put-Update methods

        /// <summary>
        /// Updates all the properties of a Movie object including its
        /// related data (entities).
        /// </summary>
        /// <remarks>
        /// The EfRepository/UpdateAsync method and this
        /// EfMovies/UpdateMovieAsync method must be named differently because
        /// this method takes precedence so it hides the other method when trying
        /// to access it through the public interface of a Movie entity.
        /// <para>
        /// Updating is a two step process. First, it recycles the
        /// EFManager/EfRepository/UpdateAsync <strong>generic</strong> method
        /// to execute an update on the primary members (properties) of the
        /// Movie object; i.e., the generic method does not include related
        /// data on the updating process. Then it updates related data by
        /// targeting each navigation property individually.
        /// </para>
        /// </remarks>
        /// <param name="entityId">The identity key value to use for querying
        /// the database. The MoviesController endpoint is responsible for
        /// validating the entityId property value against the database. 
        /// </param>
        /// <param name="dtoWithNewValues">The DTO that encapsulates the
        /// Movie object with the new property values. It includes an array
        /// of Movie.Genre Ids related to the Movie object and a dictionary
        /// with (Person.Id, Person.CharacterName) as (Key, Value) pairs of
        /// the Person items related to the Movie object. </param>
        /// <returns>A MovieEditDto with a combination of properties of
        /// an object of type Movie optimized for data transferring and turned
        /// into a single <em>model</em>.</returns>
        async Task<MovieEditDto?> IMovies.UpdateMovieAsync(
            int entityId, MovieEssentialsDto? dtoWithNewValues)
        {
            if (dtoWithNewValues == null || AppContext?.Movies == null)
                return null;

            /// Local reference of Movie object with the new property values.
            Movie? movieWithNewValues = dtoWithNewValues.Movie;

            /// Local reference of the Genre.Ids related to the Movie object.
            int[]? newGenreIds = dtoWithNewValues.RelatedGenreIds;

            /// Local reference of type Dictionary<Person.Id, CharacterName>
            /// related to the Movie object. 
            Dictionary<int, string?>? newActorsDictionary =
                dtoWithNewValues.RelatedActorsDictionary;

            /// Updates the primary members (no navigation properties)
            /// of the Movie object. 
            await ((IMovies)this).UpdateAsync(entityId, movieWithNewValues!);

            /// Returns the first Movie object that matches the movieId
            /// primary key value. If an entity with the given primary
            /// key value is being tracked by the context, then it is
            /// returned immediately.
            ///
            /// The Movie object returned holds the new values for its
            /// primary data (i.e., except related entities).
            Movie? movieToUpdate = await AppContext?.Movies?
                .Include(m => m.Genres)
                .Include(m => m.Actors)
                .Include(m => m.MovieCharacters)
                .FirstOrDefaultAsync(m => m.Id == entityId)!;

            /// Individual updating of related data of type Genre.
            ///
            /// If newGenreIds collection is empty, it means the
            /// user did not select any Genre items for the Movie
            /// object or even unselected the existing ones. The
            /// current Movie.Genres data in the database table
            /// must be cleared.
            ///
            /// If newGenreIds collection is not empty, the current
            /// Genre items related to the Movie object are cleared
            /// before inserting the Genre items sent with the
            /// Http request.
            ///
            /// Either case requires clearing the Movie.Genre data
            /// currently stored in the database. Type Genre is
            /// decorated with an [IsAuditable(IsDeletable = true)]
            /// custom attribute so it is actually "soft deleted".
            movieToUpdate?.Genres?.Clear();

            /// Maps the Genre Ids passed by the user to a Genre object
            /// from the database that is then added (or related) to
            /// the current Movie object. 
            if (newGenreIds?.Length > 0)
            {
                foreach (int genreId in newGenreIds)
                {
                    /// Advisable not to use .AsNoTracking method during an
                    /// updtae operation because the tracker duplicates
                    /// entities and duplicate primary keys will most likely
                    /// throw an exception.
                    Genre? dbGenre = await AppContext?.Genres?
                        //.AsNoTracking()
                        .FirstOrDefaultAsync(g => g.Id == genreId)!;

                    /// Starts a new iteration 
                    if (dbGenre is null) continue;

                    /// Allocation the related Movie.Genre object.
                    movieToUpdate?.Genres?.Add(dbGenre);
                }
            }

            /// Individual updating of related data of type Person &
            /// MovieCharacter.
            ///
            /// If newActorsDictionary is empty, it means the user
            /// did not select any Person items for the Movie object
            /// or even unselected the existing ones. The current
            /// Movie.Person data in the database table must be
            /// cleared.
            ///
            /// If newActorsDictionary is not empty, the current
            /// Person items related to the Movie object and their
            /// related MovieCharacters are cleared before inserting
            /// the Person items sent with the Http request.  
            ///
            /// Either case requires clearing the current Movie.Person
            /// items and their related MovieCharacter objects currently
            /// stored in the database. Both entities are decorated with
            /// an [IsAuditable(IsDeletable = true)] custom attribute so
            /// they are actually "soft deleted".
            movieToUpdate?.Actors?.Clear();
            movieToUpdate?.MovieCharacters?.Clear();

            /// Maps the Person primary keys passed by the user to a
            /// Person object from the database that is then added (or
            /// related) to the current Movie object.
            ///
            /// Creates a MovieCharacter object which is related
            /// to the current Movie and Person objects through their
            /// primary key values and then added (or related) to the
            /// current Movie object. 
            if (newActorsDictionary?.Count > 0)
            {
                for (int i = 0; i < newActorsDictionary.Count; i++)
                {
                    /// Projects each element into an Int32. Otherwise
                    /// the Dictionary collection returns the value
                    /// (string) at the specified index [i].
                    int selectedActorKey = newActorsDictionary
                        .Select(a => a.Key)
                        .ToList()[i];

                    /// Advisable not to use .AsNoTracking method during an
                    /// updtae operation because the tracker duplicates
                    /// entities and duplicate primary keys will most likely
                    /// throw an exception.
                    Person? dbActorToInclude = await AppContext?.People!
                        //.AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Id == selectedActorKey)!;

                    /// Jumps out of the iteration loop and starts a new
                    /// iteration if the criteria is met; i.e., if the
                    /// dbActorToInclude is null.
                    if (dbActorToInclude == null) continue;

                    /// Allocation of the related Movie.Person object.
                    movieToUpdate?.Actors?.Add(dbActorToInclude!);

                    /// Creates a new MovieCharacter object that is
                    /// related to the Movie and Person objects through
                    /// their primary keys. 
                    MovieCharacter? newMovieCharacter = new()
                    {
                        CharacterName = newActorsDictionary
                            .Select(a => a.Value)
                            .ToList()[i],

                        DesignatedOrder = newActorsDictionary.Keys
                            .ToList()
                            .IndexOf(dbActorToInclude!.Id) + 1,

                        PersonId = dbActorToInclude!.Id,
                        MovieId = movieToUpdate!.Id
                    };

                    /// Allocation of the created MovieCharacter object.
                    movieToUpdate!.MovieCharacters?.Add(newMovieCharacter);
                }
            }

            /// Flattening of Movie data and related entities to include
            /// in the Http response.
            /// 
            /// The DTO object allows optimization of the data transfer,
            /// prevents passing sensitive information, and makes it easier
            /// for the client to access specific data. The processing and
            /// mapping logic takes places server-side.
            MovieEditDto movieEditDto = new()
            {
                Movie = movieToUpdate,

                SelectedGenres = movieToUpdate?.Genres?
                    .ToList()!,

                AvailableGenres = AppContext?.Genres!
                    .AsNoTracking()
                    .Where(dbGenre =>
                        /// Note the negation operator (!)
                        !movieToUpdate!.Genres!
                            .Select(mG => mG.Id)
                            .Contains(dbGenre.Id))
                    .ToList(),

                /// Does not capture CharacterName in its TempCharactername
                /// property because it is not required; i.e., the
                /// MovieBulletin routable component is served with a
                /// MovieBulletinDto which captures the value.
                Actors = movieToUpdate?.Actors?
                    .ToList()!
            };

            /// All in-memory operations, including operations with any
            /// related data, are being tracked by the database context
            /// (AppDbContext) and returned by this UpdateMovieAsync
            /// method.
            ///
            /// Its consumer (MoviesController) is responsible for calling
            /// the Application/Repository/IUnitOfWork/PersistToDatabaseAsync()
            /// method that indicates the end of a Unit of Work (business
            /// transaction) and updates the database; i.e., persists any
            /// modifications to in-memory objects. 
            return movieEditDto;
        }

        #endregion

        #region Delete methods

        /// <summary>
        /// Deletes a Movie object including its related data.
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
        /// <param name="movieId">The primary key property value of the
        /// entity to delete.</param>
        /// <returns>The entity object that was successfully deleted.
        /// </returns>
        async Task<Movie?>? IMovies.DeleteMovieAsync(int movieId)
        {
            if (AppContext?.People is null) return null;

            /// Returns the first Movie object that matches the movieId
            /// primary key value. If an entity with the given primary
            /// key value is being tracked by the context, then it is
            /// returned immediately.
            ///
            /// The query result includes the related data (entities); i.e.,
            /// the included entities are also tracked (loaded) by the
            /// DbContext.
            Movie? movieToRemove = await AppContext?.Movies?
                .Include(m => m.Genres)
                .Include(m => m.Actors)
                .Include(m => m.MovieCharacters)
                .FirstOrDefaultAsync(m => m.Id == movieId)!;

            if (movieToRemove == null) return null;

            /// Begins tracking the Person entity in the EntityState.Deleted
            /// such that it will be removed (soft deleted) from the database
            /// when DbContext.SaveChangesAsync() is called.
            ///
            /// The database model has the Movie configuration under a
            /// "cascade delete referential action". This means that as long
            /// as the related entities are being tracked by the DbContext,
            /// its relationships will be automatically deleted too.
            ///
            /// In other words, any relationships that the current Movie
            /// object holds with a type Genre, Person, and/or MovieCharacter
            /// will be handled.
            EntityEntry? removedMovieEntry =
                AppContext?.Movies?
                    .Remove(movieToRemove);

            /// All in-memory operations, including operations with any
            /// related data, are being tracked by the database context
            /// (AppDbContext) and returned by this DeletePersonAsync method.
            ///
            /// Its consumer (PeopleController) is responsible for calling
            /// the Application/Repository/IUnitOfWork/PersistToDatabaseAsync()
            /// method that indicates the end of a Unit of Work (business
            /// transaction) and updates the database; i.e., persists any
            /// modifications to in-memory objects. 
            return movieToRemove;
        }

        #endregion

        #region Reset Data methods

        /// <summary>
        /// Resets to an initial state of deployment the entire
        /// application database and the containers responsible to
        /// serve with images the <see cref="Movie"/> and
        /// <see cref="Person"/> data entities . The steps executed
        /// are:
        /// <para>
        /// 1. Signs out the current user from the application. Otherwise,
        /// the application keeps current permissions even though the user's
        /// database record might not even exist anymore.
        /// </para>
        /// <para>
        /// 2. Removes any data (images) stored in the public containers
        /// responsible for serving the data (images) to the application.
        /// </para>
        /// <para>
        /// 3. Copies backup data (images) from the backup containers to the
        /// public containers.
        /// </para>
        /// <para>
        /// 4. Deletes all data from the database including the ASP.Net
        /// Identity tables.
        /// </para>
        /// <para>
        /// 5. Executes raw SQL to insert the initial state data to all the
        /// database tables including the ASP.Net Identity tables.
        /// </para>
        /// <remarks>
        /// The <see cref="IFileStorageService"/> injected in the dependency
        /// injection container of the Application/Server-Api project
        /// determines whether the containers to work with reside in an Azure
        /// Storage Account or in local directories.
        /// </remarks>
        /// </summary>
        /// <returns>True if the operation to reset the database is successful.
        /// Otherwise false.</returns>
        async Task<bool> IMovies.ResetDatabaseAsync()
        {
            string moviesContainer = "images-movies";
            string peopleContainer = "images-people";

            string moviesBackupContainer = "images-movies-backup";
            string peopleBackupContainer = "images-people-backup";

            /// 1. Sign out the current user from the application. Otherwise,
            /// the application keeps current permissions eventhough the user's
            /// database record might not even exist anymore.
            ///
            /// Must be placed before removing the application users from the
            /// database or it will throw an exception because the user might
            /// not exist any more. 
            await _signInManager.SignOutAsync();

            /// 2. Remove any data (images) from the public containers.
            /// Containers location is dependent on the IFileStorageService
            /// implementation injected in the Application/Server-Api Program
            /// class (e.g., AzureStorageService or InAppStorageService).
            await _fileStorageService.DeleteContainerContentAsync(
                moviesContainer);

            await _fileStorageService.DeleteContainerContentAsync(
                peopleContainer);

            /// 3. Copy backup data (image files) from the backup containers
            /// (or directories) to the public containers responsible for
            /// serving the data to the app.
            await _fileStorageService.CopyContainerContentAsync(
            moviesBackupContainer,
                moviesContainer);

            await _fileStorageService.CopyContainerContentAsync(
                peopleBackupContainer,
                peopleContainer);

            /// 4. Delete data from database tables. WARNING: Ensure to start
            /// from linking (dependent) to principal tables. E.g.,
            /// dbo.AspNetRoles (principal) should be deleted after
            /// dbo.AspNetRoleClaims (dependent) table. You can use the
            /// InitialCreate migration to look for any table foreign keys
            /// (dependencies).
            ///
            /// Genre and Person data entities have a relationship with Movie
            /// data entity type; i.e., they have navigation properties that
            /// relate to each other. As explained in Create Data Lesson, by
            /// convention, cascade delete is set to cascade for required
            /// (not nullable) relationships. This means dependent entities
            /// are also deleted when the principal entity is deleted.
            ///
            /// In other words, when data from Genre, Person, and/or Movie
            /// database tables is removed, any realted data in the
            /// GenreMovie and MoviePerson linking tables is automatically
            /// removed too.
            ///
            /// WARNING: Include the IgnoreQueryFilters() to bypass any global
            /// query filters configured for the given entity and remove
            /// database records that have been soft-deleted. Otherwise, the
            /// "Insert" blocks of raw sql will likely throw an exception.
            ///
            /// For example, suppose the Action genre (Id: 4) has been soft
            /// deleted. It is marked as deleted:true but the record still
            /// exist in the database. If raw sql attempts to insert the
            /// Action genre (Id:4), it will throw an exception because the
            /// database table already has an Id:4 eventhough it is marked as
            /// soft-deleted.
            /// Otherwise, the "Insert" blocks of raw sql will likely throw an
            /// exception. 
            /// https://app.flix-manager.com/10-ef-soft-delete-audit
            ///
            /// 
            /// An alternate option is to replace ExecuteDeleteAsync() with raw
            /// sql to bypass the soft delete mechanism implemented and a
            /// transaction to commit the changes made to the database. Raw SQL
            /// wipes out the complete database table records. E.g.,
            /// <code>
            /// await AppContext.Database.ExecuteSqlRawAsync(@"
            /// DELETE FROM Genres WHERE Name='Drama';");
            /// </code>
            /// https://www.w3schools.com/sql/sql_delete.asp
            int dboMovieScoresDeletedRows =
                await AppContext?.MovieScores?
                    .IgnoreQueryFilters()
                    .ExecuteDeleteAsync()!;

            int dboMovieCharactersDeletedRows =
                await AppContext?.MovieCharacters?
                    .IgnoreQueryFilters()
                    .ExecuteDeleteAsync()!;

            int dboGenresDeletedRows =
                await AppContext?.Genres?
                    .IgnoreQueryFilters()
                    .ExecuteDeleteAsync()!;

            int dboPeopleDeleteRows =
                await AppContext?.People?
                    .IgnoreQueryFilters()
                    .ExecuteDeleteAsync()!;

            int dboMoviesDeletedRows =
                await AppContext?.Movies?
                    .IgnoreQueryFilters()
                    .ExecuteDeleteAsync()!;

            /// ASP.Net Core Identity tables related to authentication and
            /// authorization operations.
            await AppContext?.DeviceFlowCodes.ExecuteDeleteAsync()!;
            await AppContext?.Keys.ExecuteDeleteAsync()!;
            await AppContext?.PersistedGrants.ExecuteDeleteAsync()!;
            await AppContext?.RoleClaims.ExecuteDeleteAsync()!;
            await AppContext?.UserRoles.ExecuteDeleteAsync()!;
            await AppContext?.UserClaims.ExecuteDeleteAsync()!;
            await AppContext?.UserLogins.ExecuteDeleteAsync()!;
            await AppContext?.UserTokens.ExecuteDeleteAsync()!;

            /// Ensure that Roles data can be safely deleted. Currently,
            /// the app's security system is based on UserClaims; i.e.,
            /// UserRoles are not consumed.
            await AppContext?.Roles.ExecuteDeleteAsync()!;

            await AppContext?.Users.ExecuteDeleteAsync()!;

            /// 5. Insert the initial state data to database tables. WARNING:
            /// Ensure to start from principal data entity tables to dependent
            /// data entity tables. E.g., Genre, People, and Movie tables
            /// should receive data before GenreMovie and MoviePerson tables.
            ///
            /// You can use the InitialCreate migration to look for any table
            /// foreign keys (dependencies).

            /// Transactions allow several database operations to be processed
            /// in an atomic manner. If the transaction is committed, all of
            /// the operations are successfully applied to the database.
            /// https://learn.microsoft.com/en-us/ef/core/saving/transactions
            await using IDbContextTransaction transaction =
                await AppContext.Database.BeginTransactionAsync();

            /// Genres table
            await AppContext.Database.ExecuteSqlRawAsync(@"
SET IDENTITY_INSERT [dbo].[Genres] ON
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1, N'Drama', N'Administrator', N'2022-08-02 09:10:27', 0, N'admin@email.com', N'2023-03-10 15:24:14')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (2, N'Comedy', N'Administrator', N'2022-08-02 09:10:27', 0, N'', N'2022-08-02 09:10:27')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (3, N'Fiction', N'Administrator', N'2022-08-02 09:10:27', 0, N'', N'2022-08-02 09:10:27')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (4, N'Action', N'Administrator', N'2022-08-02 09:10:27', 0, N'', N'2022-08-02 09:10:27')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (5, N'Adventure', N'Administrator', N'2022-08-02 09:10:27', 0, N'', N'2022-08-02 09:10:27')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1014, N'Romance', N'Administrator', N'2022-08-16 15:34:55', 0, N'Unauthenticated User', N'2022-08-16 15:34:55')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1015, N'Crime', N'Administrator', N'2022-08-16 15:40:47', 0, N'Unauthenticated User', N'2022-08-16 15:40:47')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1016, N'Thriller', N'Administrator', N'2022-08-16 15:40:55', 0, N'Unauthenticated User', N'2022-08-16 15:40:55')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1017, N'Sci-Fi', N'admin@email.com', N'2023-03-24 13:15:31', 0, N'admin@email.com', N'2023-03-24 13:15:31')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1018, N'Fantasy', N'admin@email.com', N'2023-03-24 13:23:34', 0, N'admin@email.com', N'2023-03-24 13:23:34')
INSERT INTO [dbo].[Genres] ([Id], [Name], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1019, N'Warfare', N'admin@email.com', N'2023-03-24 13:24:04', 0, N'admin@email.com', N'2023-03-24 13:24:04')
SET IDENTITY_INSERT [dbo].[Genres] OFF
");

            /// People table
            await AppContext.Database.ExecuteSqlRawAsync(@"
SET IDENTITY_INSERT [dbo].[People] ON
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1, N'Margot Robbie', N'Born in Dalby, Queensland, Australia. In her late teens, she moved to Melbourne to pursue an acting career. ', N'https://blazormoviesstg.blob.core.windows.net/images-people/7ac84670-75e5-4982-a616-0eac10d97b57.jpg', N'1990-07-02 00:00:00', N'Administrator', N'2022-08-02 09:10:27', 0, N'admin@email.com', N'2023-03-09 15:44:07')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (2, N'Scarlett Johansson', N'Born in Manhattan, New York. She began acting during childhood after her mother started taking her to auditions. She made her professional debut at the age of eight.', N'https://blazormoviesstg.blob.core.windows.net/images-people/1cb0e372-1d24-46a3-a5fc-e4c0b0bae7c6.jpg', N'1984-11-22 00:00:00', N'Administrator', N'2022-08-02 09:10:27', 0, N'admin@email.com', N'2023-03-09 15:43:21')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (3, N'Sofia Vergara', N'Born and raised in Barranquilla, Colombia. She was discovered by a photographer and this led to various jobs in modeling and television.', N'https://blazormoviesstg.blob.core.windows.net/images-people/6b347c02-2d2a-40a1-9ef2-6b07fb4f36a3.jpg', N'1972-07-10 00:00:00', N'Administrator', N'2022-08-02 09:10:27', 0, N'admin@email.com', N'2023-03-09 15:44:32')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (4, N'Beyonce', N'Born in Houston, Texas. She and her group, Desitny''s Child, were discovered by Whitney Houston. Beyonce writes and produces many of her songs.', N'https://blazormoviesstg.blob.core.windows.net/images-people/6e5f7a35-42af-4c6f-82f0-17bece9f39c6.jpg', N'1981-09-04 00:00:00', N'Administrator', N'2022-08-02 09:10:27', 0, N'admin@email.com', N'2023-03-27 11:11:30')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (7, N'Penelope Cruz', N'Bron in Madrid, Spain. After studying classical ballet for nine years at Spain''s National Conservatory, she continued her training under a series of prominent dancers. ', N'https://blazormoviesstg.blob.core.windows.net/images-people/e32daff5-4f32-438b-a4a7-cf1eb39bc9bf.jpg', N'1974-04-28 00:00:00', N'Administrator', N'2022-03-29 09:26:49', 0, N'admin@email.com', N'2023-03-09 15:44:47')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (8, N'Natalie Dormer', N'Born in Reading, Berkshire, England. She is best known for her roles as Anne Boleyn on the showtime series The Tudors, as Margaery Tyrell on the HBO series Game of Thrones, etc.', N'https://blazormoviesstg.blob.core.windows.net/images-people/dcdf8222-bedd-4dc1-942b-b43b915ce838.jpg', N'1982-02-11 00:00:00', N'Administrator', N'2022-03-29 09:34:45', 0, N'admin@email.com', N'2023-03-09 15:45:01')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (9, N'Ana de Armas', N'Born in Cuba. At the age of 14, she began studies at the National Theatre School of Havana where she graduated after 4 years. At 16, she made her first film ""Una rosa de Francia"" (2006).', N'https://blazormoviesstg.blob.core.windows.net/images-people/865cfb47-08cd-46a7-839e-af67a97beb07.jpg', N'1988-04-30 00:00:00', N'Administrator', N'2022-04-12 09:00:32', 0, N'admin@email.com', N'2023-03-09 15:45:12')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (10, N'Amber Heard', N'Born in Austin, Texas. She appeared in the Academy award nominated film North Country (2005) in which she played Charlize Theron''s character in flashbacks.', N'https://blazormoviesstg.blob.core.windows.net/images-people/cc9e56d9-5252-414e-9cde-64ef0600c1b3.jpg', N'1986-04-22 00:00:00', N'Administrator', N'2022-04-12 09:02:04', 0, N'admin@email.com', N'2023-03-09 15:45:25')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (14, N'Elizabeth Hurley', N'Born in Basingstoke, Hampshire. She won a scholarship to the London Studio Centre which taught courses for dance and theater. ', N'https://blazormoviesstg.blob.core.windows.net/images-people/d8361c6b-d224-40e6-aeb9-22f4ce470e23.jpg', N'1965-06-10 00:00:00', N'Administrator', N'2022-08-16 11:54:06', 0, N'admin@email.com', N'2023-03-09 15:45:39')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (15, N'Bridget Moynahan', N'Born in New York city. She is known for playing Dr. Susan Calvin in I, Robot (2004), Erin Reagan in Blue Bloods (2010), etc. She has a son from Tom Brady.', N'https://blazormoviesstg.blob.core.windows.net/images-people/2a9d027a-dece-4894-a946-48b23f92155d.jpg', N'1971-04-28 00:00:00', N'Administrator', N'2022-08-16 11:55:50', 0, N'admin@email.com', N'2023-03-09 15:45:52')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (16, N'Cameron Diaz', N'Born in San Diego, California. Cameron left home at 16 and lived in Japan, Australia, Mexico, Morocco, and Paris. At 21, returned to California and auditioned for a big part in The Mask (1994).', N'https://blazormoviesstg.blob.core.windows.net/images-people/7d8b108f-3c77-43f4-a426-46c0f62dea72.jpg', N'1972-08-30 00:00:00', N'Administrator', N'2022-08-16 15:12:05', 0, N'admin@email.com', N'2023-03-09 15:46:07')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (17, N'Connie Nielsen', N'Born in Frederikshavn, Denmark. At 18, she headed to Paris to continue her pursuit of acting, which led to further work and study in Rome and Milan. ', N'https://blazormoviesstg.blob.core.windows.net/images-people/a11c99bc-d193-4340-b73f-7272f2f94493.jpg', N'1965-07-03 00:00:00', N'Administrator', N'2022-08-16 15:13:46', 0, N'admin@email.com', N'2023-03-09 15:50:00')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (18, N'Gal Gadot', N'Born in Israel. Actress, singer, martial artist, and model. She made her film debut in the fourth film of the Fast and Furious franchise (2009).', N'https://blazormoviesstg.blob.core.windows.net/images-people/36bc7ae4-03b3-4a80-8427-c7346aa061c9.jpg', N'1985-04-30 00:00:00', N'Administrator', N'2022-08-16 15:16:01', 0, N'admin@email.com', N'2023-03-09 18:23:19')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (19, N'Kate Beckinsale', N'Born in Hounslow, Middlesex, England. In her teens, she twice won the British bookseller W.H. Smith Young Writer''s competition.', N'https://blazormoviesstg.blob.core.windows.net/images-people/c6b3e197-4deb-4212-b40b-cc0e373b476f.jpg', N'1973-07-26 00:00:00', N'Administrator', N'2022-08-16 15:17:39', 0, N'admin@email.com', N'2023-03-09 18:19:31')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (20, N'Marion Cotillard', N'Born in Paris, France. She studied drama at the Conservatoire d''Art Dramatique in Orléans. Her career as a film actress began in the mid 1990s.', N'https://blazormoviesstg.blob.core.windows.net/images-people/f1bfc798-6d85-4cff-8df7-a4ff1ec3fbd8.jpg', N'1975-09-30 00:00:00', N'Administrator', N'2022-08-16 15:19:36', 0, N'admin@email.com', N'2023-03-09 18:28:06')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (21, N'Marisa Tomei', N'Born  in Brooklyn, New York. She dropped her education at Boston University for a co-starring role on the CBS daytime drama ""As the World Turns"" (1985).', N'https://blazormoviesstg.blob.core.windows.net/images-people/a1e2fd66-7207-4912-b479-6ff06d1d54c5.jpg', N'1964-12-04 00:00:00', N'Administrator', N'2022-08-16 15:20:57', 0, N'admin@email.com', N'2023-03-09 18:31:58')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (22, N'Sarah Dumont', N'Born in San Diego, California. She is known for playing Denise Russo from Scouts Guide to the Zombie Apocalypse, Sequins from Don Jon, etc.', N'https://blazormoviesstg.blob.core.windows.net/images-people/53045a81-fed5-42a1-93a6-e81431f71673.jpg', N'1990-04-10 00:00:00', N'Administrator', N'2022-08-16 15:22:04', 0, N'admin@email.com', N'2023-03-09 18:35:34')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (40, N'Taylor Geare', N'She is best known for Brothers (2009), Inception (2010), and Dream House (2011).', N'https://blazormoviesstg.blob.core.windows.net/images-people/f0e2706b-2403-4b70-9598-856a3aee177b.jpg', N'2001-09-19 00:00:00', N'admin@email.com', N'2023-03-24 10:37:26', 0, N'admin@email.com', N'2023-03-24 10:37:26')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (41, N'Antonia Garrn', N'German fashion model, actress, and humanitarian participated in Spider-Man: Far from Home (2019).', N'https://blazormoviesstg.blob.core.windows.net/images-people/bcdc0438-054e-451d-af61-ebd4e9144662.jpg', N'1992-07-07 00:00:00', N'admin@email.com', N'2023-03-24 13:14:27', 0, N'admin@email.com', N'2023-03-24 13:14:27')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (42, N'Cobie Smulders', N'Born in Vancouver, British Columbia. Started his acting career in television series such as Special Unit 2 (2001) and Jeremiah (2002).', N'https://blazormoviesstg.blob.core.windows.net/images-people/8de772ce-dbe5-4901-8af1-ce5d5f5721ea.jpg', N'1982-04-03 00:00:00', N'admin@email.com', N'2023-03-24 13:37:33', 0, N'admin@email.com', N'2023-03-24 13:37:33')
INSERT INTO [dbo].[People] ([Id], [Name], [Biography], [PictureUrl], [DateOfBirth], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (43, N'Frances O''Connor', N'British-Australian actress and director. She is known for her roles in the films Mansfield Park (1999), Bedazzled (2000), A.I. Artificial Intelligence (2001), etc', N'https://blazormoviesstg.blob.core.windows.net/images-people/49c9bda9-fc67-4a68-a39c-1bf1beedf9e4.jpg', N'1967-06-12 00:00:00', N'admin@email.com', N'2023-03-24 13:48:54', 0, N'admin@email.com', N'2023-03-24 13:48:54')
SET IDENTITY_INSERT [dbo].[People] OFF
");

            /// Movies table
            await AppContext.Database.ExecuteSqlRawAsync(@"
SET IDENTITY_INSERT [dbo].[Movies] ON
INSERT INTO [dbo].[Movies] ([Id], [Title], [Summary], [InTheaters], [TrailerUrl], [ReleaseDate], [PosterPath], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1, N'SpiderMan - Far From Home', N'Following the events of Avengers: Endgame (2019), Spider-Man must step up to take on new threats in a world that has changed forever.', 0, N't06RUxPbp_c', N'2019-07-02 00:00:00', N'https://blazormoviesstg.blob.core.windows.net/images-movies/538f41fe-da3d-4c25-b9be-2496df811cf3.jpg', N'Administrator', N'2022-08-02 09:10:27', 0, N'admin@email.com', N'2023-03-30 22:33:57')
INSERT INTO [dbo].[Movies] ([Id], [Title], [Summary], [InTheaters], [TrailerUrl], [ReleaseDate], [PosterPath], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (2, N'Wonder Woman', N'When a pilot crashes and tells of conflict in the outside world, Diana, an Amazonian warrior in training, leaves home to fight a war discovering her full powers and true destiny.', 1, N'1Q8fG0TtVAY', N'2017-06-02 00:00:00', N'https://blazormoviesstg.blob.core.windows.net/images-movies/1b1a93fe-c05d-4542-9382-ec31d5ee30de.jpg', N'Administrator', N'2022-08-02 09:10:27', 0, N'admin@email.com', N'2023-03-30 22:33:29')
INSERT INTO [dbo].[Movies] ([Id], [Title], [Summary], [InTheaters], [TrailerUrl], [ReleaseDate], [PosterPath], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (3, N'Inception', N'An upcoming American science fiction film written, produced and directed by Christopher Nolan.', 1, N'8hP9D6kZseM', N'2020-10-22 00:00:00', N'https://blazormoviesstg.blob.core.windows.net/images-movies/e8d51e8d-f004-4c4c-a7f5-40af20efacff.jpg', N'Administrator', N'2022-08-02 09:10:27', 0, N'admin@email.com', N'2023-03-30 22:33:07')
INSERT INTO [dbo].[Movies] ([Id], [Title], [Summary], [InTheaters], [TrailerUrl], [ReleaseDate], [PosterPath], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (4, N'Serendipity', N'A couple reunites years after the night they first met, fell in love, and separated, convinced that one day they''d end up together.', 1, N'tC3nf6bna6s', N'2001-10-05 00:00:00', N'https://blazormoviesstg.blob.core.windows.net/images-movies/2ffb7d3a-c068-481c-817b-d84e110cabcb.jpg', N'Administrator', N'2022-08-02 09:10:27', 0, N'admin@email.com', N'2023-03-30 22:33:41')
INSERT INTO [dbo].[Movies] ([Id], [Title], [Summary], [InTheaters], [TrailerUrl], [ReleaseDate], [PosterPath], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1021, N'Don Jon', N'A New Jersey guy dedicated to his family develops unrealistic expectations from watching porn and works to find happiness and intimacy with his potential true love.', 0, N'2A63Ly0Pvpk', N'2013-09-27 00:00:00', N'https://blazormoviesstg.blob.core.windows.net/images-movies/f02b7b3a-40a6-44e1-900b-c1c1d3ecfe27.jpg', N'Unauthenticated User', N'2022-03-07 11:03:18', 0, N'admin@email.com', N'2023-03-30 22:34:11')
INSERT INTO [dbo].[Movies] ([Id], [Title], [Summary], [InTheaters], [TrailerUrl], [ReleaseDate], [PosterPath], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1022, N'The Counselor', N'A rich and successful lawyer, the Counselor, is about to get married to his fiancée but soon finds himself in over his head when he gets involved in drug trafficking.', 0, N'AxeIeDBomrU', N'2030-12-01 00:00:00', N'https://blazormoviesstg.blob.core.windows.net/images-movies/a2b36893-b426-4f43-a98f-41af426b042e.jpg', N'Unauthenticated User', N'2022-03-29 09:38:23', 0, N'admin@email.com', N'2023-03-30 22:32:39')
INSERT INTO [dbo].[Movies] ([Id], [Title], [Summary], [InTheaters], [TrailerUrl], [ReleaseDate], [PosterPath], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (1033, N'Beddazzled', N'Hopeless dweeb Elliot Richards is granted seven wishes by the devil to snare Allison, the girl of his dreams, in exchange for his soul.', 0, N'tfpLEU2YKzc', N'2030-12-01 00:00:00', N'https://blazormoviesstg.blob.core.windows.net/images-movies/43cdf4c4-44b0-428c-b3a5-7eeebaa12a61.jpg', N'Unauthenticated User', N'2022-11-11 11:50:36', 0, N'admin@email.com', N'2023-03-30 22:50:24')
SET IDENTITY_INSERT [dbo].[Movies] OFF
");

            /// MovieCharacters table
            await AppContext.Database.ExecuteSqlRawAsync(@"
SET IDENTITY_INSERT [dbo].[MovieCharacters] ON
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (103, N'Mal', 1, 20, 3, N'admin@email.com', N'2023-03-24 13:19:20', 0, N'admin@email.com', N'2023-03-24 13:19:20')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (104, N'Phillipa', 2, 40, 3, N'admin@email.com', N'2023-03-24 13:19:20', 0, N'admin@email.com', N'2023-03-24 13:19:20')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (105, N'Sara', 1, 19, 4, N'admin@email.com', N'2023-03-24 13:21:47', 0, N'admin@email.com', N'2023-03-24 13:21:47')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (106, N'Halley', 2, 15, 4, N'admin@email.com', N'2023-03-24 13:21:47', 0, N'admin@email.com', N'2023-03-24 13:21:47')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (109, N'Diana', 1, 18, 2, N'admin@email.com', N'2023-03-24 13:24:35', 0, N'admin@email.com', N'2023-03-24 13:24:35')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (110, N'Hippolyta', 2, 17, 2, N'admin@email.com', N'2023-03-24 13:24:35', 0, N'admin@email.com', N'2023-03-24 13:24:35')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (125, N'Laura', 1, 7, 1022, N'admin@email.com', N'2023-03-27 08:59:28', 0, N'admin@email.com', N'2023-03-27 08:59:28')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (126, N'Malkina', 2, 16, 1022, N'admin@email.com', N'2023-03-27 08:59:28', 0, N'admin@email.com', N'2023-03-27 08:59:28')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (138, N'The Devil', 1, 14, 1033, N'admin@email.com', N'2023-03-27 11:19:30', 0, N'admin@email.com', N'2023-03-27 11:19:30')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (139, N'Allison', 2, 43, 1033, N'admin@email.com', N'2023-03-27 11:19:30', 0, N'admin@email.com', N'2023-03-27 11:19:30')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (140, N'Barbara', 1, 2, 1021, N'admin@email.com', N'2023-03-27 11:21:20', 0, N'admin@email.com', N'2023-03-27 11:21:20')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (141, N'Sequins', 2, 22, 1021, N'admin@email.com', N'2023-03-27 11:21:20', 0, N'admin@email.com', N'2023-03-27 11:21:20')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (142, N'May', 1, 21, 1, N'admin@email.com', N'2023-03-27 11:21:43', 0, N'admin@email.com', N'2023-03-27 11:21:43')
INSERT INTO [dbo].[MovieCharacters] ([Id], [CharacterName], [DesignatedOrder], [PersonId], [MovieId], [CreatedBy], [CreatedOn], [IsDeleted], [UpdatedBy], [UpdatedOn]) VALUES (143, N'Maria', 2, 42, 1, N'admin@email.com', N'2023-03-27 11:21:43', 0, N'admin@email.com', N'2023-03-27 11:21:43')
SET IDENTITY_INSERT [dbo].[MovieCharacters] OFF

");

            /// GenreMovie linking table
            await AppContext.Database.ExecuteSqlRawAsync(@"
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (2, 1)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (4, 1)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (5, 1)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1017, 1)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (4, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (5, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1017, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1018, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1019, 2)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (4, 3)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (5, 3)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1016, 3)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1017, 3)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (2, 4)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1014, 4)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1, 1021)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (2, 1021)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1014, 1021)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1, 1022)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1015, 1022)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1016, 1022)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (2, 1033)
INSERT INTO [dbo].[GenreMovie] ([GenresId], [MoviesId]) VALUES (1018, 1033)
");

            /// MoviePerson linking table
            await AppContext.Database.ExecuteSqlRawAsync(@"
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (21, 1)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (42, 1)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (17, 2)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (18, 2)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (20, 3)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (40, 3)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (15, 4)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (19, 4)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (2, 1021)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (22, 1021)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (7, 1022)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (16, 1022)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (14, 1033)
INSERT INTO [dbo].[MoviePerson] ([ActorsId], [MoviesId]) VALUES (43, 1033)
");

            /// AspNetUsers (ApplicationUser) table
            await AppContext.Database.ExecuteSqlRawAsync(@" 
INSERT INTO [dbo].[AspNetUsers] ([Id], [FirstName], [DateOfBirth], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'64b832c1-7a73-4905-8a2b-0d22eff6d557', N'Creator', N'2000-01-01 00:00:00', N'creator@email.com', N'CREATOR@EMAIL.COM', N'creator@email.com', N'CREATOR@EMAIL.COM', 1, N'AQAAAAIAAYagAAAAEKWu6+ITojoZy8oCZ3dIvWXXKsOQN+vFk8oNLvX44l05mi98zz3ijzJbTrw4mU/Ajg==', N'OMIPMF5ZHHP7I4DKB4TIIUBJ3ZLKJ3KE', N'd042537b-6bdf-4af8-be22-14e0053e9f83', NULL, 0, 0, NULL, 1, 0)
INSERT INTO [dbo].[AspNetUsers] ([Id], [FirstName], [DateOfBirth], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'Admin', NULL, N'admin@email.com', N'ADMIN@EMAIL.COM', N'admin@email.com', N'ADMIN@EMAIL.COM', 1, N'AQAAAAIAAYagAAAAEI4hQOmAJCjXF36B1IMGYkb2/mqCI0oWudk4COqeSdQtv55vh90dGaZk3Y/WHkfUVg==', N'MSEFDLONNE45YVJCOBRMTHIEADRMKVKO', N'41c7102d-5444-4766-9a90-1e23348a3e82', NULL, 0, 0, NULL, 1, 0)
INSERT INTO [dbo].[AspNetUsers] ([Id], [FirstName], [DateOfBirth], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) VALUES (N'f9ff09a2-bc86-4b1d-bfea-2dd7db524056', N'Guest', N'2000-01-01 00:00:00', N'guest@mail.com', N'GUEST@MAIL.COM', N'guest@mail.com', N'GUEST@MAIL.COM', 1, N'AQAAAAIAAYagAAAAEFTBGj/R2xwcWNn4eoOy3b+ugUfMQeCrJGbrvXKKAipXuFKLYRGgIGZnzqirpB9P9A==', N'PGOFT3MHTT5OTXW6PDR5GDNZRARISINS', N'25a73c84-9abb-4994-8473-83d6aee42d49', NULL, 0, 0, NULL, 1, 0)
");

            /// AspNetUserClaims table
            await AppContext.Database.ExecuteSqlRawAsync(@"
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] ON
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (72, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'content.creator', N'creator')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (73, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'user.reader', N'reader')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (74, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'content.editor', N'editor')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (75, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'content.cleaner', N'cleaner')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (76, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'user.creator', N'creator')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (77, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'user.editor', N'editor')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (78, N'728f324a-81f1-4c07-a42f-21d13a07af4c', N'user.cleaner', N'cleaner')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (79, N'64b832c1-7a73-4905-8a2b-0d22eff6d557', N'content.creator', N'creator')
INSERT INTO [dbo].[AspNetUserClaims] ([Id], [UserId], [ClaimType], [ClaimValue]) VALUES (80, N'64b832c1-7a73-4905-8a2b-0d22eff6d557', N'user.creator', N'creator')
SET IDENTITY_INSERT [dbo].[AspNetUserClaims] OFF

");

            /// Asynchronously commits all changes made to the database in
            /// the current transaction .
            await transaction.CommitAsync();

            return true;
        }

        #endregion
    }
}


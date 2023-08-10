using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
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
    /// Implements the <see cref="IRepository{TEntity}"/> interface with
    /// general functionality applicable to all data entities and also extends
    /// its functionality with operations that are specific to the entity type
    /// passed to satisfy its type parameter. 
    /// <para>
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in
    /// the result of a query with EF's "Include" extension method. 
    /// </para>
    /// </remarks>
    public interface IMovies : IRepository<Movie>
    {
        #region Post-Create methods

        /// <summary>
        /// Inserts an entity of type Movie to the database. It includes any
        /// related entities and its data. 
        /// </summary>
        /// <param name="movieDto">The DTO that encapsulates the Movie entity
        /// to insert into the database and applicable collections of related
        /// data.
        /// </param>
        /// <returns>The entity successfully inserted into the database
        /// including any related data (data entities).</returns>
        Task<Movie?> CreateAsync(MovieEssentialsDto movieDto);

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Queries the database for items whose property values match the
        /// values extracted from the <see cref="MoviesQueryFilterDto"/>
        /// passed to satisfy its formal input parameter and paginates the
        /// query result adhering to the pagination parameters sent by the
        /// client.
        /// </summary>
        /// <param name="moviesQueryFilterDto">The DTO that encapsulates
        /// values that can be directly related to one or more properties of a
        /// type <see cref="Movie"/> and the parameters to paginate the
        /// response.</param>
        /// <returns>A type that wraps the collection of items after applying
        /// the filtering criteria and the pagination parameters.</returns>
        Task<PaginatedResponseDto<IEnumerable<Movie>>>
            FilterPaginateMoviesAsync(MoviesQueryFilterDto moviesQueryFilterDto);

        /// <summary>
        /// Builds two collections with Movie items for the FlixManager
        /// routable component and encapsulates them in an object of type
        /// FlixManagerDto.
        /// </summary>
        /// <returns>A DTO that encapsulates a collection with Movie items
        /// currently in theaters and one with Movie items that will be
        /// released in theaters.
        /// <para>
        /// It is limited to return a maximum of 4 items per collection
        /// and it does not include related data (entities).
        /// </para>
        /// </returns>
        Task<FlixManagerDto> GetFlixManagerDtoAsync();

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
        /// transfer, prevents passing sensitive information, and makes it easier
        /// for the client to access specific data.
        /// </para> 
        /// </remarks>
        /// <param name="movieId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>A MovieBulletinDto with a combination of properties of
        /// an object of type Movie that include its movie score average
        /// optimized for data transferring and turned into a single
        /// <em>model</em>.
        /// </returns>
        Task<MovieBulletinDto?> GetMovieBulletinDtoAsync(int movieId);

        /// <summary>
        /// Queries the database for a Movie object whose identity property
        /// value matches the value passed to satisfy its formal input
        /// parameter, calculates its <see cref="MovieScore"/> average if
        /// any, and retrieves the movie score record for the current
        /// <see cref="ApplicationUser"/>, if any. 
        /// </summary>
        /// <remarks>
        /// This method is employed by the MovieBulletin routable component for
        /// scenarios where the current <see cref="ApplicationUser"/> has been
        /// authenticated.
        /// <para>
        /// It creates an object of type MovieBulletinDto that optimizes data
        /// transfer, prevents passing sensitive information, and makes it easier
        /// for the client to access specific data.
        /// </para> 
        /// </remarks>
        /// <param name="movieId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>A MovieBulletinDto with a combination of properties of
        /// an object of type Movie that include its movie score average and
        /// the current user's selected movie score optimized for data
        /// transferring and turned into a single <em>model</em>.
        /// </returns>
        Task<MovieBulletinDto?> GetMovieBulletinWithUserScoreDtoAsync(int movieId);

        /// <summary>
        /// Queries the data source for a Movie object whose identity
        /// property value matches the value passed to satisfy its formal
        /// input parameter <em>movieId</em>.
        /// </summary>
        /// <remarks>
        /// It consumes the GetMovieBulletinDtoAsync method to create an object
        /// of type MovieEditDto that optimizes data transfer, prevents passing
        /// sensitive information, and makes it easier for the client to access
        /// specific data.
        /// </remarks>
        /// <param name="movieId">The identity key value to use for querying
        /// the database.</param>
        /// <returns>A MovieEditDto with a combination of properties of an
        /// object of type Movie optimized for data transferring and turned
        /// into a single <em>model</em>.</returns>
        Task<MovieEditDto?> GetMovieEditDtoAsync(int movieId);

        #endregion

        #region Put-Updtate methods

        /// <summary>
        /// Updates all the properties of a Movie object including its
        /// related data (entities). The process first clears the current
        /// values of the entity properties and then persists the new
        /// property values into the database. 
        /// </summary>
        /// <param name="entityId">The identity key value to use for querying
        /// the database.</param>
        /// <param name="dtoWithNewValues">The DTO that encapsulates the
        /// Movie object with the new property values. It includes an array
        /// of Movie.Genre Ids related to the Movie object and a dictionary
        /// with (Person.Id, Person.CharacterName) as (Key, Value) pairs of
        /// the Person items related to the Movie object. </param>
        /// <returns>A MovieEditDto with a combination of properties of
        /// an object of type Movie optimized for data transferring and turned
        /// into a single <em>model</em>. </returns>
        Task<MovieEditDto?> UpdateMovieAsync(
            int entityId, MovieEssentialsDto? dtoWithNewValues);

        #endregion

        #region Delete methods

        /// <summary>
        /// Deletes a Movie object including its related data.
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
        /// <param name="movieId">The primary key property value of the
        /// entity to delete.</param>
        /// <returns>The entity object that was successfully deleted.
        /// </returns>
        Task<Movie?>? DeleteMovieAsync(int movieId);

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
        /// </summary>
        /// <returns>True if the operation to reset the data is successful.
        /// Otherwise false.</returns>
        Task<bool> ResetDatabaseAsync();

        #endregion
    }
}



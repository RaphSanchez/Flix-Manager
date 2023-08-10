using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.DataStore;
using BlazorMovies.Server.FileStorageManager;
using BlazorMovies.Server.Helpers;
using BlazorMovies.Shared.EDM;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using NuGet.Protocol.Core.Types;

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
    /// Creates concrete instances (classes) for each IEntityName interface
    /// exposed in the IUnitOfWork and satisfies their constructor with a
    /// single (unique) instance of the AppDbContext class that represents a
    /// session with the database that can be used to query and save instances
    /// of the data entities. 
    /// <remarks>
    /// <para>
    /// DbContext is a system resource and must be disposed deterministically.
    /// This implementation includes sync and async dispose methods. 
    /// </para>
    /// <para>
    /// Public access modifier required to make it available for the
    /// ConfigureServices() method of the RESTful service (Startup.cs of the
    /// Application/Server-API project or Program.cs for .Net 6 and later). The
    /// application services container is used to inject the IUnitOfWork service
    /// with the UnitOfWork implementation which has access to the database
    /// through your AppDbContext class that derives from DbContext.
    /// </para>
    /// <para>
    /// <strong>The IUnitOfWork service must be configured with a Transient
    /// lifecycle to deterministically dispose the DbContext derived instance
    /// (e.g., AppDbContext) when a business transaction completes.</strong>
    /// The IUnitOfWork interface implements IDisposable and IAsyncDisposable
    /// interfaces for this purpose. Otherwise the DbContext instance will, by
    /// default, continue tracking all the entities that go through it.
    /// </para>
    /// <see href="https://docs.microsoft.com/en-us/ef/ef6/fundamentals/working-with-dbcontext#lifetime">
    /// Working with DbContext - Lifetime
    /// </see>
    /// <para>
    /// UnitOfWork is application specific. 
    /// </para>
    /// </remarks>
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Private backing field for the application specific
        /// DbContext that represents a session with the database.
        /// This is a unique instance that will be used throughout
        /// a complete business transaction that includes one or
        /// more root entities. 
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Flag: has Dispose(bool) method been called? Indicator
        /// of the current state. 
        /// </summary>
        private bool _disposed;

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

        /// <summary>
        /// Represents an instance of a root entity mapped to the
        /// database.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; 
        /// i.e., it can only be instantiated inside the containing
        /// class. 
        /// </remarks>
        public IGenres? Genres { get; private set; }

        /// <summary>
        /// Represents an instance of a root entity mapped to the
        /// database.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; 
        /// i.e., it can only be instantiated inside the containing
        /// class. 
        /// </remarks>
        public IMovies? Movies { get; private set; }

        /// <summary>
        /// Represents an instance of a root entity mapped to the database.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; i.e., it
        /// can only be instantiated inside the containing class. 
        /// </remarks>
        public IPeople? People { get; private set; }

        /// <summary>
        /// Represents an instance of an Identity entity mapped to the database.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; i.e., it can
        /// only be instantiated inside the containing class. 
        /// </remarks>
        public IUsers? Users { get; private set; }

        /// <summary>
        /// Represents an instance of a data entity mapped to the database.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; i.e., it can
        /// only be instantiated inside the containing class. 
        /// </remarks>
        public IMovieScores? MovieScores { get; private set; }

        /// <summary>
        /// Represents an instance of a data entity mapped to the database.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; i.e., it can
        /// only be instantiated inside the containing class. 
        /// </remarks>
        public IPushSubscriptions?
            PushSubscriptions
        { get; private set; }

#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

        /// <summary>
        /// An instance of the  application specific DbContext is passed to
        /// satisfy the constructor of each EfEntityName concrete instance.
        /// However, an EfEntityName instance does not consume this application
        /// specific DbContext directly from its constructor. In turn, an
        /// EfEntityName instance is derived (inherits) from the
        /// <see cref="EfRepository{TEntity}"/> class which consumes the
        /// DbContext instance.
        /// </summary>
        /// <remarks>
        /// <strong>It is the same structure for the IHttpContextAccessor
        /// formal input parameter.</strong>
        /// <para>
        /// The <see cref="EfRepository{TEntity}"/> class declares a field
        /// named Context of type DbContext with a "protected" access modifier
        /// to make it available to any derived (child) class.
        /// </para>
        /// <para>
        /// When the consumer of the UnitOfWork instantiates a DbContext
        /// (AppDbContext), this unique context will be used across all entity
        /// repositories because it is passed all the way up to the parent
        /// generic class <see cref="Repository{TEntity}"/> responsible for
        /// tying everything together with a protected field available to any
        /// child class (EntityRepository).
        /// </para>
        /// </remarks>
        /// <param name="context">The specific derived class to query the
        /// database and persist any changes made to in-memory objects.</param>
        /// <param name="httpContextAccessor">Instance of an
        /// IHttpContextAccessor that provides access to intrinsic
        /// HttpContext.Request, HttpContext.Response, and HttpContext.Server
        /// properties with info about the current Http request/response.
        /// </param>
        /// <param name="userManager">Provides access to the built-in APIs for
        /// managing User in a persistence store.</param>
        /// <param name="signInManager">The API that allows managing
        /// authentication state for an application user.</param>
        /// <param name="fileStorageService">Implementation of the
        /// IFileStorageService that enables upload, download, copy, and delete
        /// operations with the containers responsible for serving images to
        /// the <see cref="Movie"/> and <see cref="Person"/> data entities.
        /// </param>
        /// <param name="optionsAccessor">Custom class that provides access to
        /// the VAPID details (sensitive information stored in application
        /// secrets) to configure the <see cref="PushNotificationsService"/>.
        /// </param>
        public UnitOfWork(AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IFileStorageService fileStorageService,
            IOptions<VapidOptions> optionsAccessor)
        {
            _context = context;

            /// Concrete instances of the IEntityName interfaces exposed in the
            /// IUnitOfWork interface. A single AppDbContext instance serves
            /// the EfEntityName concrete instances during a complete business
            /// transaction.
            ///
            /// It is the same case for the IHttpContextAccessor.
            Genres = new EfGenres(_context, httpContextAccessor);

            Movies = new EfMovies(_context, httpContextAccessor,
                userManager, signInManager, fileStorageService);

            People = new EfPeople(_context, httpContextAccessor);

            Users = new EfUsers(_context, httpContextAccessor, userManager);

            MovieScores = new EfMovieScores(_context, httpContextAccessor,
                userManager);

            PushSubscriptions = new EfPushSubscriptions(_context,
                httpContextAccessor, optionsAccessor);
        }

        /// <summary>
        /// Indicates the end of a Unit Of Work or business
        /// transaction and updates the database with changes made
        /// to in-memory objects
        /// (DbSet<typeparm name="TEntity">&lt;TEntity&gt;</typeparm>). 
        /// </summary>
        /// <returns>The number of state entries that were successfully
        /// written to the database.</returns>
        public async Task<int> PersistToDatabaseAsync()
        {
            if (_context.Database != null)
                return await _context.SaveChangesAsync();
            return 0;
        }

        /// <summary>
        /// Releases unmanaged resources asynchronously; e.g.,
        /// <see cref="AppDbContext"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.
        /// </returns>
        public ValueTask DisposeAsync()
        {
            Dispose(disposing: true);
            /// https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1816
            GC.SuppressFinalize(this);

            /// Set large fields to null.
            Genres = null;
            Movies = null;
            People = null;
            Users = null;
            MovieScores = null;
            PushSubscriptions = null;

            if (_context != null)
            {
                return _context.DisposeAsync();
            }

            /// Update current state flag.
            _disposed = true;

            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Calls the <see cref="Dispose"/> method to release unmanaged
        /// resources synchronously; e.g., <see cref="AppDbContext"/>.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    /// Dispose managed state (managed objects).
                    _context.Dispose();
                }

                /// Set large fields to null.
                Genres = null;
                Movies = null;
                People = null;
                Users = null;
                MovieScores = null;
                PushSubscriptions = null;

                /// Update current state flag.
                _disposed = true;
            }
        }

        /// <summary>
        /// Frees unmanaged resources synchronously; e.g.,
        /// <see cref="AppDbContext"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}




using Microsoft.EntityFrameworkCore;

/// <summary>
/// The application architecture has an Application/Client (UI) project that
/// employs:
/// 
/// 1. An abstract layer Application/Client/ApiServices/IApiService 
/// responsible for encapsulating the details of how to invoke Server-WebApi
/// endpoints (send/receive Http requests/responses).
/// 
/// 2. An abstract layer Application/Client/IEFManager/IUnitOfWork responsible
/// for encapsulating the business logic of the application and the details of
/// how to communicate to the database. 
/// 
/// The Application/Client makes a data request, the IApiService sends the
/// HttpRequest to the Application/Server-Api/Controllers.
/// 
/// The Application/Server-Api controller employs the UnitOfWork business logic
/// methods to query the Application/Server-Api DataStore database and, if
/// necessary, persist any changes made to in-memory objects. 
/// </summary>
/// <remarks>
/// Both abstract layers implement the Repository pattern to expose to the
/// client their higher level interfaces and hide the actual implementation.
/// <para>
/// The operations exposed to the Client (by the IApiService) mirror (same
/// signature) the operations executed by the IUnitOfWork (business logic and
/// database operations).
/// </para>
/// <para>
/// For this reason, both abstract layers employ the IEntityName interfaces to
/// expose and represent data entities which in turn implement a single (unique)
/// <see cref="IRepository{TEntity}"/> interface which establishes a contract
/// for the required general functionality compatible to all data entities. 
/// </para>
/// <para>
/// IEntityName interfaces not only implement <see cref="IRepository{TEntity}"/>,
/// they also extend their functionality with operations that are specific to
/// the entity type passed to satisfy the type parameter.
/// </para>
/// <para>
/// IEntityName interfaces are exposed, and implemented in their own way, by the
/// IApiService and IUnitOfWork interfaces. This means they need a "public"
/// access modifier.
/// </para>
/// https://docs.microsoft.com/en-us/previous-versions/msp-n-p/ff649690(v=pandp.10)?redirectedfrom=MSDN
/// https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
/// </remarks>
namespace BlazorMovies.Client.ApiServices.IRepositories
{
    /// <summary>
    /// Establishes a contract for exposing one IEntityName interface, in the
    /// form of a read-only property, for each data entity mapped to the
    /// database. It is also responsible for persisting to the database any
    /// changes made to in-memory collections (<see cref="DbSet{TEntity}"/>)
    /// during a business transaction. 
    /// <para>
    /// Its implementation (UnitOfWork) uses an instance of a class that
    /// derives from DbContext which is a system resource and must be
    /// disposed. For this reason, it implements the IDisposable and the
    /// IAsyncDisposable interfaces. 
    /// </para>
    /// </summary>
    /// <remarks>
    /// Public access modifier required to make it available for the
    /// ConfigureServices() method of the RESTful service
    /// (Application/Server-API/Program.cs). 
    /// <para>
    /// The project's services container is used to inject the IUnitOfWork
    /// service with the UnitOfWork implementation which has access to the
    /// database through your Application/Server-Api/DataStore/AppDbContext
    /// class that derives from DbContext. 
    /// </para>
    /// <para>
    /// <strong>The IUnitOfWork service must be configured with a transient
    /// lifecycle to dispose deterministically the DbContext derived instance
    /// (e.g., AppDbContext) when a business transaction completes.</strong>
    /// The IUnitOfWork interface implements IDisposable and IAsyncDisposable
    /// interfaces for this purpose. Otherwise the DbContext instance will, by
    /// default, continue tracking all the entities that go through it. 
    /// </para>
    /// </remarks>
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Represents a data entity that the 
        /// Application/Server-Api/Controllers can consume to perform
        /// business logic operations and persist to the database any
        /// changes made to in-memory objects.
        /// </summary>
        public IGenres Genres { get; }

        /// <summary>
        /// Represents a data entity that the 
        /// Application/Server-Api/Controllers can consume to perform
        /// business logic operations and persist to the database any
        /// changes made to in-memory objects.
        /// </summary>
        public IMovies Movies { get; }

        /// <summary>
        /// Represents a data entity that the 
        /// Application/Server-Api/Controllers can consume to perform
        /// business logic operations and persist to the database any
        /// changes made to in-memory objects.
        /// </summary>
        public IPeople People { get; }

        /// <summary>
        /// Represents a data entity that the 
        /// Application/Server-Api/Controllers Users controller can use to
        /// access business logic operations and persist to the database any
        /// changes made to in-memory objects.
        /// </summary>
        public IUsers Users { get; }

        /// <summary>
        /// Represents a data entity that the 
        /// Application/Server-Api/Controllers MovieScores controller can use 
        /// to access business logic operations and persist to the database any
        /// changes made to in-memory objects.
        /// </summary>
        public IMovieScores MovieScores { get; }

        /// <summary>
        /// Represents a data entity that the 
        /// Application/Server-Api/Controllers PushSubscriptions
        /// controller can use to access business logic operations and persist
        /// to the database any changes made to in-memory objects.
        /// </summary>
        public IPushSubscriptions PushSubscriptions { get; }

        /// <summary>
        /// Indicates the end of a Unit Of Work or business transaction
        /// and updates the database with changes made to in-memory
        /// objects (<see cref="DbSet{TEntity}"/>) instances. 
        /// </summary>
        /// <returns>The number of state entries that were successfully
        /// written to the database.</returns>
        Task<int> PersistToDatabaseAsync();
    }
}



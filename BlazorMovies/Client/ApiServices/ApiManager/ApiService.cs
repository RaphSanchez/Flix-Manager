using BlazorMovies.Client.ApiServices.IRepositories;

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
    /// Creates concrete instances (classes) for each IEntityName interface
    /// exposed in the IApiService and satisfies their constructor with a
    /// single (unique) instance of the IApiConnector interface (its
    /// implementation is responsible for building URI endpoints and
    /// sending/receiving Http requests/responses).
    /// </summary>
    /// <remarks>
    /// The implementation (functionality) of the IApiConnector can be easily
    /// replaced by passing a different class to the IApiConnector service
    /// configured in the Application/Client/Program class. 
    /// </remarks>
    public class ApiService : IApiService
    {
        /// <summary>
        /// Represents a single (unique) instance with the functionality
        /// required by the IApiService (available to the client) to
        /// send/receive HTTP requests/responses.
        /// </summary>
        private readonly IApiConnector _apiConnector;

        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api endpoints
        /// to send/receive Http requests/responses safely.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; 
        /// i.e., it can only be instantiated inside the containing
        /// class. 
        /// </remarks>
        public IGenres Genres { get; private set; }

        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api endpoints
        /// to send/receive Http requests/responses safely.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; 
        /// i.e., it can only be instantiated inside the containing
        /// class. 
        /// </remarks>
        public IMovies Movies { get; private set; }

        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api endpoints
        /// to send/receive Http requests/responses safely.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; 
        /// i.e., it can only be instantiated inside the containing
        /// class. 
        /// </remarks>
        public IPeople People { get; private set; }

        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api User
        /// controller endpoints to send/receive Http requests/responses
        /// safely.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; 
        /// i.e., it can only be instantiated inside the containing
        /// class. 
        /// </remarks>
        public IUsers Users { get; private set; }

        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api MovieScore
        /// controller endpoints to send/receive Http requests/responses
        /// safely.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; 
        /// i.e., it can only be instantiated inside the containing
        /// class. 
        /// </remarks>
        public IMovieScores MovieScores { get; private set; }

        /// <summary>
        /// Represents a data entity that the Application/Client can consume to
        /// invoke Application/Server-Api PushSubscriptions controller endpoints
        /// to send/receive Http requests/responses safely.
        /// </summary>
        /// <remarks>
        /// It is a read-only property with private setter method; i.e., it can
        /// only be instantiated inside the containing class. 
        /// </remarks>
        public IPushSubscriptions PushSubscriptions { get; private set; }

        /// <summary>
        /// The desired implementation of the IApiConnector is passed to the
        /// constructor of each ApiEntityName concrete instance.
        /// <remarks>
        /// <para>
        /// However, an ApiEntityName instance does not consume this
        /// IApiConnector instance directly from its constructor. Instead,
        /// each EntityName instance is derived from the ApiRepository class
        /// which consumes the IApiConnector instance and makes it available
        /// through an ApiConnector "protected" field.
        /// </para>
        /// <para>
        /// This unique IApiConnector instance is used across all ApiEntityName
        /// instances for tying everything together. The structure also allows
        /// to create Http requests/responses using a single (unique)
        /// Http Client instance to avoid socket exhaustion under heavy loads.
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="apiConnector">The desired implementation of the class
        /// that encapsulates the functionality for the <em>resource methods</em>
        /// that serialize/deserialize Http requests/responses.</param>
        public ApiService(IApiConnector apiConnector)
        {
            _apiConnector = apiConnector;

            /// Concrete instances of the IEntityName interfaces
            /// exposed in the IApiService interface. A single
            /// IApiConnector (HttpClient instance) serves all 
            /// ApiEntityName instances during a complete
            /// Http message communication.
            Genres = new ApiGenres(_apiConnector);
            Movies = new ApiMovies(_apiConnector);
            People = new ApiPeople(_apiConnector);
            Users = new ApiUsers(_apiConnector);
            MovieScores = new ApiMovieScores(_apiConnector);
            PushSubscriptions = new ApiPushSubscriptions(_apiConnector);
        }
    }
}


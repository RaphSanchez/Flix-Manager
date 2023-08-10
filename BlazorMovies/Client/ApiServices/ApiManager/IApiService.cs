using BlazorMovies.Client.ApiServices.IRepositories;

namespace BlazorMovies.Client.ApiServices.ApiManager
{
    /// <summary>
    /// Establishes a contract for exposing one IEntityName interface, in the
    /// form of read-only property, for each data entity mapped to the database.
    /// </summary>
    /// <remarks>
    /// This is the entry point for the UI (or client) to the back-end
    /// (Application/Server-Api).
    /// <para>
    /// Public access modifier required to make it available for the
    /// Application/Client/Program class where it is configured as a service
    /// with a "scoped" lifetime.
    /// </para>
    /// </remarks>
    public interface IApiService
    {
        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api endpoints
        /// to send/receive Http requests/responses safely.
        /// </summary>
        public IGenres Genres { get; }

        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api endpoints
        /// to send/receive Http requests/responses safely.
        /// </summary>
        public IMovies Movies { get; }

        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api endpoints
        /// to send/receive Http requests/responses safely.
        /// </summary>
        public IPeople People { get; }

        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api endpoints
        /// to send/receive Http requests/responses safely.
        /// </summary>
        public IUsers Users { get; }

        /// <summary>
        /// Represents a data entity that the Application/Client
        /// can consume to invoke Application/Server-Api endpoints
        /// to send/receive Http requests/responses safely.
        /// </summary>
        public IMovieScores MovieScores { get; }

        /// <summary>
        /// Represents a data entity that the Application/Client can use to
        /// invoke Application/Server-Api endpoints to send/receive Http
        /// requests/responses safely. 
        /// </summary>
        public IPushSubscriptions PushSubscriptions { get; }
    }
}


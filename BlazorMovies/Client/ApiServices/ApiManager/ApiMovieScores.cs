using System.Text;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;

namespace BlazorMovies.Client.ApiServices.ApiManager
{
    /// <summary>
    /// One application specific ApiEntityName class for each
    /// IEntityName interface exposed in the <see cref="IApiService"/>
    /// interface.
    /// </summary>
    /// <remarks>
    /// It is a subclass of the <see cref="ApiRepository{TEntity}"/>
    /// class which means it inherits its general functionality
    /// applicable to all data entities. 
    /// <para>
    /// This class is application specific and extends its base class
    /// with specific functionality for the type passed as type parameter.
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in
    /// the result of a query with EF's "Include" extension method.
    /// </para>
    /// <para>
    /// Its methods have an "explicit interface implementation" to hide
    /// them from unwanted consumers. 
    /// </para>
    /// </remarks>
    internal class ApiMovieScores : ApiRepository<MovieScore>, IMovieScores
    {
        /// <summary>
        /// The name of the Application/Sever-Api/Controller of the
        /// resource (data entity).
        /// </summary>
        private const string ControllerName = "moviescores";

        /// <summary>
        /// Its formal input parameter <paramref name="apiConnector"/> is not
        /// stored in a local variable because it is not consumed like that.
        /// Instead, it is passed to satisfy its base class's constructor and
        /// it is that parent class which consumes it and also makes it
        /// available to any child class through a field named
        /// <see cref="ApiConnector"/> with a "<c>protected read-only</c>"
        /// access modifier. 
        /// </summary>
        /// <remarks>
        /// This structure ensures that a complete business transaction
        /// can have multiple operations with different entity types 
        /// using a single instance of a class that implements the
        /// <see cref="IApiConnector"/> interface which in turn employs a
        /// single instance of the <see cref="HttpClient"/> class to avoid
        /// exhausting the web sockets under heavy loads.
        /// </remarks>
        /// <param name="apiConnector">Instance responsible for building
        /// the URI to map to the Application/Server-Api controller and
        /// for sending/receiving Http requests/responses.</param>
        public ApiMovieScores(IApiConnector apiConnector)
            : base(ControllerName, apiConnector)
        { }

        #region Post-Create methods

        /// <summary>
        /// Sends an Http request to retrieve the current user from the data
        /// store and either create or update a <see cref="MovieScore"/>
        /// database record using the one passed to satisfy its formal input
        /// parameter.
        /// </summary>
        /// <param name="movieScore">Represents a record in the database table
        /// that stores a <see cref="BlazorMovies.Shared.EDM.Movie"/> rating
        /// selected by an <see cref="ApplicationUser"/>.</param>
        /// <returns>The deserialized JSON content from the Http response
        /// message; i.e., an ActionResult with a StatusCodes.Status200OK and
        /// the object value successfully inserted/updated to the database.
        /// </returns>
        async Task<MovieScore> IMovieScores.HandleScoreAsync(MovieScore movieScore)
        {
            try
            {
                MovieScore insertedMovieScore =
                    await ApiConnector.InvokePostAsync<MovieScore>(
                    movieScore,
                    ControllerName,
                    routeTemplateComplement: null,
                    jwtOptions: JwtOptions.IncludeJWTs);

                return insertedMovieScore;

            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        #endregion

        #region Get-Read methods


        #endregion

        #region Put-Update methods


        #endregion

        #region Delete methods

        #endregion
    }
}


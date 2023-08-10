using BlazorMovies.Shared.EDM;
using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.DataStore;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text;
using BlazorMovies.Shared.Helpers;

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
    internal class EfMovieScores : EfRepository<MovieScore>, IMovieScores
    {
        /// <summary>
        /// Property designed to have access to the unique DbContext inherited
        /// from its generic <see cref="EfRepository{TEntity}"/> parent class.
        /// This technique allows you to use the property for querying the DB
        /// instead of having to make an explicit conversion on each query (on
        /// each method). 
        /// </summary>
        /// <remarks>
        /// <see cref="Context"/> is a read-only protected field inherited from
        /// the parent class. This derived class employs its base class's
        /// DbContext instance as opposed to storing its constructor argument
        /// in a local variable to consume it. 
        /// <para>
        /// Every EfEntityName class that derives from the generic 
        /// <see cref="EfRepository{TEntity}"/> class employs its base class's
        /// DbContext protected field which is unique and represents a single
        /// session with the database that can have multiple operations with
        /// different entity types in a single business transaction. 
        /// </para>
        /// </remarks>
        private AppDbContext? AppContext => Context as AppDbContext;

        /// <summary>
        /// Provides the built-in APIs for managing User in a persistence
        /// store.
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;

        public EfMovieScores(DbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
            : base(context, httpContextAccessor)
        {
            _userManager = userManager;
        }

        #region Post-Create methods

        /// <summary>
        /// Retrieves the current user from the data store and either creates
        /// or updates a <see cref="MovieScore"/> database record using the
        /// one passed to satisfy its formal input parameter.
        /// </summary>
        /// <param name="movieScore">The score value or vote selected
        /// by the <see cref="ApplicationUser"/> for a <see cref="Movie"/>
        /// object.</param>
        /// <returns>The entity being tracked by the current database context.
        /// </returns>
        async Task<MovieScore> IMovieScores.HandleScoreAsync(MovieScore movieScore)
        {
            if (movieScore == null || AppContext?.MovieScores == null)
                return null;

            /// Local reference of the MovieScore object passed from the
            /// front-end.
            MovieScore movieScoreLocal = movieScore;

            #region Method 1 to retrieve ApplicationUser.Id (example)

            ClaimsPrincipal? claimsPrincipal =
                HttpContextAccessor.HttpContext?.User;

            /// IdentityServer maps the IdentityUser.UserName to the
            /// preferred_username claim. The IdentityUser.UserName property
            /// value is set with the email of the application user in the
            /// Application/Server-Api/Areas/Identity/Pages/Account
            /// Register model.
            /// https://stackoverflow.com/questions/51449763/asp-net-identity-and-identityserver4-claims
            Claim? preferredUserNameClaim = claimsPrincipal?.Claims
                .FirstOrDefault(c => c.Type == "preferred_username");

            /// Retrieves the application user associated with the specified
            /// email.
            ApplicationUser user = await _userManager
                .FindByEmailAsync(preferredUserNameClaim?.Value);

            /// The primary key for the application user.
            string userId = user.Id;

            #endregion

            #region Method 2 to retrieve ApplicationUser.Id (consumed)

            /// The primary key for the application user.
            string? dbUserId = HttpContextAccessor.HttpContext?.User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            #endregion

            /// Retrieves the application user associatd with the specified
            /// ApplicationUser.Id (just for illustrative purposes).
            ApplicationUser dbUser = await _userManager
                .FindByIdAsync(dbUserId);

            /// Database query to search for a MovieScore record that matches
            /// the Movie item and the current application user. Returns null
            /// if no matches are found; i.e., if current application user has
            /// not voted for the Movie item.
            MovieScore? dbMovieScore = await AppContext.MovieScores
                .FirstOrDefaultAsync(
                    mS => mS.MovieId == movieScoreLocal.MovieId
                          && mS.UserId == dbUserId);

            /// If no vote (movie score record) is found, insert a new
            /// one to the database. 
            if (dbMovieScore is null)
            {
                movieScoreLocal.UserId = dbUserId;
                movieScoreLocal.Movie = await AppContext.Movies
                    .FirstOrDefaultAsync(m => m.Id == movieScoreLocal.MovieId);

                /// Begins tracking the given entity and other reachable
                /// entities that are not being tracked, in the
                /// EntityState.Added such that they will be inserted into
                /// the database when DbContext.SaveChangesAsync() is
                /// called.
                ///
                /// Returns an EntityEntry that provides access to change
                /// tracking information.
                /// https://docs.microsoft.com/en-us/ef/core/change-tracking/
                EntityEntry<MovieScore> insertedMovieScoreEntry =
                    await AppContext.AddAsync(movieScoreLocal);

                /// Gets the MovieRating entity being tracked by this
                /// EntityEntry.
                MovieScore insertedMovieRating =
                    insertedMovieScoreEntry.Entity;

                return insertedMovieRating;
            }

            /// Otherwise, update the existing vote (movie rating record)
            /// for the Movie object by the current application user.
            dbMovieScore.ScoreDate = movieScoreLocal.ScoreDate;
            dbMovieScore.Score = movieScoreLocal.Score;

            return dbMovieScore;
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




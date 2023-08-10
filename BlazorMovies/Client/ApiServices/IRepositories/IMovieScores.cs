using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Client.ApiServices.IRepositories
{
    /// <summary>
    /// Entry point for the IApiService and IUnitOfWork interfaces because they
    /// expose one IEntityName interface for each data entity type in the
    /// Entity Domain Model (EDM).
    /// </summary>
    /// <remarks>
    /// <see cref="IMovieScores"/> implements the
    /// <see cref="IRepository{TEntity}"/> interface which has general
    /// functionality applicable to all data entities. It also extends the
    /// general functionality with operations that are specific to the entity
    /// type passed to satisfy its type parameter.
    /// <para>
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in the
    /// result of a query with EF's "Include" extension method. 
    /// </para>
    /// </remarks>
    public interface IMovieScores : IRepository<MovieScore>
    {
        #region Post-Create methods

        /// <summary>
        /// Retrieves the current <see cref="ApplicationUser"/> from the
        /// data store and either creates or updates a 
        /// <see cref="MovieScore"/> database record using the one passed
        /// to satisfy its formal input parameter.
        /// </summary>
        /// <param name="movieScore">The movie score or vote selected
        /// by the <see cref="ApplicationUser"/> for a <see cref="Movie"/>
        /// object.</param>
        /// <returns>The MovieScore object value successfully inserted or
        /// updated into the database.</returns>
        Task<MovieScore> HandleScoreAsync(MovieScore? movieScore);

        #endregion

        #region Get-Read methods


        #endregion

        #region Put-Update methods


        #endregion

        #region Delete methods


        #endregion
    }
}


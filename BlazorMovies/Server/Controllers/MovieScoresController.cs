using Microsoft.AspNetCore.Mvc;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Client.ApiServices.IRepositories;
using Microsoft.AspNetCore.Authorization;
using System.Text;
using BlazorMovies.Server.Helpers.ServiceExtensions;
using BlazorMovies.Shared.Helpers;
using Microsoft.AspNetCore.OutputCaching;

namespace BlazorMovies.Server.Controllers
{
    /// <summary>
    /// Responsible for responding to Application/Client Http requests made
    /// for data and/or operations related to the
    /// <see cref="MovieScore"/> type.
    /// </summary>
    /// <remarks>
    /// The <see cref="ApiControllerAttribute"/> enables model binding on the
    /// controller to automatically bind the data from an Http request to the
    /// corresponding action method's parameter(s).
    /// <para>
    /// The <see cref="Route"/> attribute determines the URI of the resource
    /// at the controller level; e.g.,
    /// https://localhost:7077/api/moviescores
    /// </para>
    /// </remarks>
    [ApiController]
    //[OutputCache(PolicyName = nameof(CachingServices.NoCachePolicy))]
    [Route("api/[Controller]")]
    public class MovieScoresController : ControllerBase
    {
        /// <summary>
        /// Exposes one IEntityName interface for each data entity mapped to
        /// the database. It keeps track of changes made to in-memory objects
        /// during a business transaction and persists those changes to the
        /// database when completed.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor requests object instances to the dependency injection
        /// container and uses local variables to store their reference.
        /// </summary>
        /// <param name="unitOfWork">The unit of work that exposes the
        /// available functionality through the IEntityName interfaces.</param>
        public MovieScoresController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Post - Create actions

        /// <summary>
        /// Handles an Http POST request issued to the controller's
        /// route (URI): "POST /api/moviescores/".
        /// </summary>
        /// <param name="movieScore">Route parameter that represents
        /// a record in the database table that stores a
        /// <see cref="BlazorMovies.Shared.EDM.Movie"/> score selected by
        /// an <see cref="ApplicationUser"/>. It is passed to the server
        /// in the request body with JSON format; i.e., implements "model
        /// binding" using the "request body". See
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">
        /// Sources</see>.
        /// </param>
        /// <returns>A type that wraps the Movie object successfully
        /// inserted into the database and a StatusCodes.StatusCode
        /// that informs the user the status of the request.</returns>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{TValue}"/> automatically
        /// serializes the object to JSON format and writes the JSON into the
        /// response body of the response message.
        /// </remarks>
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<MovieScore>> HandleScoreTask(
            [FromBody] MovieScore? movieScore)
        {
            try
            {
                if (movieScore == null)
                {
                    /// Returns a BadRequestResult that produces a 
                    /// StatusCodes.Status400BadRequest response and passes a
                    /// custom message which will be consumed in the fron-end
                    /// to inform the user.
                    return BadRequest("New instance cannot be null or empty.");

                }

                /// Retrieves the current application user from the data store
                /// and either creates or updates a MovieScore database record
                /// using the one passed to satisfy its formal input parameter.
                MovieScore insertedObject =
                    await _unitOfWork.MovieScores
                        .HandleScoreAsync(movieScore);

                /// Prevents any unwanted operations on the database.
                if (insertedObject is null)
                    return UnprocessableEntity("An unexpected error occurred." +
                                               "Please try again.");

                /// Indicates the end of a Unit of Work (business
                /// transaction) and updates the database; i.e., persists
                /// any modifications made to in-memory objects. 
                await _unitOfWork.PersistToDatabaseAsync();

                return Ok(insertedObject);
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and send it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Replaces the exception with the StatusCode with information
                /// of what went wrong to inform the caller (client). 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    $"An unexpected error occurred while trying to create a " +
                    $"movie score record. Please try again.");
            }
        }
    }

    #endregion

    #region Get - Read actions

    #endregion

    #region Put - Update actions

    #endregion

    #region Delete actions

    #endregion
}


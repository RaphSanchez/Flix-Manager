using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Client.Pages.Movies;
using BlazorMovies.Server.FileStorageManager;
using BlazorMovies.Server.Helpers;
using BlazorMovies.Server.Helpers.ServiceExtensions;
using BlazorMovies.Shared.AuthZHelpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BlazorMovies.Server.Controllers
{
    /// <summary>
    /// Responsible for responding to Application/Client Http
    /// requests made for data related to the entity type.
    /// </summary>
    /// <remarks>
    /// The <c>[Route]</c> attribute determines the URI of the resource at
    /// the controller level; e.g.,
    /// https://localhost:44363/api/movies
    /// <para>
    /// The <c>[ApiController]</c> attribute enables model binding on the
    /// controller to automatically bind the data from an Http
    /// request to the corresponding action method's parameter(s).
    /// </para>
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class MoviesController : ControllerBase
    {
        /// <summary>
        /// Exposes one IEntityName interface for each data entity
        /// mapped to the database. It keeps track of changes made
        /// to in-memory objects during a business transaction and
        /// persists those changes to the database when completed.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Encapsulates functionality to upload, download and delete
        /// data objects from a cloud service. For example, images,
        /// documents, files, video, audio, and restore or analysis
        /// data.
        /// </summary>
        private readonly IFileStorageService _fileStorageService;

        /// <summary>
        /// Allows the Application/Server-Api to send push messages to the
        /// servers of the web browsers employed by the end users. The
        /// browsers' push service in turn transmits the message to the end
        /// user. 
        /// </summary>
        private readonly IPushNotificationsService _pushService;

        /// <summary>
        /// Represents a store for cached Http responses.
        /// </summary>
        //private readonly IOutputCacheStore _cacheStore;

        /// <summary>
        /// Constructor requests object instances to the dependency injection
        /// container and uses local variables to store their reference.
        /// </summary>
        /// <param name="unitOfWork">The unit of work that exposes the
        /// available functionality through the IEntityName interfaces.</param>
        /// <param name="fileStorageService">Service to upload, download,
        /// and delete data objects from a cloud service.</param>
        /// <param name="cacheStore">Represents a store for cached Http
        /// responses.</param>
        /// <param name="pushService">Service to send push messages to the
        /// servers of the web browsers employed by the end users.</param>
        public MoviesController(IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            IPushNotificationsService pushService
            /*IOutputCacheStore cacheStore*/)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _pushService = pushService;

            //_cacheStore = cacheStore;
        }

        #region Post-Create actions

        /// <summary>
        /// Handles an Http POST request issued to the controller's
        /// route (URI): "POST /api/movies/".
        /// </summary>
        /// <param name="movieDto">Route parameter expecting a
        /// Data Transfer Object type that encapsulates the new
        /// Movie object to insert to the database and its related
        /// data (entities). It is passed to the server in the
        /// request body with JSON format; i.e., implements "model
        /// binding" using the "request body".
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">
        /// Sources</see>
        /// </param>
        /// <returns>A type that wraps the Movie object successfully
        /// inserted into the database and a StatusCodes.StatusCode
        /// that informs the user the status of the request.</returns>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{TValue}"/> automatically
        /// serializes the object to JSON format and writes the JSON into the
        /// response body of the response message.
        /// </remarks>
        [Authorize(Policy = AuthZPolicies.ApiCreateContent)]
        [HttpPost]
        public async Task<ActionResult<Movie>> Add(
            [FromBody] MovieEssentialsDto movieDto)
        {
            try
            {
                if (movieDto == null)
                {
                    /// Returns a BadRequestResult that produces a 
                    /// StatusCodes.Status400BadRequest response and passes a
                    /// custom message which will be consumed in the fron-end
                    /// to inform the user.
                    return BadRequest("New instance cannot be null or empty.");
                }

                /// Verifies that the data provided in the request body is
                /// not already in use. The test for the condition passed is
                /// case insensitive.
                /// https://docs.microsoft.com/en-us/ef/ef6/saving/validation
                /// YouTube: Kudvenkat Post in Asp Net Core REST API:
                /// https://youtu.be/XF6Pcst5SX8
                IEnumerable<Movie?> names =
                    (await _unitOfWork.Movies.GetAllAsync())
                    .Where(m => m?.Title?.Trim() == movieDto.Movie?.Title?.Trim());

                if (names.Any())
                {
                    /// Adds the specified "errorMessage" to the 
                    /// ModelStateEntry.Errors associated with the specified
                    /// key (property). 
                    /// https://docs.microsoft.com/en-us/ef/ef6/saving/validation
                    ModelState.AddModelError("Title",
                        $"{nameof(Movie.Title)} title already exists.");

                    /// Returns a BadRequestResult that produces a 
                    /// StatusCodes.Status400BadRequest response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user. 
                    return BadRequest("Movie title already exists.");
                }

                /// Handles converting Person image into a byte array,
                /// storing it, and overwriting the Person.PictureUrl
                /// property value with the URL that points to the
                /// storage location.
                /// 
                /// Check Application/Server/Program to determine the
                /// type of FileStorageService currently activated; e.g.,
                /// InAppStorage or AzureStorage.
                if (!string.IsNullOrEmpty(movieDto.Movie?.PosterPath))
                {
                    /// Converts the Movie.PosterPath into a byte[].
                    /// 
                    /// The MovieForm employs the UploadImage component to
                    /// handle image files selected by the user. The selected
                    /// image is encoded in Base64 format to be embedded into
                    /// the web browser.
                    byte[] newInstancePicture = Convert
                        .FromBase64String(movieDto.Movie?.PosterPath!);

                    /// Employs the IFileStorageService to upload the current
                    /// Movie.PosterPath converted into a byte[] and overwrites
                    /// the property value of the new instance of the Movie
                    /// object with the string representation of the URL that
                    /// points to the cloud service where the data content
                    /// resides (or local directory if InAppStorageService is
                    /// activated).
                    ///
                    /// The URL is stored in the Movie.PosterPath property
                    /// which is persisted into the database when the Movie
                    /// object (newInstance) is inserted into the database.
                    movieDto.Movie!.PosterPath = await _fileStorageService
                        .SaveFile(newInstancePicture, ".jpg", "images-movies");
                }

                /// Uses the Unit of Work to begin tracking the in-memory
                /// operations performed on the data entity encapsulated
                /// by the MovieBulletinDto object as argument in the route
                /// parameter (request body) of this action (method). It
                /// includes its related data. 
                Movie? insertedObject =
                    await _unitOfWork.Movies
                        .CreateAsync(movieDto);

                /// Prevents any unwanted operations on the database.
                if (insertedObject is null)
                    return UnprocessableEntity("An unexpected error occurred." +
                                               "Please try again.");

                /// Indicates the end of a Unit of Work (business
                /// transaction) and updates the database; i.e., persists
                /// any modifications made to in-memory objects. 
                await _unitOfWork.PersistToDatabaseAsync();

                if (insertedObject.InTheaters)
                {
                    /// Sends a push message to the servers of the user agents
                    /// (computer program representing a person; e.g., a
                    /// browser) of any users subscribed to the notification
                    /// service. The message contains the title, image, and
                    /// URL of the Movie object successfully inserted.
                    await _pushService
                        .SendPushMessageMovieOnTheatersAsync(insertedObject);
                }

                /// Evicts cached Http responses for the Movies controller
                /// actions.
                //await EvictCacheEntriesAsync();

                /// On a successful post (creation of an object), 3 things 
                /// should be performed: 
                /// 1. Return the Http StatusCodes.StatusCode201Created.
                /// 2. Add a "location header" to the response to specify
                ///     the URI where the newly created object is
                ///     available. 
                /// 3. Return the newly created resource (object value) in
                ///     JSON format.
                /// Built-in CreatedAtAction method meets all 3 requirements.
                /// <remarks>
                /// To create the "location header" we pass:
                /// 1. The name of the action (method) to use for generating
                ///     the location header's URI.
                /// 2. The route values of the action used. Note that the
                ///     "id" identifier matches the route parameter name "id"
                ///     of the action (method). Its value is obtained from
                ///     the insertedObject.Id property value.
                /// </remarks>
                /// YouTube "Post in ASP NET Core REST API" by Kudvenkat:
                /// https://youtu.be/XF6Pcst5SX8 
                /// https://docs.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-2.2#actionresultt-type
                return CreatedAtAction(nameof(GetMovieBulletinDtoTask),
                    new { id = insertedObject?.Id },
                    insertedObject);
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
                    $"movie record. Please try again.");
            }
        }

        #endregion

        #region Get-Read actions

        /// <summary>
        /// Handles an <c>Http POST</c> request issued to the controller's
        /// route (URI): "POST /api/movies/filter"
        /// </summary>
        /// <remarks>
        /// It decorates its formal input parameter with a <c>[FromBody]</c>
        /// binding source attribute; i.e., it delegates to the model binding
        /// system the responsibility of converting, and mapping, these
        /// parameter values represented as strings to .Net types.
        /// <para>
        /// Even though its ultimate purpose is to get-retrieve-read data, it
        /// defines an <c>[HttpPost]</c> Http verb because "a payload within a
        /// GET request message has no defined semantics; sending a payload on
        /// a GET request might cause some existing implementations to reject
        /// the request".
        /// </para>
        /// <para>
        /// For more info visit
        /// <see href="https://stackoverflow.com/questions/978061/http-get-with-request-body">
        /// HTTP GET with request body</see>.
        /// </para>
        /// <para>
        /// If successful, the <see cref="ActionResult{TValue}"/> automatically
        /// serializes the object to JSON format and writes the JSON into the
        /// response body of the response message.
        /// </para>
        /// </remarks>
        /// <param name="moviesQueryFilterDto">The DTO that encapsulates
        /// values that can be directly related to one or more properties of a
        /// type <see cref="Movie"/> and the parameters to paginate the
        /// response.</param>
        /// <returns>A type that wraps the collection of items after applying
        /// the filtering criteria and the pagination parameters.</returns>
        [AllowAnonymous]
        [HttpPost("filter")]
        public async Task<ActionResult<PaginatedResponseDto<IEnumerable<Movie>>>>
            FilterPaginateMoviesTask(
                [FromBody] MoviesQueryFilterDto moviesQueryFilterDto)
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the collection of items adhering to the conditions specified
                /// in the PaginationRequestDto.
                ///
                /// It consumes the entity specific
                /// EfMovies/FilterPaginateMoviesAsync method which means it
                /// includes its related data in the response.
                PaginatedResponseDto<IEnumerable<Movie>> paginatedRecords =
                    await _unitOfWork.Movies
                            .FilterPaginateMoviesAsync(moviesQueryFilterDto);

                /// If no matches were found, it is a common convention to
                /// return a NotFoundResult that produces a
                /// StatusCodes.Status404NotFound response.
                ///
                /// Nevertheless, the application employs a
                /// TestCollectionNullOrEmpty component to render collections
                /// of items to the client. This component has RenderFragment
                /// parameters for situations where a collection is either
                /// "null" or "empty". In any case, the component expects a
                /// collection of items (empty or not). Otherwise, it will
                /// most likely throw an exception. For this reason, we always
                /// return an OkObjectResult with a collection of items. 
                if (!paginatedRecords.ResponseData.Any())
                {
                    /// Returns a NotFound result that produces a
                    /// StatusCodes.Status404NotFound response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return NotFound("No content was found in the database.");
                }

                /// ActionResult<PaginatedResponseDto<IEnumerable<Movie>>>>
                /// automatically serializes the entity objects collection to
                /// JSON format and writes it into the response body of the
                /// response message along with the StatusCodes.Status200OK
                /// response. 
                return Ok(paginatedRecords);
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
                    "An unexpected error occurred while trying to paginate data. " +
                    "Please try again.");
            }
        }

        /// <summary>
        /// Handles an Http GET request issued to the controller's
        /// route (URI): "GET /api/movies".
        /// </summary>
        /// <returns>A type that wraps the collection of items and
        /// a StatusCodes.StatusCode that informs the user the status
        /// of the request. 
        /// </returns>
        /// <remarks>
        /// If successful, the
        /// ActionResult<typeparam name="T">&lt;T&gt;</typeparam>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response 
        /// message.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Movie>>> Get()
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the collection of existing items.
                IEnumerable<Movie?> existingItems =
                    await _unitOfWork.Movies.GetAllAsync();

                /// Returns a NotFound result that produces a
                /// StatusCodes.Status404NotFound response and passes a
                /// custom message which will be consumed in the front-end
                /// to inform the user. 
                if (!existingItems.Any())
                    return NotFound("No content was found in the database.");


                /// ActionResult<IEnumerable<T>> automatically
                /// serializes the entity objects collection to JSON format
                /// and writes it into the response body of the response
                /// message along with the StatusCodes.Status200OK response.
                /// Status codes tell the caller the status of the request. 
                return Ok(existingItems);
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
                    "An unexpected error occurred while retrieving data from " +
                    "the database. Please try again.");
            }
        }

        /// <summary>
        /// Handles an Http GET request issued to the controller's
        /// route (URI): "GET /api/movies/flix-manager".
        /// </summary>
        /// <returns>A type that wraps an object ot type
        /// FlixManagerDto and a StatusCodes.StatusCode that informs the
        /// user the status of the request. 
        /// </returns>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{TValue}"/>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response 
        /// message.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("flix-manager")]
        //[OutputCache(PolicyName = nameof(CachingServices.NoCachePolicy))]
        public async Task<ActionResult<FlixManagerDto>> GetFlixManagerDtoTask()
        {
            try
            {
                /// Uses the UnitOfWork to invoke the appropriate functionality
                /// to retrieve from the database the collections of Movie
                /// items required by the FlixManager routable component. 
                FlixManagerDto flixManagerCollections =
                    await _unitOfWork.Movies.GetFlixManagerDtoAsync();

                if (flixManagerCollections is null)
                {
                    /// Returns a NoContentResult that produces an empty
                    /// StatusCodes.Status204NoContent response.
                    return NoContent();
                }

                /// ActionResult<FlixManagerDto> automatically serializes the
                /// FlixManagerDto object to JSON format and writes it into
                /// the response body of the response message along with the
                /// StatusCodes.Status200OK response.
                /// 
                /// Status codes tell the caller the status of the request. 
                return Ok(flixManagerCollections);
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including its name, message, stack trace,
                /// and any inner exceptions. It employs a
                /// <see cref="StringBuilder"/> to construct the information
                /// and send it to the debugging console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Replaces the exception with the StatusCode with information
                /// of what went wrong to inform the caller (client). 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while retrieving data from " +
                    "the database. Please try again.");
            }
        }

        /// <summary>
        /// Handles an Http GET request issued to the controller's
        /// route (URI): "GET /api/movies/id". It is consumed by
        /// the Application/Pages/Movies MovieBulletin component for
        /// scenarios where the user has not been authenticated.
        /// </summary>
        /// <param name="id">Route parameter expecting the entity's
        /// primary key property value with a route constraint of
        /// type Int32. Its value is passed in the URL; i.e.,
        /// implements "model binding" using its route parameters.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">
        /// Sources</see>
        /// </param>
        /// <returns>A type that wraps the object instance and a 
        /// StatusCodes.StatusCode that informs the user the status
        /// of the request.</returns>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{TValue}"/> automatically
        /// serializes the object to JSON format and writes the JSON into
        /// the response body of the response message.
        /// <para>
        /// It is consumed by the <em>Add</em> action to build the
        /// CreatedAtActionResult object. 
        /// </para>
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<MovieBulletinDto>>
            GetMovieBulletinDtoTask([FromRoute] int id)
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the entity whose Id property value matches the route
                /// parameter of this action method. Formal input parameters
                /// are case insensitive. As long as the word matches the
                /// action argument (controller method's argument), it 
                /// will work. 
                MovieBulletinDto? movieBulletinDto = await _unitOfWork.Movies
                    .GetMovieBulletinDtoAsync(id);

                if (movieBulletinDto == null)
                {
                    /// Returns a NotFound result that produces a
                    /// StatusCodes.Status404NotFound response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return NotFound("No content was found in the database.");
                }

                /// ActionResult<T> automatically serializes the foundItem
                /// object to JSON format and writes it into the response
                /// body of the response message along with the
                /// StatusCodes.Status200OK response. Status codes tell the
                /// caller the status of the request.
                return Ok(movieBulletinDto);
            }
            /// Exception could be caught to a database table or a file
            /// for later review. 
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
                    "An unexpected error occurred while retrieving data from " +
                    "the database. Please try again.");
            }
        }

        /// <summary>
        /// Handles an Http GET request issued to the controller's route (URI):
        /// "GET /api/movies/user-score/id". It is consumed by the
        /// Application/Pages/Movies MovieBulletin component for scenarios where
        /// the user has been authenticated.
        /// </summary>
        /// <param name="id">Route parameter expecting the entity's
        /// primary key property value with a route constraint of
        /// type Int32. Its value is passed in the URL; i.e.,
        /// implements "model binding" using its route parameters.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">
        /// Sources</see>
        /// </param>
        /// <returns>A type that wraps the object instance and a 
        /// StatusCodes.StatusCode that informs the user the status
        /// of the request.</returns>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{TValue}"/> automatically
        /// serializes the object to JSON format and writes the JSON into the
        /// response body of the response message.
        /// </remarks>
        [Authorize]
        [HttpGet("user-score/{id:int}")]
        public async Task<ActionResult<MovieBulletinDto>>
            GetMovieBulletinUserRatingDtoTask([FromRoute] int id)
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the entity whose Id property value matches the route
                /// parameter of this action method. Formal input parameters
                /// are case insensitive. As long as the word matches the
                /// action argument (controller method's argument), it 
                /// will work. 
                MovieBulletinDto? movieBulletinDto = await _unitOfWork.Movies
                    .GetMovieBulletinWithUserScoreDtoAsync(id);

                if (movieBulletinDto == null)
                {
                    /// Returns a NotFound result that produces a
                    /// StatusCodes.Status404NotFound response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return NotFound("No content was found in the database.");
                }

                /// ActionResult<T> automatically serializes the foundItem
                /// object to JSON format and writes it into the response
                /// body of the response message along with the
                /// StatusCodes.Status200OK response. Status codes tell the
                /// caller the status of the request.
                return Ok(movieBulletinDto);
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
                    "An unexpected error occurred while retrieving data from " +
                    "the database. Please try again.");
            }
        }

        /// <summary>
        /// Handles an Http GET request issued to the controller's
        /// route (URI): "GET /api/movies/edit/id".
        /// </summary>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{TValue}"/>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response
        /// message.
        /// </remarks>
        /// <param name="id">Route parameter expecting the entity's
        /// primary key property value with a route constraint of
        /// type Int32. Its value is passed in the URL; i.e, implements
        /// "model binding" using its route parameters.
        /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">
        /// Model Binding in ASP.Net Core.</see>
        /// </param>
        /// <returns>A type that wraps the object instance and a
        /// StatusCodes.StatusCode that informs the user the status of the
        /// request.</returns>
        [AllowAnonymous]
        [HttpGet("edit/{id:int}")]
        public async Task<ActionResult<MovieEditDto>> GetMovieEditDtoTask(
            [FromRoute] int id)
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the entity whose id property value matches the route
                /// parameter of this action method. Formal input parameters
                /// are case insensitive. As long as the word matches the
                /// action argument (controller method's argument), it
                /// will work. 
                MovieEditDto? movieEditDto = await _unitOfWork.Movies
                    .GetMovieEditDtoAsync(id);

                if (movieEditDto == null)
                {
                    /// Returns a NotFound result that produces a
                    /// StatusCodes.Status404NotFound response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return NotFound("No content was found in the database.");
                }

                /// ActionResult<T> automatically serializes the foundItem
                /// object to JSON format and writes it into the response
                /// body of the response message along with the
                /// StatusCodes.Status200OK response. Status codes tell the
                /// caller the status of the request.
                return Ok(movieEditDto);
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
                    "An unexpected error occurred while retrieving data from " +
                    "the database. Please try again.");
            }
        }

        #endregion

        #region Put-Update actions

        /// <summary>
        /// Handles an Http PUT request issued to the controller's
        /// route (URI): "PUT /api/movies/id"
        /// </summary>
        /// <param name="id">Route parameter expecting the entity's
        /// primary key property value with a route constraint of
        /// type Int32. Its values are passed in the URL and in the 
        /// body of the request; i.e., implements "model binding"
        /// using its route parameters for the 'id" and its
        /// request body for the actual 'entity' object.
        /// </param>
        /// <param name="dtoWithNewValues">Route parameter expecting
        /// the instance object with the new values that will be updated
        /// in the database. It is passed to the server in the request
        /// body with JSON format.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">Sources</see>
        /// </param>
        /// <returns>A type that wraps the updated item and a
        /// StatusCodes.StatusCode that informs the user the status of
        /// the request.</returns>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{TValue}"/>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response
        /// message. 
        /// </remarks>
        [Authorize(Policy = AuthZPolicies.ApiEditContent)]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<MovieEditDto>> UpdateMovieTask(
            [FromRoute] int id,
            [FromBody] MovieEssentialsDto? dtoWithNewValues)
        {
            try
            {
                /// Local reference of the Movie object with the new property
                /// values.
                Movie? movieWithNewValues = dtoWithNewValues?.Movie;

                /// The route parameter Id value must match the serialized 
                /// object's Id property value. Otherwise, returns a 
                /// BadRequestResult that produces a 
                /// StatusCodes.Status400BadRequest response. Formal input 
                /// parameters are case insensitive, as long as the word
                /// matches the action argument (controller method's
                /// argument), it will work. 
                if (id != movieWithNewValues?.Id)
                    return BadRequest($"{nameof(Movie)} Id mismatch.");

                /// Uses the UnitOfWork to query the database and retrieve
                /// the entity whose Id property value matches the route
                /// parameter value of this action method. 
                Movie? movieToUpdate =
                    await _unitOfWork.Movies.GetByIdAsync(id);

                if (movieToUpdate is null)
                {
                    /// Returns a NotFoundResult that produces a 
                    /// StatusCodes.Status404 not found response.
                    return NotFound(
                        $"{nameof(Movie)} with Id: ({id}) not found.");
                }

                /// If the user selected a new image file for the Movie
                /// object, the current image must be deleted from the
                /// storage service and replaced by the new image. 
                ///
                /// The conditional block handles converting a new image file
                /// selected by the user into a byte array, storing it, and
                /// overwriting the Movie.PosterPath property value with the
                /// URL that points to the storage location. It also deletes
                /// the image previously stored and associated with the
                /// "movieToUpdate".
                /// 
                /// You can check Application/Server/Program class to
                /// determine the type of IFileStorageService currently
                /// activated; e.g., InAppStorage or AzureStorage.
                if (!string.IsNullOrEmpty(movieWithNewValues.PosterPath)
                    && !movieWithNewValues.PosterPath
                        .Equals(movieToUpdate.PosterPath))
                {
                    /// Converts the Movie.PosterPath into a byte[].
                    /// 
                    /// The MovieForm employs the UploadImage component to
                    /// handle image files selected by the user. The selected
                    /// image is encoded in Base64 format to be embedded into
                    /// the web browser.
                    byte[] newInstancePicture = Convert
                        .FromBase64String(movieWithNewValues.PosterPath);

                    /// Employs the IFileStorageService to upload the current
                    /// Movie.PosterPath converted into a byte[] and overwrites
                    /// the property value of the new instance of the Movie
                    /// object with the string representation of the URL that
                    /// points to the cloud service where the data content
                    /// resides (or local directory if InAppStorageService is
                    /// activated).
                    ///
                    /// It also deletes, if any, the image previously stored
                    /// and associated with the "itemToUpdate". 
                    ///
                    /// The URL is stored in the Movie.PosterPath property
                    /// which is persisted into the database when the Movie
                    /// object (newInstance) is inserted into the database.
                    movieWithNewValues.PosterPath = await _fileStorageService
                        .EditFile(
                            newInstancePicture,
                            ".jpg",
                            "images-movies",
                            movieToUpdate.PosterPath);
                }

                /// Uses the UnitOfWork to update in the database the
                /// existing entity's property values passed in the route 
                /// parameter (request body) of this action method. The 
                /// code logic in the UpdateAsync() method does not modify 
                /// the entity's primary key property value. 
                MovieEditDto? updatedItem = await _unitOfWork.Movies
                    .UpdateMovieAsync(id, dtoWithNewValues);

                /// Prevents any unwanted operations on the database.
                if (updatedItem is null)
                    return UnprocessableEntity("An unexpected error occurred." +
                                               "Please try again.");

                /// Indicates the end of a Unit of Work (business
                /// transaction) and updates the database; i.e., persists
                /// any modifications made to in-memory objects. 
                await _unitOfWork.PersistToDatabaseAsync();

                if (updatedItem.Movie!.InTheaters)
                {
                    /// Sends a push message to the servers of the user agents
                    /// (computer program representing a person; e.g., a
                    /// browser) of any users subscribed to the notification
                    /// service. The message contains the title, image, and
                    /// URL of the Movie object successfully updated.
                    await _pushService
                        .SendPushMessageMovieOnTheatersAsync(updatedItem.Movie);
                }

                /// Evicts cached Http responses for the Movies controller
                /// actions.
                //await EvictCacheEntriesAsync();

                /// ActionResult<T> automatically serializes the object
                /// instance to JSON format and writes it into the response
                /// body of the response message along with the
                /// StatusCodes.Status200OK response. Status codes tell the
                /// caller the status of the request.
                return Ok(updatedItem);

            }
            catch (Exception ex)
            {
                /// <remarks>
                /// Application/Repository/EFManager/EfMovies/FilterAsync
                /// throws an InvalidFilterCriteriaException if route template
                /// is not build appropriately in the query string.
                /// </remarks>

                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and send it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Replaces the exception with the StatusCode with information
                /// of what went wrong to inform the caller (client). 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while updating the movie " +
                    "record. Please try again.");
            }
        }

        #endregion

        #region Delete actions

        /// <summary>
        /// Handles an Http DELETE request issued to the controller's
        /// route (URI): "DELETE /api/movies/id".
        /// </summary>
        /// <param name="id">Route parameter expecting the entity's
        /// primary key property value with a route constraint of
        /// type Int32. Its value is passed in the URL; i.e.,
        /// implements "model binding" using its route parameters.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">Sources</see>
        /// </param>
        /// <returns>A type that wraps the content value to format 
        /// to JSON and a StatusCodes.StatusCode that informs the
        /// user the status of the request. 
        /// </returns>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{TValue}"/>
        /// automatically serializes the deleted object to JSON format
        /// and writes the JSON into the response body of the response
        /// message.
        /// </remarks>
        [Authorize(Policy = AuthZPolicies.ApiDeleteContent)]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Movie>> DeleteMovieTask(
            [FromRoute] int id)
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the entity whose Id property value matches the route
                /// parameter value of this action method. Formal input 
                /// parameters are case insensitive, as long as the word 
                /// matches the action argument (controller method's
                /// argument), it will work. 
                Movie? itemToDelete =
                    await _unitOfWork.Movies.GetByIdAsync(id);

                if (itemToDelete is null)
                {
                    /// Returns a NotFoundResult that produces a 
                    /// StatusCodes.Status404 not found response.
                    return NotFound(
                        $"{nameof(Movie)} with Id: ({id}) not found.");
                }

                /// When deleting a Movie object, the image that its
                /// Movie.PosterPath property value points to must also be
                /// removed from the storage service. 
                /// 
                /// You can check the Application/Server/Program class to
                /// determine the type of IFileStorageService currently
                /// activated; e.g., InAppStorage or AzureStorage.
                if (!string.IsNullOrEmpty(itemToDelete.PosterPath))
                {
                    await _fileStorageService.DeleteFile(
                        itemToDelete.PosterPath,
                        "images-movies");
                }

                /// Executes a "soft delete" for the domain entities that have
                /// been provided an "IsActive" discriminator. Otherwise, it 
                /// completely removes the entity from the database. 
                Movie? deletedMovie = await _unitOfWork.Movies
                    .DeleteMovieAsync(id)!;

                /// Prevents any unwanted operations on the database.
                if (deletedMovie is null)
                    return UnprocessableEntity("An unexpected error occurred." +
                                               "Please try again.");

                /// Persist the changes into the database.
                await _unitOfWork.PersistToDatabaseAsync();

                /// Evicts cached Http responses for the Movies controller
                /// actions.
                //await EvictCacheEntriesAsync();

                /// ActionResult<Movie> automatically serializes the Movie
                /// object to JSON format and writes it into the response
                /// body of the response message along with the
                /// StatusCodes.Status200OK response. Status codes tell the
                /// caller the status of the request.
                return Ok(deletedMovie);
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
                    "An unexpected error occurred while removing the movie " +
                    "record. Please try again.");
            }
        }

        #endregion

        #region Reset database actions

        /// <summary>
        /// Handles an Http GET request issued to the controller's route (URI):
        /// "GET /api/movies/reset-db". It is consumed by the
        /// <see cref="FlixManager"/> routable component.
        /// </summary>
        /// <returns>A StatusCodes.StatusCode that informs the user the status
        /// of the request and a boolean value determined by the success of
        /// the operation to reset the database.</returns>
        [AllowAnonymous]
        [HttpGet("reset-db")]
        public async Task<ActionResult<bool>> ResetDatabaseTask()
        {
            try
            {
                /// Resets to an initial state of deployment the entire
                /// application database and the containers responsible to
                /// serve with images the <see cref="Movie"/> and
                /// <see cref="Person"/> data entities . The steps executed
                /// are:
                bool dbResetSuccessful = await
                    _unitOfWork.Movies.ResetDatabaseAsync();

                return Ok(dbResetSuccessful);
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
                    "An unexpected error occurred while attempting to reset the " +
                    "database. Please try again.");
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Invokes the built-in <see cref="IOutputCacheStore.EvictByTagAsync"/>
        /// method to evict cached Http responses from the group of endpoints
        /// identified by the tag passed as an argument; i.e., evicts the cache
        /// entries for the Movies controller actions. 
        /// </summary>
        /// <remarks>
        /// The <see cref="CachingServices.MoviesEndpointsTag"/> is built within
        /// a BasePolicy in the Application/Server-Api/Helpers/ServiceExtensions
        /// CachingServices class.
        /// <para>
        /// This approach allows adhering to the DRY principle. 
        /// </para>
        /// </remarks>
        //private ValueTask EvictCacheEntriesAsync()
        //{
        //    /// Evicts cached responses by tag.
        //    return _cacheStore.EvictByTagAsync(
        //        CachingServices.MoviesEndpointsTag,
        //        CancellationToken.None);
        //}

        #endregion
    }
}



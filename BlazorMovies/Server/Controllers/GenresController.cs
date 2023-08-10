using System.Text;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.Helpers.ServiceExtensions;
using BlazorMovies.Shared.AuthZHelpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

/// <summary>
/// In the context of a REST API a "resource" is an abstraction 
/// of information. Any information that can be named is a resource. 
/// For example, a REST resource can be a document, an image, a
/// temporal service, a collection of other resources, or a non-
/// virtual object such as a Person or a Genre data entity. Data
/// and functionality are considered resources and are accessed
/// using Uniform Resource Identifiers (URIs).
/// 
/// The clients and servers exchange representations of resources
/// by using a standarized interface (API) and protocol (HTTP).
/// The resources have to be decoupled from their representation
/// so that clients can access the content in various formats such
/// as HTML, XML, plain text, PDF, JPEG, JSON, and others.
/// 
/// The state of the resource at any particular time is known as the
/// "resource representation". A resource representation consists of
/// 1. The data.
/// 2. The metadata describing the data to control caching, detect
///     transmission errors, negotiate appropriate representation
///     format, and perform authentication or access control.
/// 3. The hypermedia links that can help the clients transition to
///     the next desired state.
/// 
/// The data format of a representation is known as a "media type" 
/// formerly "MIME type" and sometimes called "content type". The
/// media type identifies a specification that defines how a 
/// representation is to be processed. Your application media type is
/// specified in the constructor of the 
/// Application/Repository/ApiManager/ApiConnector class which defines
/// the code logic to map the Application/Repository/ApiService 
/// (available to the client) to the Application/Server-Api/Controllers
/// (hidden to the client); i.e., it defines generic methods that
/// encapsulate the "resource methods" responsible of performing the
/// desired transition between two states of any resource. 
/// https://restfulapi.net/
/// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.headers.mediatypewithqualityheadervalue?view=net-6.0
/// https://stackoverflow.com/questions/30649347/setting-accept-header-without-using-mediatypewithqualityheadervalue
/// 
/// A REST API consists of an assembly of interlinked resources. This
/// set of resources is known as the REST API's "resource model". 
/// 
/// In a web API controller, each method is considered an 
/// "Action" and each "Action Method" is decorated with an "Http verb
/// attribute" which defines the action to perform.
/// DO NOT!!!!!!!!! include an "Async" suffix to the name of the action
/// methods because ASP.Net Core MVC trims the suffix "Async" from action
/// names by default. This change affects both routing and link generation.
/// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-6.0
/// https://stackoverflow.com/questions/39459348/asp-net-core-web-api-no-route-matches-the-supplied-values
/// </summary>
/// <remarks>
/// The [Route] attribute determines the URI of the resource and each
/// resource is identified by a specific Uniform Resource Identifier
/// (URI); e.g., the collection of Genres is available at the URI:
/// https://localhost:44365/api/genres. The Http verbs that decorate
/// the action methods tell the Server-API what to do with the resource
/// and each action method is directly related to a CRUD operation:
/// Post-Create, Get-Read, Put(full) or Patch(partial)-Update, and
/// Delete-Delete.
/// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
/// https://docs.microsoft.com/en-us/dotnet/framework/wcf/specifying-an-endpoint-address?redirectedfrom=MSDN
/// </remarks>
/// <remarks>
/// Since controller methods are "Actions", it makes sense that they 
/// return action results. There are 5 major sets of Action Results:
/// 1. Status Code Results: return HTTP status code(s) to the client.
/// 2. Status Code w/Object Results: return HTTP status code(s) and 
///     an object value.
/// 3. Redirect Results: redirect the client to another action or to
///     another external resource.
/// 4. File Results: returns a file.
/// 5. Content Results: return various kinds of content. 
/// https://dejanstojanovic.net/aspnet/2018/december/choosing-the-proper-return-type-for-webapi-controller-actions/
/// ActionResult<T> represents the result of an action method (status 
/// codes) along with the data. ActionResult<T> automatically serializes
/// the object to JSON and writes the JSON into the body of the response
/// message. 
/// https://docs.microsoft.com/en-us/aspnet/core/web-api/action-return-types?view=aspnetcore-2.2
/// ActionResult<T> return types can represent a wide range of HTTP 
/// status codes. These Http response status codes are grouped in 5
/// classes: 
/// 1. Informational responses (100-199).
/// 2. Successful responses (200-299).
/// 3. Redirects (300 - 399).
/// 4. Client errors (400 - 499).
/// 5. Server errors (500 - 599). 
/// https://docs.microsoft.com/en-us/troubleshoot/iis/http-status-code
/// https://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html
/// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions?view=aspnetcore-6.0
/// https://en.wikipedia.org/wiki/JSON
/// ActionResult<T> also represents (wraps) the actual data.
/// </remarks>
/// <remarks>
/// For model binding to work, you need to decorate the controller class
/// with an "ApiController" attribute. ASP.Net core automatically binds
/// the data from an Http request to the corresponding action method's 
/// parameter(s).
/// https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0
/// </remarks>
/// <remarks>
/// Suggested resources:
/// https://docs.microsoft.com/en-us/aspnet/mvc/overview/older-versions-1/controllers-and-routing/aspnet-mvc-controllers-overview-cs
/// https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase?view=aspnetcore-6.0
/// https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-6.0&tabs=visual-studio
/// https://docs.microsoft.com/en-us/ef/ef6/saving/validation
/// https://docs.microsoft.com/en-us/dotnet/framework/wcf/specifying-an-endpoint-address?redirectedfrom=MSDN
/// YouTube "Blazor tutorial for beginners" by Kudvenkat:
/// https://youtube.com/playlist?list=PL6n9fhu94yhVowClAs8-6nYnfsOTma14P
/// Working with Query Strings in Blazor by Chris Sainty:
/// https://chrissainty.com/working-with-query-strings-in-blazor/
/// </remarks>
namespace BlazorMovies.Server.Controllers
{
    /// <summary>
    /// Responsible for responding to Application/Client Http
    /// requests made for data related to the entity type.
    /// </summary>
    /// <remarks>
    /// The [Route] attribute determines the URI of the resource
    /// at the controller level; e.g.,
    /// https://localhost:44363/api/genres
    /// <para>
    /// The [ApiController] attribute enables model binding on the
    /// controller to automatically bind the data from an Http
    /// request to the corresponding action method's parameter(s).
    /// </para>
    /// </remarks>
    [Route("api/[controller]")]
    [ApiController]
    public class GenresController : ControllerBase
    {
        /// <summary>
        /// Exposes one IEntityName interface for each data entity
        /// mapped to the database. It keeps track of changes made
        /// to in-memory objects during a business transaction and
        /// persists those changes to the database when completed.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

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
        /// <param name="cacheStore">Represents a store for cached Http
        /// responses.</param>
        public GenresController(IUnitOfWork unitOfWork
            /*IOutputCacheStore cacheStore*/)
        {
            _unitOfWork = unitOfWork;
            //_cacheStore = cacheStore;
        }

        #region Post-Create actions 

        /// <summary>
        /// Handles an Http POST request issued to the controller's
        /// route (URI): "POST /api/genres/".
        /// </summary>
        /// <param name="newInstance">Route parameter expecting the
        /// new object to insert to the database. It is passed to
        /// the server in the request body with JSON format; i.e.,
        /// implements "model binding" using the "request body".
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">
        /// Sources</see>
        /// </param>
        /// <returns>A type that wraps the collection of items and
        /// a StatusCodes.StatusCode that informs the user the
        /// status of the request.</returns>
        /// <remarks>
        /// If successful, the
        /// ActionResult<typeparam name="Genre">&lt;Genre&gt;</typeparam> 
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response 
        /// message.
        /// </remarks>
        [Authorize(Policy = AuthZPolicies.ApiCreateContent)]
        [HttpPost]
        public async Task<ActionResult<Genre>> Add(
            [FromBody] Genre? newInstance)
        {
            try
            {
                if (newInstance == null)
                {
                    /// Returns a BadRequestResult that produces a 
                    /// StatusCodes.Status400BadRequest response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return BadRequest("New instance cannot be null or empty.");
                }

                /// Verifies that the data provided in the request body is
                /// not already in use. The test for the condition passed is
                /// case insensitive.
                /// https://docs.microsoft.com/en-us/ef/ef6/saving/validation
                /// YouTube: Kudvenkat Post in Asp Net Core REST API:
                /// https://youtu.be/XF6Pcst5SX8
                IEnumerable<Genre?> names =
                    (await _unitOfWork.Genres.GetAllAsync())
                    .Where(e => e?.Name == newInstance.Name);

                if (names.Any())
                {
                    /// Adds the specified "errorMessage" to the 
                    /// ModelStateEntry.Errors associated with the specified
                    /// key (property). 
                    /// https://docs.microsoft.com/en-us/ef/ef6/saving/validation
                    ModelState.AddModelError("Name",
                        $"{nameof(Genre.Name)} name already exists.");

                    /// Returns a BadRequestResult that produces a 
                    /// StatusCodes.Status400BadRequest response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user. 
                    return BadRequest("Genre name already exists.");
                }

                /// Uses the UnitOfWork to insert to the database the entity
                /// passed in the route parameter (request body) of this
                /// action method. 
                Genre? insertedObject =
                    await _unitOfWork.Genres.AddAsync(newInstance);

                /// Prevents any unwanted operations on the database.
                if (insertedObject is null)
                    return UnprocessableEntity("An unexpected error occurred." +
                                               "Please try again.");

                /// Object's primary key property value is automatically
                /// generated because the Id column in the database table
                /// database is an identity column and Sql Server produces
                /// it when inserting the object into the database
                /// and persisting the changes.
                await _unitOfWork.PersistToDatabaseAsync();

                /// Evicts cached Http responses for the Genres controller
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
                return CreatedAtAction(nameof(GetById),
                    new { id = insertedObject?.Id },
                    insertedObject);
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
                    $"An unexpected error occurred while trying to create a " +
                    $"genre record. Please try again.");
            }
        }

        #endregion

        #region Get-Read actions

        /// <summary>
        /// Handles an Http GET request issued to the controller's 
        /// route (URI): "GET 
        /// /api/genres/filter?id=someGenreId
        /// &name=someGenreValue
        /// &moviename=someMovieNameValue".
        /// </summary>
        /// <param name="genresQueryFilterDto">The property values
        /// must be passed as a query string inside the URL not as
        /// a route parameter:
        /// /api/genres/filter?id=someGenreId&name=someGenreName&moviename=someMovieName
        /// I.e., it implements "model binding" using the query string.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">Sources</see>
        /// </param>
        /// <returns>A type that wraps the collection of items and
        /// a StatusCodes.StatusCode that informs the user the status
        /// of the request.
        /// </returns>
        /// <remarks>
        /// It is case insensitive.
        /// <para>
        /// The ActionResult<typeparam name="T">&lt;T&gt;</typeparam>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response.
        /// </para>
        /// <para>
        /// Suggested reading: 
        /// <see href="https://chrissainty.com/working-with-query-strings-in-blazor/"/>
        /// </para>
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Genre>>> FilterGenresTask(
            [FromQuery] GenresQueryFilterDto genresQueryFilterDto)
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieves
                /// the items whose property value(s) match the value(s)
                /// passed as arguments to satisfy the formal input parameter.
                /// Formal input parameters are case insensitive, as long as
                /// the word matches the action argument (controller method's 
                /// argument), it will work. 
                IEnumerable<Genre> itemsThatContainValues =
                    await _unitOfWork.Genres.FilterAsync(genresQueryFilterDto);

                /// Returns a NotFound result that produces a
                /// StatusCodes.Status404NotFound response and passes a
                /// custom message which will be consumed in the front-end
                /// to inform the user.
                if (!itemsThatContainValues!.Any())
                    return NotFound("No content was found in the database.");

                /// ActionResult<IEnumerable<T>> automatically serializes
                /// the collection to JSON format and writes it into the
                /// response body of the respose message along with the
                /// StatusCodes.Status200OK response.
                return Ok(itemsThatContainValues);
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
                    "An unexpected error occurred while filtering data. " +
                    "Please try again.");
            }
        }

        /// <summary>
        /// Handles an Http GET request issued to the controller's
        /// route (URI): "GET /api/genres".
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
        //[OutputCache(PolicyName = nameof(CachingServices.NoCachePolicy))]
        public async Task<ActionResult<IEnumerable<Genre>>> Get()
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the collection of existing items.
                IEnumerable<Genre?> existingItems =
                    await _unitOfWork.Genres.GetAllAsync();

                if (!existingItems.Any())
                {
                    /// Returns a NotFound result that produces a
                    /// StatusCodes.Status404NotFound response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return NotFound("No content was found in the database.");
                }

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
                "An unexpected error occurred while retrieving data from the " +
                "database. Please try again.");
            }
        }

        /// <summary>
        /// Handles an Http GET request issued to the controller's
        /// route (URI): "GET /api/genres/id".
        /// </summary>
        /// <param name="id">Route parameter expecting the entity's
        /// primary key property value with a route constraint of
        /// type Int32. Its value is passed in the URL; i.e.,
        /// implements "model binding" using its route parameters.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">Sources</see>
        /// </param>
        /// <returns>A type that wraps the object instance and a 
        /// StatusCodes.StatusCode that informs the user the status
        /// of the request.</returns>
        /// <remarks>
        /// If successful, the
        /// ActionResult<typeparam name="Genre">&lt;Genre&gt;</typeparam>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response
        /// message.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Genre>> GetById([FromRoute] int id)
        {

            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the entity whose Id property value matches the route
                /// parameter of this action method. Formal input parameters
                /// are case insensitive, as long as the word matches the
                /// action argument (controller method's argument), it 
                /// will work. 
                Genre? foundItem = await _unitOfWork.Genres.GetByIdAsync(id);

                if (foundItem == null)
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
                return Ok(foundItem);
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

        #endregion

        #region Put-Update actions

        /// <summary>
        /// Handles an Http PUT request issued to the controller's
        /// route (URI): "PUT /api/genres/id"
        /// </summary>
        /// <param name="id">Route parameter expecting the entity's
        /// primary key property value with a route constraint of
        /// type Int32. Its values are passed in the URL and in the 
        /// body of the request; i.e., implements "model binding"
        /// using its route parameters for the 'id" and its
        /// request body for the actual 'entity' object.
        /// </param>
        /// <param name="instanceWithNewValues">Route parameter expecting
        /// the instance object with the new values that will be updated
        /// in the database. It is passed to the server in the request
        /// body with JSON format.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">Sources</see>
        /// </param>
        /// <returns>A type that wraps the updated item and a
        /// StatusCodes.StatusCode that informs the user the status of
        /// the request.</returns>
        /// <remarks>
        /// If successful, the
        /// ActionResult<typeparam name="Genre">&lt;Genre&gt;</typeparam>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response
        /// message. 
        /// </remarks>
        [Authorize(Policy = AuthZPolicies.ApiEditContent)]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<Genre>> Update(int id,
            [FromBody] Genre instanceWithNewValues)
        {
            try
            {
                /// The route parameter Id value must match the serialized 
                /// object's Id property value. Otherwise, returns a 
                /// BadRequestResult that produces a 
                /// StatusCodes.Status400BadRequest response. Formal input 
                /// parameters are case insensitive, as long as the word
                /// matches the action argument (controller method's
                /// argument), it will work. 
                if (id != instanceWithNewValues.Id)
                    return BadRequest($"{nameof(Genre)} Id mismatch.");

                /// Uses the UnitOfWork to query the database and retrieve
                /// the entity whose Id property value matches the route
                /// parameter value of this action method. 
                Genre? itemToUpdate =
                    await _unitOfWork.Genres.GetByIdAsync(id);

                if (itemToUpdate is null)
                {
                    /// Returns a NotFound result that produces a
                    /// StatusCodes.Status404NotFound response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return NotFound(
                        $"{nameof(Genre)} with Id: ({id}) not found.");
                }

                /// Uses the UnitOfWork to update in the database the
                /// existing entity's property values passed in the route 
                /// parameter (request body) of this action method. The 
                /// code logic in the UpdateAsync() method does not modify 
                /// the entity's primary key property value. 
                Genre? updatedItem = await _unitOfWork.Genres.UpdateAsync(
                    id,
                    instanceWithNewValues);

                /// Prevents any unwanted operations on the database.
                if (updatedItem is null)
                    return UnprocessableEntity("An unexpected error occurred." +
                                               "Please try again.");

                /// Persist the changes into the database.
                await _unitOfWork.PersistToDatabaseAsync();

                /// Evicts cached Http responses for the Genres controller
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
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and send it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Replaces the exception with the StatusCode with information
                /// of what went wrong to inform the caller (client). 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while updating the genre " +
                    "record. Please try again.");
            }
        }

        #endregion

        #region Delete actions

        /// <summary>
        /// Handles an Http DELETE request issued to the controller's
        /// route (URI): "DELETE /api/genres/id".
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
        /// If successful, the
        /// ActionResult<typeparam name="Genre">&lt;Genre&gt;</typeparam>
        /// automatically serializes the deleted object to JSON format
        /// and writes the JSON into the response body of the response
        /// message.
        /// </remarks>
        [Authorize(Policy = AuthZPolicies.ApiDeleteContent)]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Genre>> DeleteGenreTask(
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
                Genre? itemToDelete =
                    await _unitOfWork.Genres.GetByIdAsync(id);

                if (itemToDelete is null)
                {
                    /// Returns a NotFoundResult that produces a 
                    /// StatusCodes.Status404 not found response.
                    return NotFound(
                        $"{nameof(Genre)} with Id: ({id}) not found.");
                }

                /// The root entity type Genre is decorated with the custom
                /// "IsAuditable" attribute and an IsDeletable formal input
                /// parameter; i.e., it implements soft-delete.
                Genre? deletedGenre = await _unitOfWork.Genres
                    .DeleteGenreAsync(id)!;

                /// Prevents any unwanted operations on the database.
                if (deletedGenre is null)
                    return UnprocessableEntity("An unexpected error occurred." +
                                               "Please try again.");

                /// Persist the changes into the database.
                await _unitOfWork.PersistToDatabaseAsync();

                /// Evicts cached Http responses for the Genres controller
                /// actions.
                //await EvictCacheEntriesAsync();

                /// ActionResult<Genre> automatically serializes the Genre
                /// object to JSON format and writes it into the response
                /// body of the response message along with the
                /// StatusCodes.Status200OK response. Status codes tell the
                /// caller the status of the request.
                return Ok(deletedGenre);
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
                    "An unexpected error occurred while removing the genre " +
                    "record. Please try again.");
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Invokes the built-in <see cref="IOutputCacheStore.EvictByTagAsync"/>
        /// method to evict cached Http responses from the group of endpoints
        /// identified by the tag passed as an argument; i.e., evicts the cache
        /// entries for the Genres controller actions. 
        /// </summary>
        /// <remarks>
        /// The <see cref="CachingServices.GenresEndpointsTag"/> is built within
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
        //        CachingServices.GenresEndpointsTag,
        //        CancellationToken.None);
        //}

        #endregion
    }
}


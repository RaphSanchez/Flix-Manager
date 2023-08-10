using System.Text;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.FileStorageManager;
using BlazorMovies.Server.Helpers.ServiceExtensions;
using BlazorMovies.Shared.AuthZHelpers;
using BlazorMovies.Shared.EDM;
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
    /// The [Route] attribute determines the URI of the resource
    /// at the controller level; e.g.,
    /// https://localhost:44363/api/people
    /// <para>
    /// The [ApiController] attribute enables model binding on the
    /// controller to automatically bind the data from an Http
    /// request to the corresponding action method's parameter(s).
    /// </para>
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class PeopleController : ControllerBase
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
        public PeopleController(IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService
            /*IOutputCacheStore cacheStore*/)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            //_cacheStore = cacheStore;
        }

        #region Post-Create actions 

        /// <summary>
        /// Handles an Http POST request issued to the controller's
        /// route (URI): "POST /api/people/".
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
        /// ActionResult<typeparam name="Person">&lt;Person&gt;</typeparam> 
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response 
        /// message.
        /// </remarks>
        [Authorize(Policy = AuthZPolicies.ApiCreateContent)]
        [HttpPost]
        public async Task<ActionResult<Person>> Add(
            [FromBody] Person? newInstance)
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
                IEnumerable<Person?> names =
                    (await _unitOfWork.People.GetAllAsync())
                    .Where(e => e?.Name == newInstance.Name);

                if (names.Any())
                {
                    /// Adds the specified "errorMessage" to the 
                    /// ModelStateEntry.Errors associated with the specified
                    /// key (property). 
                    /// https://docs.microsoft.com/en-us/ef/ef6/saving/validation
                    ModelState.AddModelError("Name",
                        $"{nameof(Person)} name already exists.");

                    /// Returns a BadRequestResult that produces a 
                    /// StatusCodes.Status400BadRequest response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user. 
                    return BadRequest("Actor name already exists.");
                }

                if (!string.IsNullOrEmpty(newInstance.PictureUrl))
                {
                    /// Converts the Person.PictureUrl into a byte[].
                    /// 
                    /// The PersonForm employs the UploadImage component to
                    /// handle image files selected by the user. The selected
                    /// image is encoded in Base64 format to be embedded into
                    /// the web browser.
                    byte[] newInstancePicture = Convert
                        .FromBase64String(newInstance.PictureUrl);

                    /// Employs the IFileStorageService to upload the current
                    /// Person.PictureUrl converted into a byte[] and overwrites
                    /// the property value of the new instance of the Person
                    /// object with the string representation of the URL that
                    /// points to the cloud service where the data content
                    /// resides.
                    ///
                    /// The URL is stored in the Person.PictureUrl property
                    /// which is persisted into the database when the Person
                    /// object (newInstance) is inserted into the database.
                    newInstance.PictureUrl = await _fileStorageService
                        .SaveFile(newInstancePicture, ".jpg", "images-people");
                }

                /// Uses the UnitOfWork to insert to the database the entity
                /// passed in the route parameter (request body) of this
                /// action method. 
                Person? insertedObject =
                    await _unitOfWork.People.AddAsync(newInstance);

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

                /// Evicts cached Http responses for the People controller
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
                    $"person record. Please try again.");
            }
        }

        #endregion

        #region Get-Read actions

        /// <summary>
        /// Handles an Http GET request issued to the controller's 
        /// route (URI):
        /// </summary>
        /// <remarks>
        /// "GET /api/people/filter?id=somePersonId&amp;name=someNameValue
        /// &amp;moviecharactername=someMovieCharacterNameValue".
        /// <para>
        /// It is case insensitive.
        /// </para>
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
        /// <param name="peopleQueryFilterDto">The property values
        /// must be passed as a query string inside the URL not as
        /// a route parameter:
        /// /api/people/filter?id=somePersonId&amp;name=somePersonName&amp;moviecharactername=someCharacterName
        /// I.e., it implements "model binding" using the query string.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">Sources</see>
        /// </param>
        /// <returns>A type that wraps the collection of items and
        /// a StatusCodes.StatusCode that informs the user the status
        /// of the request.
        /// </returns>
        [AllowAnonymous]
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Person>>> FilterPeopleTask(
            [FromQuery] PeopleQueryFilterDto peopleQueryFilterDto)
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieves
                /// the items whose property value(s) match the value(s)
                /// passed as arguments to satisfy the formal input parameter.
                /// Formal input parameters are case insensitive, as long as
                /// the word matches the action argument (controller method's 
                /// argument), it will work. 
                IEnumerable<Person> itemsThatContainValues =
                    await _unitOfWork.People.FilterAsync(peopleQueryFilterDto);

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
            /// Exception could be caught to a database table or a file
            /// for later review. 
            catch (Exception ex)
            {
                /// <remarks>
                /// Application/Repository/EFManager/EfPeople/FilterAsync
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
        /// route (URI): "GET /api/people/id".
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
        /// ActionResult<typeparam name="Person">&lt;Person&gt;</typeparam>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response
        /// message.
        /// </remarks>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Person>> GetById([FromRoute] int id)
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the entity whose Id property value matches the route
                /// parameter of this action method. Formal input parameters
                /// are case insensitive, as long as the word matches the
                /// action argument (controller method's argument), it 
                /// will work. 
                Person? foundItem = await _unitOfWork.People.GetByIdAsync(id);

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


        /// <summary>
        /// Handles an Http GET request issued to the controller's
        /// route (URI):
        /// "GET /api/people?pagenumber=somePageNumberValue
        /// &amp;recordsperpage=someRecordsPerPageValue".
        /// </summary>
        /// <remarks>
        /// Implements <em>pagination</em>; i.e., delivers the data requested
        /// in segments (or portions). 
        /// <para>
        /// It has a [FromQuery] binding source attribute to retrieve the
        /// pagination parameters (which page and how many records per page)
        /// from the query string, convert them to .Net types and map them as
        /// property values of the PaginationRequestDto parameter; i.e.,
        /// implements model binding from the query string.
        /// <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">Sources</see>
        /// </para>
        /// <para>
        /// If successful, the
        /// ActionResult<typeparam name="T">&lt;T&gt;</typeparam>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response 
        /// message.
        /// </para>
        /// </remarks>
        /// <param name="paginationRequestDto">A DTO tha encapsulates the
        /// pagination parameters (which page and how many records per page)
        /// to serve the Http request. 
        /// </param>
        /// <returns>A type that wraps the collection of items, its pagination
        /// metadata, and a StatusCodes.StatusCode that informs the user the
        /// status of the request. 
        /// </returns>
        [AllowAnonymous]
        [HttpGet]
        //[OutputCache(PolicyName = nameof(CachingServices.NoCachePolicy))]
        public async Task<ActionResult<PaginatedResponseDto<IEnumerable<Person>>>>
            GetPeoplePaginatedTask(
                [FromQuery] PaginationRequestDto paginationRequestDto)
        {
            try
            {
                /// Uses the UnitOfWork to query the database and retrieve
                /// the collection of items adhering to the conditions specified
                /// in the PaginationRequestDto.
                ///
                /// It consumes the generic EfRepository/GetPaginatedAsync() method
                /// which means it does not include any related data in the
                /// response.
                PaginatedResponseDto<IEnumerable<Person>> paginatedRecords =
                    await _unitOfWork.People
                            .GetPaginatedAsync(paginationRequestDto);

                if (!paginatedRecords.ResponseData!.Any())
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
                return Ok(paginatedRecords);
            }
            /// This exception handling mechanism is designed to aid in
            /// debugging problems; i.e., only during the development stage
            /// because it is very verbose. It includes all relevant
            /// information in the HttpResponse headers which can be found
            /// using Postman app or the web browser's developer tools.
            /// 
            /// Its result is much more specific and contains considerably
            /// more information. It is not meant to be used in a production
            /// environment but it was left here for illustrative purposes.
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
                    "An unexpected error occurred while trying to paginate " +
                    "data. Please try again.");
            }
        }
        #endregion

        #region Put-Update actions

        /// <summary>
        /// Handles an Http PUT request issued to the controller's
        /// route (URI): "PUT /api/people/id"
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
        /// ActionResult<typeparam name="Person">&lt;Person&gt;</typeparam>
        /// automatically serializes the object to JSON format and
        /// writes the JSON into the response body of the response
        /// message. 
        /// </remarks>
        [Authorize(Policy = AuthZPolicies.ApiEditContent)]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<Person>> Update(int id,
            [FromBody] Person instanceWithNewValues)
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
                    return BadRequest($"{nameof(Person)} Id mismatch.");

                /// Uses the UnitOfWork to query the database and retrieve
                /// the entity whose Id property value matches the route
                /// parameter value of this action method. 
                Person? itemToUpdate =
                    await _unitOfWork.People.GetByIdAsync(id);

                if (itemToUpdate is null)
                {
                    /// Returns a NotFoundResult that produces a 
                    /// StatusCodes.Status404 not found response.
                    return NotFound(
                        $"{nameof(Person)} with Id: ({id}) not found.");
                }

                /// If the user selected a new image file for the Person
                /// object, the current image must be deleted from the storage
                /// service and replaced by the new image.
                /// 
                /// The conditional block handles converting a new image file
                /// selected by the user into a byte array, storing it, and
                /// overwriting the Person.PictureUrl property value with the
                /// URL that points to the storage location.
                ///
                /// You can check the Application/Server/Program class to
                /// determine the type of IFileStorageService currently
                /// activated; e.g., InAppStorage or AzureStorage.
                if (!string.IsNullOrEmpty(instanceWithNewValues.PictureUrl)
                    && !instanceWithNewValues.PictureUrl
                        .Equals(itemToUpdate.PictureUrl))
                {
                    /// Converts the Person.PictureUrl into a byte[].
                    /// 
                    /// The PersonForm employs the UploadImage component to
                    /// handle image files selected by the user. The selected
                    /// image is encoded in Base64 format to be embedded into
                    /// the web browser.
                    byte[] newInstancePicture = Convert
                        .FromBase64String(instanceWithNewValues.PictureUrl);

                    /// Employs the IFileStorageService to upload the current
                    /// Person.PictureUrl converted into a byte[] and overwrites
                    /// the property value of the new instance of the Person
                    /// object with the string representation of the URL that
                    /// points to the cloud service where the data content
                    /// resides (or local directory if InAppStorageService is
                    /// activated).
                    ///
                    /// It also deletes, if any, the image previously stored
                    /// and associated with the "itemToUpdate". 
                    ///
                    /// The URL is stored in the Person.PictureUrl property
                    /// which is persisted into the database when the Person
                    /// object (newInstance) is inserted into the database.
                    instanceWithNewValues.PictureUrl = await _fileStorageService
                        .EditFile(
                            newInstancePicture,
                            ".jpg",
                            "images-people",
                            itemToUpdate.PictureUrl);
                }

                /// Uses the UnitOfWork to update in the database the
                /// existing entity's property values passed in the route 
                /// parameter (request body) of this action method. The 
                /// code logic in the UpdateAsync() method does not modify 
                /// the entity's primary key property value. 
                Person? updatedItem = await _unitOfWork.People.UpdateAsync(
                    id,
                    instanceWithNewValues);

                /// Prevents any unwanted operations on the database.
                if (updatedItem is null)
                    return UnprocessableEntity("An unexpected error occurred." +
                                               "Please try again.");

                /// Persist the changes into the database.
                await _unitOfWork.PersistToDatabaseAsync();

                /// Evicts cached Http responses for the People controller
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
                    "An unexpected error occurred while updating the person " +
                    "record. Please try again.");
            }
        }

        #endregion

        #region Delete actions

        /// <summary>
        /// Handles an Http DELETE request issued to the controller's
        /// route (URI): "DELETE /api/people/id".
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
        /// ActionResult<typeparam name="Person">&lt;Person&gt;</typeparam>
        /// automatically serializes the deleted object to JSON format
        /// and writes the JSON into the response body of the response
        /// message.
        /// </remarks>
        [Authorize(Policy = AuthZPolicies.ApiDeleteContent)]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<Person>> DeletePersonTask(
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
                Person? itemToDelete =
                    await _unitOfWork.People.GetByIdAsync(id)!;

                if (itemToDelete is null)
                {
                    /// Returns a NotFoundResult that produces a 
                    /// StatusCodes.Status404 not found response.
                    return NotFound(
                        $"{nameof(Person)} with Id: ({id}) not found.");
                }

                /// When deleting a Person object, the image that its
                /// Person.PictureUrl property value points to must also be
                /// removed from the storage service. 
                /// 
                /// You can check the Application/Server/Program class to
                /// determine the type of IFileStorageService currently
                /// activated; e.g., InAppStorage or AzureStorage.
                if (!string.IsNullOrEmpty(itemToDelete.PictureUrl))
                {
                    await _fileStorageService.DeleteFile(
                            itemToDelete.PictureUrl,
                            "images-people");
                }

                /// The root entity type Person is decorated with the custom
                /// "IsAuditable" attribute and an IsDeletable formal input
                /// parameter; i.e., it implements soft-delete. 
                Person? deletedPerson = await _unitOfWork.People
                    .DeletePersonAsync(id)!;

                /// Prevents any unwanted operations on the database.
                if (deletedPerson is null)
                    return UnprocessableEntity("An unexpected error occurred." +
                                               "Please try again.");

                /// Persist the changes into the database.
                await _unitOfWork.PersistToDatabaseAsync();

                /// Evicts cached Http responses for the People controller
                /// actions.
                //await EvictCacheEntriesAsync();

                /// ActionResult<Person> automatically serializes the Person
                /// object to JSON format and writes it into the response
                /// body of the response message along with the
                /// StatusCodes.Status200OK response. Status codes tell the
                /// caller the status of the request.
                return Ok(deletedPerson);
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
                    "An unexpected error occurred while removing the person " +
                    "record. Please try again.");
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Invokes the built-in <see cref="IOutputCacheStore.EvictByTagAsync"/>
        /// method to evict cached Http responses from the group of endpoints
        /// identified by the tag passed as an argument; i.e., evicts the cache
        /// entries for the People controller actions. 
        /// </summary>
        /// <remarks>
        /// The <see cref="CachingServices.PeopleEndpointsTag"/> is built within
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
        //        CachingServices.PeopleEndpointsTag,
        //        CancellationToken.None);
        //}

        #endregion
    }
}



using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Shared.EDM;

using Microsoft.AspNetCore.Authorization;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Mvc;

namespace BlazorMovies.Server.Controllers
{
    /// <summary>
    /// Responsible for responding to Application/Client Http requests made for
    /// data related to the entity type <see cref="PushSubscriptionDetails"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="ApiControllerAttribute"/> enables model binding on the
    /// controller to automatically bind the data from an Http request to the
    /// corresponding action method's parameter(s).
    /// <para>
    /// The [<see cref="Route"/>] attribute determines the URI of the resource
    /// at the controller level; e.g., 
    /// https://localhost/44363/api/movies
    /// </para>
    /// <para>
    /// See "Episode 154. Push API - Backend" of Udemy course <see href="https://www.udemy.com/share/101ZK23@WsbIyvVEOB5253Ct28tyRK2KkX6fo6m98gwrjUXTiTtgL450UYBiQg4-AvBHA6a_/">
    /// Programando en Blazor - ASP.Net Core 7
    /// </see> by Felipe Gavilán. 
    /// </para>
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class PushSubscriptionsController : ControllerBase
    {
        /// <summary>
        /// Exposes one IEntityName interface for each data entity
        /// mapped to the database. It keeps track of changes made
        /// to in-memory objects during a business transaction and
        /// persists those changes to the database when completed.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor requests object instances to the dependency injection
        /// container and uses local variables to store their reference.
        /// </summary>
        /// <param name="unitOfWork">The unit of work that exposes the
        /// available functionality through the IEntityName interfaces.</param>
        public PushSubscriptionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Post-Create actions

        /// <summary>
        /// Handles an Http POST request issued to the controller's
        /// route (URI): "POST
        /// /api/pushsubscriptions/push-notifications-subscribe".
        /// </summary>
        /// <remarks>
        /// Note that it is decorated with an [AllowAnonymous] attribute which
        /// means that the Http request must be built without a security JWT.
        /// This allows any user to subscribe to the web push notifications
        /// service.
        /// </remarks>
        /// <param name="pushSubscriptionDetails">Represents a record in the
        /// PushSubscriptionsDetails database table with the required data to
        /// target a specific end user to send a push notification. It is passed
        /// to the server in the request body with JSON format; i.e.,
        /// implements "model binding" using the "request body".
        /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">
        /// Sources</see>.
        /// </param>
        /// <returns>A type that wraps the StatusCodes.StatusCode that informs
        /// the user the status of the request and the object value inserted
        /// into the database table.</returns>
        [HttpPost("push-notifications-subscribe")]
        [AllowAnonymous]
        public async Task<ActionResult<PushSubscriptionDetails>>
            AddPushSubscriptionTask(
                [FromBody] PushSubscriptionDetails? pushSubscriptionDetails)
        {
            try
            {
                if (pushSubscriptionDetails == null)
                {
                    /// Returns a BadRequestResult that produces a 
                    /// StatusCodes.Status400BadRequest response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return BadRequest("Push subscription details cannot be " +
                                      "null or empty.");
                }

                /// Persists the pushSubscriptionDetails into the
                /// PushSubscriptionsDetails database table. It consumes the
                /// Application/Server-Api/Repositories EfPushSubscriptionDetails
                /// repository to access the business logic.
                PushSubscriptionDetails? insertedObject = await _unitOfWork
                    .PushSubscriptions
                        .AddPushSubscriptionAsync(pushSubscriptionDetails);

                /// Object's primary key property value is automatically
                /// generated because the Id column in the database table
                /// database is an identity column and Sql Server produces
                /// it when inserting the object into the database
                /// and persisting the changes.
                await _unitOfWork.PersistToDatabaseAsync();

                /// ActionResult automatically serializes the object value of
                /// the response to JSON format and writes it into the response
                /// body of the response message along with the
                /// StatusCodes.Status200OK response.
                return Ok(insertedObject);
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
                    "An unexpected error occurred while trying to insert a " +
                    "PushSubscriptionDetails record. Please try again.");
            }
        }

        #endregion

        #region Get-Read actions

        /// <summary>
        /// Handles an Http GET request issued to the controller's route
        /// (URI): "GET /api/pushsubscriptions/get-public-key".
        /// </summary>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{TValue}"/> automatically
        /// serializes the object to JSON format and writes the JSON into the
        /// response body of the response message.
        /// <para>
        /// An alternative to this <dfn>action</dfn> controller is to create
        /// a minimal API. Refer to "Episode 154. Push API - Backend" of Udemy
        /// course <see href="https://www.udemy.com/share/101ZK23@FHXR8HROjz7zUmlmb0LdPBR7qBb3ffpRwyKadwoOp833xq3Gp8pRfnTVDJ6xaO9s/">
        /// Programando en Blazor - ASP.Net Core 7
        /// </see> by Felipe Gavilán and
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-7.0">
        /// Tutorial: Create a minimal API with ASP.Net Core</see>.
        /// </para>
        /// </remarks>
        /// <returns>A type that wraps the object value in the response and
        /// a StatusCodes.StatusCode that informs the user the status of the
        /// request. 
        /// </returns>
        [HttpGet("get-public-key")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetVapidPublicKeyTask()
        {
            try
            {
                /// Retrieves the 'public key' of the VAPID details using the
                /// custom PushNotificationOptions class created to store
                /// sensitive data with the application secrets manager.
                string vapidPublicKey =
                    await _unitOfWork.PushSubscriptions
                        .GetVapidPublicKeyAsync();

                if (string.IsNullOrEmpty(vapidPublicKey))
                {
                    /// Returns a NotFound result that produces a
                    /// StatusCodes.Status404NotFound response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return NotFound("No VAPID public key found.");
                }

                /// ActionResult automatically serializes the object value of
                /// the response to JSON format and writes it into the response
                /// body of the response message along with the
                /// StatusCodes.Status200OK response. 
                return Ok(vapidPublicKey);
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
                    "An unexpected error occurred while attempting to retrieve " +
                    "the VAPID public key. Please try again.");
            }
        }

        #endregion

        #region Put-Update actions

        #endregion

        #region Delete actions

        /// <summary>
        /// Handles an Http POST request issued to the controller's route
        /// (URI): "POST
        /// /api/pushsubscriptions/push-notifications-unsubscribe".
        /// </summary>
        /// <remarks>
        /// It is decorated with an [AllowAnonymous] attribute which
        /// means that the Http request must be built without a security JWT.
        /// This allows any user to unsubscribe from the web push notifications
        /// service.
        /// <para>
        /// Note that it is decorated with an <see cref="HttpPostAttribute"/>
        /// action verb because, unlike the HTTP.Client.PostAsJsonAsync, the
        /// HTTPClient.DeleteAsync resource method encapsulated in the
        /// InvokeDeleteAsync method of the <see cref="ApiConnector"/> class
        /// does not have an overload that can take an object value to
        /// serialize and pass it to the backend. 
        /// </para>
        /// <para>
        /// The object value (<see cref="PushSubscriptionDetails"/>) is
        /// required by the controller action because two of its members
        /// (<see cref="PushSubscriptionDetails.P256dh"/> and
        /// <see cref="PushSubscriptionDetails.Auth"/>) are used to retrieve
        /// a matching record from the database.
        /// </para>
        /// </remarks>
        /// <param name="pushSubscriptionDetails">Represents a record in the
        /// PushSubscriptionsDetails database table with the required data to
        /// target a specific end user to send a push notification. It is passed
        /// to the server in the request body with JSON format; i.e.,
        /// implements "model binding" using the "request body".
        /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">
        /// Sources</see>.
        /// </param>
        /// <returns>A type that wraps the PushSubscriptionDetails object
        /// successfully removed from the database and a StatusCodes.StatusCode
        /// that informs the user the status of the request.</returns>
        [HttpPost("push-notifications-unsubscribe")]
        [AllowAnonymous]
        public async Task<ActionResult<PushSubscriptionDetails>>
            DeletePushSubscriptionTask(
                [FromBody] PushSubscriptionDetails? pushSubscriptionDetails)
        {
            try
            {
                if (pushSubscriptionDetails == null)
                {
                    /// Returns a BadRequestResult that produces a 
                    /// StatusCodes.Status400BadRequest response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return BadRequest("Push subscription metadata cannot be " +
                                      "null or empty.");
                }

                /// Removes the pushSubscriptionDetails record from the
                /// PushSubscriptionsDetails database table. 
                PushSubscriptionDetails deletedEntity =
                    await _unitOfWork.PushSubscriptions
                        .DeletePushSubscriptionAsync(
                            pushSubscriptionDetails);

                /// Creates a NotFoundResult with a
                /// StatusCodes.Status404NotFound
                if (deletedEntity == null)
                    return NotFound();

                /// Object's primary key property value is automatically
                /// generated because the Id column in the database table
                /// database is an identity column and Sql Server produces
                /// it when inserting the object into the database
                /// and persisting the changes.
                await _unitOfWork.PersistToDatabaseAsync();

                /// ActionResult automatically serializes the entity object
                /// to JSON format and writes it into the response body of
                /// the response message along with the
                /// StatusCodes.Status200OK response. Status codes tell the
                /// caller the status of the request.
                return Ok(deletedEntity);
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
                    "An unexpected error occurred while trying to remove a " +
                    "PushSubscriptionDetails record. Please try again.");
            }
        }

        #endregion

    }
}


using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Users
{
    /// <summary>
    /// Allows to manage the custom authorization claims registered in the
    /// data store for a given <see cref="ApplicationUser"/>.
    /// </summary>
    public partial class UserEdit
    {
        /// Represents the ApplicationUser.Id and a collection of authorization
        /// claim DTOs. It flags the authorization claims currently assigned to
        /// the user.
        private UserClaimsDto? _userClaimsDto = new();

        /// Represents an <see cref="ApplicationUser"/>.Id and Email.
        UserDto? _userDto = new();

        /// <summary>
        ///  Exposes one IEntityName interface for each data entity mapped to
        /// the database. 
        /// </summary>
        [Inject] private IApiService ApiService { get; set; } = null!;

        /// <summary>
        /// Provides an abstraction for querying and managing URI navigation.
        /// </summary>
        [Inject] private NavigationManager? NavManager { get; set; }

        /// <summary>
        /// Represents an instance of a JavaScript runtime to which calls may
        /// be dispatched. 
        /// </summary>
        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

        /// <summary>
        /// Encapsulates custom methods to handle exceptions with clear
        /// messages to inform the end user. It allows to centralize custom
        /// messages; e.g., messages conveyed to the user when a JSException
        /// is thrown because the user attempts a get, create, update, or
        /// delete operation when the application is offline.
        /// </summary>
        [Inject] private IExceptionHandlers ExHandlers { get; set; } = null!;

        /// The <see cref="ApplicationUser"/>.Id
        [Parameter]
        public string? UserId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _userDto = await ApiService?.Users.GetUserAsync(UserId);

                /// Retrieves a collection of all the custom AuthZClaim items
                /// available for controlling acceess to application resources
                /// and flags the authorization claims currently assigned to
                /// the user passed to  satisfy its formal input parameter.
                _userClaimsDto = await ApiService.Users
                    .GetUserAuthZClaimsAsync(UserId);
            }
            catch (Exception ex)
            {
                /// If it is an HttpRequestException with inner JSException, it
                /// creates a custom message to inform the user that a
                /// connection with the network server is required the first
                /// time an Http GET request attempts to access an
                /// Application/Server-Api resource.
                ///
                /// Otherwise, passes the Exception.Message property value to
                /// informe the user of the unexpected error. 
                await JsRuntime.SwAlDisplayMessageAsync(
                    "Warning",
                    ExHandlers.CreateMessageForFailedGetRequest(ex),
                    SwAlIconType.warning);
            }
        }

        /// Updates the custom authorization claims for application user and
        /// displays an alert dialog box if successful.
        public async Task UpdateUserClaims()
        {
            string message = string.Empty;
            try
            {
                /// Updates the claims assigned to the application user and
                /// returns a type bool that represents the result of the
                /// operation; e.g., true for a successful operation.
                bool result = await ApiService.Users
                    .UpdateUserClaimsAsync(_userClaimsDto);

                /// Provides feedback to the user.
                if (result)
                    await JsRuntime
                        .AlertDialogBox("User claims updated successfully.");
            }
            catch (Exception ex)
            {
                /// If it is an inner JSException, it means that the user is
                /// offline, our PWA application will persist an IndexedDB
                /// record with the data required to send the request during
                /// a synchronization process. 
                if (ex.InnerException != null
                    && ex.InnerException.GetType().ToString()
                        .Contains("JSException"))
                {
                    /// 1. Persists into our custom IndexedDB a record with the
                    /// data required to build the HTTP request to perform an
                    /// update operation during a synchronization process by the
                    /// PwaSync component.
                    /// 
                    /// 2. Calls the event publisher method of the
                    /// UpdateNumberOfPendingOperationsAsync event in the
                    /// Application/Client/Events/ISynchronizationState to
                    /// update the total number of create, update, and delete
                    /// operations stored in our custom IndexedDB that need to
                    /// be synchronized with the web server; i.e., it sends an
                    /// event notification which triggers an update of the
                    /// value for the number of pending operations displayed
                    /// to the user by the bell icon of the PwaSync component.
                    /// 
                    /// 3. Informs the user that the operation was successfully
                    /// stored for synchronization once a connection to the
                    /// network server is reestablished.
                    ///
                    /// The routeTemplateComplement can be obtained from the
                    /// ApiEntityName and/or the controller.
                    await ExHandlers.HandleInnerJSExceptionUpdateAsync(
                        objectToUpdate: _userClaimsDto,
                        controllerName: "users",
                        routeTemplateComplement: "/update-claims");
                }
                else
                {
                    /// Informs the user when an unexpected error occurred in a
                    /// clear and meaningful message.
                    await JsRuntime.SwAlDisplayMessageAsync(
                        "Warning",
                        ex.Message,
                        SwAlIconType.warning);

                    /// Reloads the routable component with empty fields to allow
                    /// the user to try once more. It bypasses client side routing
                    /// and forces the browser to load the new page from the
                    /// server.
                    NavManager?.NavigateTo(NavManager.Uri, true);
                }
            }
        }
    }
}


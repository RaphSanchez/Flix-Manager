using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Genres
{
    /// <summary>
    /// Allows an application user with appropriate credentials to edit a
    /// <see cref="Genre"/> object.
    /// </summary>
    public partial class GenreEdit
    {
        private Genre? _dbGenre = null;

        /// <summary>
        /// Route parameter property value passed by the consumer of
        /// this GenreEdit routable component.
        /// </summary>
        [Parameter]
        public int Id { get; set; }

        /// <summary>
        ///  Exposes one IEntityName interface for each data entity mapped to
        /// the database. 
        /// </summary>
        [Inject] private IApiService? ApiService { get; set; }

        /// <summary>
        /// Provides an abstraction for querying and managing URI navigation.
        /// </summary>
        [Inject] private NavigationManager? NavManager { get; set; }

        /// <summary>
        /// Represents an instance of a JavaScript runtime to which calls may
        /// be dispatched. 
        /// </summary>
        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        /// <summary>
        /// Encapsulates custom methods to handle exceptions with clear
        /// messages to inform the end user. It allows to centralize custom
        /// messages; e.g., messages conveyed to the user when a JSException
        /// is thrown because the user attempts a get, create, update, or
        /// delete operation when the application is offline.
        /// </summary>
        [Inject] private IExceptionHandlers ExHandlers { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _dbGenre = await ApiService?.Genres.GetByIdAsync(Id)!;
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

        private async Task EditGenre()
        {
            try
            {
                /// Updates the properties of the Genre type. It
                /// returns the Genre object with the new values
                /// but it is not consumed here so it is not
                /// stored in a local variable. 
                await ApiService?.Genres
                    .UpdateAsync(_dbGenre!.Id, _dbGenre)!;

                /// If Genre object is updated successfully, the
                /// user is directed to the IndexGenres component.
                NavManager?.NavigateTo("genres");
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
                        objectToUpdate: _dbGenre,
                        controllerName: "genres",
                        routeTemplateComplement: $"/{_dbGenre?.Id}");
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


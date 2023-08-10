using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Client.Shared;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Genres
{
    /// Presents genre records in a table. It provides "Create", "Edit" and
    /// "Delete" button elements for application users with appropriate
    /// credentials.
    public partial class IndexGenres
    {
        /// <summary>
        /// Stores the collection of genres retrieved from the database.
        /// </summary>
        private List<Genre?>? _dbGenres;

        /// <summary>
        /// Custom Confirmation modal component responsible for requiring
        /// a confirmation from the user before deleting a Genre object.
        /// </summary>
        private Confirmation? _confirmation;

        /// <summary>
        /// Stores a reference to the related Genre object captured when
        /// the user raises the @onclick event of the Delete button element.
        /// </summary>
        private Genre? _currentGenre;

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
        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

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
                _dbGenres = (await ApiService?.Genres.GetAllAsync()!)
                    .OrderBy(g => g.Name)
                    .ToList();
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

        /// <summary>
        /// Event handler for the @onclick event of the "Delete" button
        /// element.
        /// </summary>
        /// <param name="genre">The Genre item captured when the @onclick
        /// event of the Delete button element is raised.
        /// </param>
        private void DeleteGenre(Genre? genre)
        {
            /// Stores a reference to the current Genre object.
            _currentGenre = genre;

            /// Executes the Confirmation component.
            _confirmation?.ShowConfirmComponent();
        }

        /// <summary>
        /// Event handler for the OnCancelCallback of the Confirmation
        /// component.
        /// </summary>
        private void CancelDelete()
        {
            _confirmation?.HideConfirmComponent();

            /// Current Genre object captured when the user raises the
            /// @onclick event of the Delete button element. If delete
            /// operation is cancelled, the reference to the Genre object
            /// must be set to null because it is no longer needed.
            _currentGenre = null;
        }

        /// <summary>
        /// Event handler for the OnConfirmCallback of the Confirmation
        /// component.
        /// </summary>
        /// <returns>An asynchronous operation.</returns>
        private async Task PerformDelete()
        {
            try
            {
                /// Removes a Genre object from the database. It returns
                /// the entity object that was successfully removed.
                Genre deletedGenre = await ApiService?.Genres
                    .DeleteAsync(_currentGenre!.Id)!;

                _confirmation?.HideConfirmComponent();

                /// Current Genre object captured when the user raises the
                /// @onclick event of the Delete button element. It delete
                /// operation is successful, the reference to the Genre object
                /// must be set to null because it is no longer needed.
                _currentGenre = deletedGenre != null ? null : _currentGenre;

                /// Reloads the collection of Genre objects available in the
                /// database; i.e., Genre objects are re-rendered and the query
                /// result should not include the Genre object removed.
                _dbGenres = (await ApiService.Genres.GetAllAsync())
                    .ToList();
            }
            catch (Exception ex)
            {
                /// If it is an inner JSException, it means our custom cache
                /// (dynamic-cache) has not stored the Http response. If the
                /// user is offline, our PWA application will persist an
                /// IndexedDB record with the data required to send the request
                /// during a synchronization process. 
                if (ex.InnerException != null
                    && ex.InnerException.GetType().ToString()
                        .Contains("JSException"))
                {
                    /// 1. Persists into our custom IndexedDB a record with the
                    /// data required to build the HTTP request to perform a
                    /// delete operation during a synchronization process by the
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
                    await ExHandlers.HandleInnerJSExceptionDeleteAsync(
                        controllerName: "genres",
                        routeTemplateComplement: $"/{_currentGenre?.Id}");

                    /// Removes the genre from the local collection of Genre
                    /// items displayed to the user.
                    _dbGenres?.Remove(_currentGenre);

                    /// Current Genre object captured when the user raises the
                    /// @onclick event of the Delete button element. If the
                    /// IndexedDB record that represents a delete operation is
                    /// persisted successfully, the reference to the Genre object
                    /// must be set to null because it is no longer needed.
                    _currentGenre = null;
                }
                else
                {
                    /// Informs the user when an unexpected error occurred in a
                    /// clear and meaningful message.
                    await JsRuntime.SwAlDisplayMessageAsync(
                        "Warning",
                        ex.Message,
                        SwAlIconType.warning);

                    /// Reloads the routable component. It bypasses client side
                    /// routing and forces the browser to load the new page from
                    /// the server.
                    NavManager?.NavigateTo(NavManager.Uri, true);
                }
            }
            finally
            {
                _confirmation?.HideConfirmComponent();
            }
        }
    }
}




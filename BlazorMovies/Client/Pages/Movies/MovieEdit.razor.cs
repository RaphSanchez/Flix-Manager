using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Movies
{
    /// <summary>
    /// Allows an application user with appropriate credentials to edit a
    /// <see cref="Movie"/> object.
    /// </summary>
    public partial class MovieEdit
    {
        /// <summary>
        /// The movie object to edit.
        /// </summary>
        private Movie? _movie;

        /// <summary>
        /// Satisfies the UnSelectedGenres parameter with a
        /// collection of Genre items for the MultipleSelector
        /// component.
        /// </summary>
        private List<Genre>? _availableGenres;

        /// <summary>
        /// Satisfies the SelectedGenres parameter with a
        /// collection of Genre items for the MultipleSelector
        /// component. Only MovieEdit routable component should
        /// contain any previously selected items.
        /// </summary>
        private List<Genre>? _selectedGenres;

        /// <summary>
        /// Satisfies the SelectedActors parameter with a
        /// collection of Person items for the TypeaheadDragSelect
        /// component. Only MovieEdit routable component should
        /// contain any previously selected items.
        /// </summary>
        private List<Person>? _selectedActors;

        /// <summary>
        ///  Exposes one IEntityName interface for each data entity mapped to
        /// the database. 
        /// </summary>
        [Inject]
        private IApiService? ApiService { get; set; }

        /// <summary>
        /// Provides an abstraction for querying and managing URI navigation.
        /// </summary>
        [Inject]
        private NavigationManager? NavManager { get; set; }

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
        [Inject]
        private IExceptionHandlers ExHandlers { get; set; } = null!;

        /// <summary>
        /// Route parameter.
        /// </summary>
        [Parameter]
        public int MovieId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                /// Retrieves a DTO with a Movie object and its
                /// related data flattened for data transfer
                /// optimization and easy access.
                MovieEditDto? _movieDto = await ApiService?.Movies
                    .GetMovieEditDtoAsync(MovieId)!;

                /// The Movie object to update or edit.
                _movie = _movieDto?.Movie;

                /// The Genre items related to the Movie object.
                _selectedGenres = _movieDto?.SelectedGenres;

                /// The Genre items available in the database
                /// that are not related to the Movie object.
                _availableGenres = _movieDto?.AvailableGenres;

                /// The Person items related to the Movie object.
                _selectedActors = _movieDto?.Actors;
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

        private async Task EditMovieAsync(MovieEssentialsDto movieDto)
        {
            try
            {
                /// Receives a DTO with the Movie object (and its
                /// related data) after successfully updating its
                /// values.
                MovieEditDto? _movieEditDto = await ApiService?.Movies?
                    .UpdateMovieAsync(MovieId, movieDto)!;

                /// Redirects the user to the MovieBulletin routable component.
                /// It replaces the empty spaces in Movie.Title with dashes (-)
                /// used to build the URL which Blazor inserts to the address
                /// bar of the web browser.
                NavManager?.NavigateTo(
                    $"movies/bulletin/{_movieEditDto?.Movie?.Id}/" +
                    $"{_movieEditDto?.Movie?.Title?.Trim().Replace(" ", "-")}");

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
                        objectToUpdate: movieDto,
                        controllerName: "movies",
                        routeTemplateComplement: $"/{MovieId}");
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

using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Movies
{
    /// <summary>
    /// Allows an application user to create a Movie object.
    /// </summary>
    public partial class MovieCreate
    {
        /// <summary>
        /// Instance of the Movie entity that resides in
        /// BlazorMovies/Shared/EDM.
        /// </summary>
        private readonly Movie _movie = new();

        /// <summary>
        /// The Movie object successfully inserted into the
        /// database and returned within the Http response.
        /// </summary>
        private Movie? _dbResponseMovie;

        /// <summary>
        /// Satisfies the UnSelectedGenres parameter with a
        /// collection of Genre items for the MultipleSelector
        /// component.
        /// </summary>
        /// <remarks>
        /// <strong>Don't instantiate the collection</strong>.
        /// Otherwise, the conditional in the markup becomes
        /// useless and produces a System.NullReferenceException.
        /// </remarks>
        private List<Genre?>? _availableGenres;

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
                /// Retrieves the available Genre items from the
                /// database.
                _availableGenres = (await ApiService?.Genres.GetAllAsync()!)
                    .ToList();

                /// Must be instantiated or throws a
                /// System.NullReferenceException. For
                /// MovieCreate is empty but for MovieEdit
                /// could have previously selected items.
                _selectedGenres = new List<Genre>();

                /// Must be instantiated or throws a
                /// System.NullReferenceException. For
                /// MovieCreate is empty but for MovieEdit
                /// could have previously selected items.
                _selectedActors = new List<Person>();
            }
            catch(Exception ex)
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

        private async Task CreateMovie(MovieEssentialsDto movieDto)
        {
            try
            {
                /// Inserts the Movie entity, provided by the MovieForm fields,
                /// to the database and stores in a local variable the Movie
                /// object returned within the Http response. The Http response
                /// includes the inserted object with a database primary key.
                _dbResponseMovie = await ApiService?.Movies
                    .CreateAsync(movieDto)!;

                /// Redirects the user to the MovieBulletin routable component
                /// which will eventually display the Movie details. It replaces
                /// the Movie.Title white space with dashes (-) that can be
                /// inserted to the address bar of the web browser.
                NavManager?.NavigateTo(
                    $"movies/bulletin/{_dbResponseMovie?.Id}/" +
                    $"{_dbResponseMovie?.Title?.Trim().Replace(" ", "-")}");
            }
            catch(Exception ex)
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
                    /// data required to build the HTTP request to perform a
                    /// create operation during a synchronization process by the
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
                    /// ApiEntityName and/or the controller. Create operations
                    /// do not require a route template complement.
                    await ExHandlers.HandleInnerJSExceptionCreateAsync(
                        objectToCreate: movieDto,
                        controllerName: "movies",
                        routeTemplateComplement: null);
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


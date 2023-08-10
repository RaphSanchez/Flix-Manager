using System.Security.Claims;

using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Movies
{
    /// <summary>
    /// Presents two collections of Movie objects. One with movies currently
    /// in theaters and one with upcoming releases. Each collection is limited
    /// to a max of 4 items.
    /// </summary>
    public partial class FlixManager
    {
        #region Metadata

        private const string PageTitle = "Flix-Manager App";

        private const string Description =
            "Flix-Manager is an application that encompasses, from the ground" +
            " up, all the major subjects that any aspiring full stack .Net" +
            " Blazor developer should know. It explains, provides resources," +
            " and demonstrates the practical implementation of each topic.";

        private const string Author = "Rafael Sanchez";

        #endregion

        /// <summary>
        /// Collection of Movie objects that are currently in theaters.
        /// </summary>
        private List<Movie>? _inTheaters;

        /// <summary>
        /// Collection of Movie objects that will be released in the
        /// future.
        /// </summary>
        private List<Movie>? _upcomingReleases;

        /// <summary>
        /// DTO encapsulates two collections of Movie objects
        /// </summary>
        private FlixManagerDto? _dbFlixManagerDto;

        /// <summary>
        /// Flag used to inform the user while a reset DB operation is in
        /// progress.
        /// </summary>
        private bool _isResettingDb = false;

        /// <summary>
        ///  Exposes one IEntityName interface for each data entity mapped to
        /// the database. 
        /// </summary>
        [Inject] private IApiService? ApiService { get; set; }

        /// <summary>
        /// Represents an instance of a JavaScript runtime to which calls may
        /// be dispatched. 
        /// </summary>
        [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

        /// <summary>
        /// Provides an abstraction for querying and managing URI navigation.
        /// </summary>
        [Inject] private NavigationManager? NavManager { get; set; }

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
                /// Retrieves two collections of Movie objects wrapped into a
                /// FlixManagerDto object.
                _dbFlixManagerDto = await ApiService?.Movies
                    .GetFlixManagerDtoAsync()!;

                _inTheaters = _dbFlixManagerDto.MoviesInTheaters;

                _upcomingReleases = _dbFlixManagerDto.MoviesUpcomingReleases;
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
        /// Handler for the Reset Database button element. It resets to an
        /// initial state of deployment the entire application data and the
        /// containers responsible for serving images to the <see cref="Movie"/>
        /// and <see cref="Person"/> data entities.
        /// </summary>
        /// <remarks>
        /// The IFileStorageService implementation injected in the dependency
        /// injection container of the Application/Server-Api project
        /// determines whether the containers to work with reside in an Azure
        /// Storage Account or in directories of the web root folder that
        /// resides in a physical drive on-premises.
        /// </remarks>
        /// <returns>True if the operation to reset the application data is
        /// successful. Otherwise false.</returns>
        public async Task ResetData()
        {
            /// Confirmation dialog box before resetting data.
            bool dbResetConfirmed =
                await JsRuntime.SwAlConfirmDialogAsync(
                    "Confirmation Dialog",
                    "Are you sure you want to reset the database data?",
                    SwAlIconType.question);

            if (dbResetConfirmed)
            {
                /// If the code has come this far, it means the process to
                /// reset the DB is enabled.
                _isResettingDb = true;

                /// Forces the component to re-render and reflect the is
                /// resetting flag.
                //await InvokeAsync(StateHasChanged);
                StateHasChanged();

                try
                {
                    bool dbResetSuccessful =
                        await ApiService?.Movies.ResetDatabaseAsync()!;

                    if (dbResetSuccessful)
                    {
                        await JsRuntime.SwAlDisplayMessageAsync(
                            "Database successfully reset.");

                        /// Reloads the routable component. It bypasses client side
                        /// routing and forces the browser to load the new page from
                        /// the server.
                        NavManager?.NavigateTo(NavManager.Uri, true);
                    }
                }
                catch (Exception ex)
                {
                    string customExceptionMessage =
                        ex.InnerException != null
                        && ex.InnerException
                            .GetType().ToString().Contains("JSException")
                        ? "You must be online to use this feature."
                        : ex.Message;

                    await JsRuntime.SwAlDisplayMessageAsync(
                        "Info",
                        customExceptionMessage,
                        SwAlIconType.warning);
                }
                finally
                {
                    _isResettingDb = false;
                    StateHasChanged();
                }
            }
        }
    }
}


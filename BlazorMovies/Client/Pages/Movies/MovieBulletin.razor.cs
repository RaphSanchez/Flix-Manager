using System.Security.Claims;

using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Client.Shared;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Movies
{
    /// <summary>
    /// Displays the details of a given <see cref="Movie"/> object. It targets
    /// two different GetMovieBulletinDto endpoints dependent on the
    /// authentication state of the current user. 
    /// </summary>
    /// <remarks>
    /// If the user is not authenticated, casting a vote is not authorized
    /// because there is no way to identify an unauthenticated user to persist
    /// its selected movie rating into the data store.
    /// </remarks>
    public partial class MovieBulletin
    {
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
        /// Route parameter with a reference to the current Movie.Id
        /// property value.
        /// </summary>
        [Parameter]
        public int MovieId { get; set; }

        /// <summary>
        /// Route parameter with a reference to the current Movie.Title
        /// property value.
        /// </summary>
        [Parameter]
        public string? MovieTitle { get; set; }

        /// <summary>
        /// Provides the User's <see cref="ClaimsPrincipal"/>.
        /// </summary>
        /// <remarks>
        /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-6.0">
        /// Procedural logic</see>.
        /// </remarks>
        [CascadingParameter]
        private Task<AuthenticationState> AuthNStateTask { get; set; } = null!;

        /// <summary>
        /// DTO with Movie properties and related data encapsulated into
        /// a single model used to serve this MovieBulletin component.
        /// </summary>
        /// <remarks>
        /// Its flattening process provides optimization for data transfer,
        /// prevents sending sensitive info to the client, and facilitates
        /// data access to its consumer.
        /// </remarks>
        private MovieBulletinDto? _dbMovieDto;

        /// <summary>
        /// Encapsulates custom methods to handle exceptions with clear
        /// messages to inform the end user. It allows to centralize custom
        /// messages; e.g., messages conveyed to the user when a JSException
        /// is thrown because the user attempts a get, create, update, or
        /// delete operation when the application is offline.
        /// </summary>
        [Inject] private IExceptionHandlers ExHandlers { get; set; } = null!;

        /// <summary>
        /// This MovieBulletin component has a section of UI (Genres with
        /// hypertext) that renders dynamic content (Razor code: HTML & CSharp)
        /// based on specific criteria (current genre's name). The generic
        /// RenderFragment<Genre> permits to define the markup in a single place
        /// and consume it to render the appropriate data based on the argument
        /// (Genre object) passed to satisfy its formal input parameter.
        /// </summary>
        /// <remarks>
        /// It has a private access modifier because it is implemented and
        /// consumed locally.
        /// </remarks>
        //private readonly RenderFragment<Genre> _genreLinkTemplate = (genre) =>
        //    @<a href = "movies/search" > @genre.Name </ a >;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                /// Gets the ClaimsPrincipal for the current user.
                ClaimsPrincipal user = (await AuthNStateTask).User;

                /// If current user is authenticated, target the
                /// GetMovieBulletinDto endpoint that includes the user's
                /// rating for the current Movie.
                if (user.Identity is { IsAuthenticated: true })
                {
                    _dbMovieDto = await ApiService.Movies
                        .GetMovieBulletinWithUserScoreDtoAsync(MovieId);
                }
                else
                {
                    /// Targets the GetMovieBulletinDto endpoint that only
                    /// includes the average rating for the Movie object. It
                    /// does not include the user's rating because it is not
                    /// authenticated; i.e., there is no primaary key.
                    _dbMovieDto = await ApiService.Movies
                        .GetMovieBulletinDtoAsync(MovieId);
                }

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
        /// Handler for the <see cref="Ranking.OnScoreSelected"/> event 
        /// callback. It captures the score value when a new one is selected
        /// and uses it to build a type <see cref="MovieScore"/> to either 
        /// create or update the database record.
        /// </summary>
        /// <param name="selectedScore">The score selected by the user; i.e.,
        /// the index value of the star selected by the user.</param>
        /// <returns>An asynchronous operation.</returns>
        private async Task OnScoreSelectedAsync(int selectedScore)
        {
            /// Root entity represents a record in the database table that
            /// stores the movie score selected by the logged in application
            /// user for the current Movie object.
            MovieScore movieScore = null!;
            try
            {
                /// Gets the ClaimsPrincipal for the current user.
                ClaimsPrincipal? user = (await AuthNStateTask).User;

                /// If current user is authenticated, target the
                /// GetMovieBulletinDto endpoint that includes the user's
                /// rating for the current Movie.
                if (user.Identity is { IsAuthenticated: false })
                {
                    await RedirectToLoginView();
                }
                else
                {
                    /// Needs to be updated because it is consumed in the markup
                    /// section by the Ranking compoonent responsible for rendering
                    /// the stars that represent the score selected by the user.
                    _dbMovieDto!.UserScore = selectedScore;

                    /// Root entity represents a record in the database table that
                    /// stores the movie score selected by the logged in application
                    /// user for the current Movie object.
                    movieScore = new()
                    {
                        Score = selectedScore,
                        MovieId = this.MovieId,
                        ScoreDate = DateTime.Today,
                    };

                    /// Retrieves the current user from the data store and either
                    /// creates or updates a MovieScore database record using the
                    /// one passed to satisfy its formal input parameter.
                    MovieScore insertedMovieScore = await ApiService.MovieScores
                        .HandleScoreAsync(movieScore);

                    /// Custom IJSRuntime extension method overload invokes a
                    /// SweetAlert JS function to provide feedback to the user. It
                    /// displays a dialog box with a title, a message, and an icon.
                    await JsRuntime
                        .SwAlDisplayMessageAsync(
                            "Success!",
                            "Movie score updated.",
                            SwAlIconType.success);
                }
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
                        objectToCreate: movieScore,
                        controllerName: "moviescores",
                        routeTemplateComplement: null);

                    /// Reloads the routable component with empty fields to allow
                    /// the user to try once more. It does NOT bypass client side
                    /// routing.
                    NavManager?.NavigateTo(NavManager.Uri, false);
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

        /// <summary>
        /// Redirects the user to the Login page and captures the current URL
        /// to return the user if authentication is successful.
        /// </summary>
        /// <remarks>
        /// It uses a custom extension method that invokes a SweetAlert JS
        /// function to inform the user that he must be logged in before
        /// selecting a MovieScore.
        /// </remarks>
        private async Task RedirectToLoginView()
        {
            /// Custom extension method that encapsulates the code logic
            /// necessary to invoke a SweetAlert JS function to display a
            /// dialog box with title, message, and icon.
            await JsRuntime.SwAlDisplayMessageAsync(
                title: "Notice!",
                message: "You must be logged in to select a movie score.",
                swAlIconType: SwAlIconType.info);

            /// <summary>
            /// Support for authentication in Blazor WebAssembly apps changed to
            /// rely on navigation history state instead of query strings in the
            /// URL. This is the new redirection approach for apps that target
            /// .Net 7 or later.
            /// </summary>
            /// <remarks>
            /// See <see href="https://learn.microsoft.com/en-us/aspnet/core/migration/60-70?view=aspnetcore-7.0">
            /// Blazor WebAssembly authentication uses history state for redirects
            /// </see>,
            /// <see href="https://github.com/aspnet/Announcements/issues/497">
            /// [Breaking change]: Updated to Authentication in web assembly
            /// applications
            /// </see>,
            /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-7.0#navigation-history-state">
            /// navigation history state
            /// </see>, and
            /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.components.webassembly.authentication.navigationmanagerextensions.navigatetologin">
            /// NavigateToLogin
            /// </see>.
            /// </remarks>
            NavManager.NavigateToLogin(
                Options.Get(
                    Microsoft.Extensions.Options.Options.DefaultName)
                    .AuthenticationPaths.LogInPath);

            /// <summary>
            /// Valid with ASP.Net 6.
            /// </summary>
            //NavManager?
            //    .NavigateTo
            //        ($"authentication/login?returnUrl=" +
            //         $"{Uri.EscapeDataString(NavManager.Uri)}");
        }
    }
}





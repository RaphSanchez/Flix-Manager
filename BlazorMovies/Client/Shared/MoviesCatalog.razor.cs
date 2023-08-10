using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Presents the user with objects of type <see cref="Movie"/> passed as a
    /// collection to satisfy its formal input parameter named
    /// <see cref="Movies"/> and provides the user with a Window.Alert
    /// confirmation dialog box before removing an item from the collection.
    /// </summary>
    public partial class MoviesCatalog
    {
        /// <summary>
        /// The collection of items of type <see cref="Movie"/> to display to
        /// the user.
        /// </summary>
        [Parameter] public List<Movie>? Movies { get; set; }

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

        /// <summary>
        /// Removes the items selected by the user from the database. 
        /// </summary>
        /// <remarks>
        /// It provides the user with a <em>Window.Alert</em> confirmation
        /// dialog box before removing an item from the collection. 
        /// </remarks>
        /// <param name="movie">The <see cref="Movie"/> object to remove from
        /// the data source.</param>
        /// <returns>An asynchronous operation.</returns>
        private async Task DeleteMovieAsync(Movie movie)
        {
            try
            {
                /// Invokes a custom .Net extension method that encapsulates
                /// the JS code logic required to display a built-in JS dialog
                /// box with no styling.
                //bool confirmed =
                //    await _jsRuntime.ConfirmDeleteDialogBox<bool>(
                //        $"Please confirm deletion of {movie.Title}");

                bool confirmed =
                    await _jsRuntime.SwAlConfirmDialogAsync(
                        title: "Confirmation",
                        message: $"Do you want to delete {movie.TitleSummary}?",
                        swAlIconType: SwAlIconType.question);

                if (confirmed)
                {
                    /// Deletes the Movie object from the database. It returns
                    /// the entity that was successfully deleted.
                    Movie? deletedMovie = await apiService.Movies
                        .DeleteMovieAsync(movie.Id)!;

                    /// Removes the deleted Movie object from the collection
                    /// of in-memory Movie items that this MoviesCatalog is
                    /// responsible for rendering.
                    ///
                    /// This notifies the component that its state has changed
                    /// and will cause the component to be re-rendered with the
                    /// collection of Movie items updated.
                    Movies?.Remove(movie);

                    /// Custom extension method invokes a SweetAlert JS
                    /// function to display a basic message to the user. It
                    /// provides feedback to the user. 
                    await _jsRuntime.SwAlDisplayMessageAsync(
                        $"{movie.TitleSummary} deleted successfully.");
                }
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
                        controllerName: "movies",
                        routeTemplateComplement: $"/{movie.Id}");

                    /// Removes the deleted Movie object from the collection
                    /// of in-memory Movie items that this MoviesCatalog is
                    /// responsible for rendering.
                    ///
                    /// This notifies the component that its state has changed
                    /// and will cause the component to be re-rendered with the
                    /// collection of Movie items updated.
                    Movies?.Remove(movie);
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
                    NavManager?.NavigateTo("movies/flix-manager", true);
                }
            }
        }
    }
}


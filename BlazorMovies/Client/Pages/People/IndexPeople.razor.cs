using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Client.Shared;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.People
{
    /// <summary>
    /// Displays items of type <see cref="Person"/> to the user in manageable
    /// segments or portions of data with the help of a custom
    /// <see cref="Pagination"/> component.
    /// </summary>
    public partial class IndexPeople
    {
        /// <summary>
        /// Collection of items of type <see cref="Person"/> that belong to
        /// the segment of data retrieved from the data source based on the
        /// pagination page number selected by the user.
        /// </summary>
        private List<Person>? _dbPeoplePaginated;

        /// <summary>
        /// Custom <see cref="Confirmation"/> modal component responsible for
        /// requiring a confirmation from the user before requesting a delete
        /// operation on the database.
        /// </summary>
        private Confirmation? _confirmation;

        /// <summary>
        /// Stores a reference to the <see cref="Person"/> object captured
        /// when the user raises the @onclick event of a Delete button element.
        /// </summary>
        private Person? _currentPerson;

        /// <summary>
        /// Defines the pagination parameters for an Http request for
        /// paginated data. It includes the page number and how many records
        /// per page.
        /// </summary>
        /// <remarks>
        /// A single instance of a <see cref="_paginationRequestDto"/> as a
        /// class level field allows centralizing the pagination parameters
        /// for the Http request. Its PageNumber property value is overwritten
        /// by the <see cref="LoadSelectedPageAsync"/> handler every time the
        /// user selects a new page number. 
        /// </remarks>
        private readonly PaginationRequestDto _paginationRequestDto = new()
        { RecordsPerPage = 7 };

        /// <summary>
        /// Encapsulates the object value served with an Http response that
        /// implements pagination. It includes the description and context of
        /// the paginated data (includes metadata).
        /// </summary>
        private PaginatedResponseDto<IEnumerable<Person>>? _paginatedResponseDto;

        /// <summary>
        ///  Exposes one IEntityName interface for each data entity mapped to
        /// the database. 
        /// </summary>
        [Inject]
        private IApiService ApiService { get; set; } = null!;

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
                /// Makes a request to the data source for Person items that
                /// belong to the segment (page number) specified in the
                /// _paginationRequestDto field.
                await RequestPeoplePaginatedAsync();
            }
            catch (Exception ex)
            {
                /// Informs the user when an unexpected error occurred in a
                /// clear and meaningful message.
                await JsRuntime.SwAlDisplayMessageAsync(
                    "Info",
                    ex.Message,
                    SwAlIconType.warning);
            }
        }

        /// <summary>
        /// Event handler for the @onclick event of the "Delete" button
        /// element.
        /// </summary>
        /// <param name="person">The Person item captured when the @onclick
        /// event of the Delete button element is raised.</param>
        private void DeletePerson(Person? person)
        {
            /// Stores a reference to the current Person object.
            _currentPerson = person;

            /// Executes the <see cref="Confirmation"/> component.
            _confirmation?.ShowConfirmComponent();
        }

        /// <summary>
        /// Event handler for the <see cref="OnCancelCallback"/> of the
        /// <see cref="Confirmation"/> component.
        /// </summary>
        private void CancelDelete()
        {
            _confirmation?.HideConfirmComponent();

            /// Current Person object captured when the user raises the
            /// @onclick event of the Delete button element must be set
            /// to null if the user canceled the delete operation.
            _currentPerson = null;
        }

        /// <summary>
        /// Event handler for the <see cref="OnConfirmCallback"/> of the
        /// <see cref="Confirmation"/> component.
        /// </summary>
        /// <returns>An asynchronous operation.</returns>
        private async Task PerformDelete()
        {
            try
            {
                /// Removes the Person object from the database. It returns
                /// the entity that was successfully removed.
                Person? deletedPerson = await ApiService.People
                .DeletePersonAsync(_currentPerson!.Id)!;

                _confirmation?.HideConfirmComponent();

                /// Current Person object captured when the user raises the
                /// @onclick event of the Delete button element must be set
                /// to null after the object is successfully removed from the
                /// database.
                _currentPerson = deletedPerson != null ? null : _currentPerson;

                /// Makes a request to the data source for Person items that
                /// belong to the segment (page number) specified in the
                /// _paginationRequestDto field; i.e., it updates the UI after
                /// the delete operation takes places.
                await RequestPeoplePaginatedAsync();
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
                        routeTemplateComplement: $"/{_currentPerson?.Id}");

                    /// Removes the genre from the local collection of Genre
                    /// items displayed to the user.
                    _dbPeoplePaginated?.Remove(_currentPerson!);

                    /// Current Genre object captured when the user raises the
                    /// @onclick event of the Delete button element. If the
                    /// IndexedDB record that represents a delete operation is
                    /// persisted successfully, the reference to the Genre object
                    /// must be set to null because it is no longer needed.
                    _currentPerson = null;
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

        /// <summary>
        /// Event handler (method) passed to satisfy the
        /// <see cref="OnSelectedPageValidated"/> event callback parameter of
        /// the <see cref="Pagination"/> component. 
        /// </summary>
        /// <remarks>
        /// It overwrites the <see cref="PageNumber"/> property value of
        /// the <see cref="_paginationRequestDto"/> field, used to
        /// construct the Http request, with the page number selected by the
        /// user.
        /// </remarks>
        /// <param name="selectedPageNumber">The number of the page selected
        /// by the user. Its value is captured and passed from the event
        /// handler delegate (Pagination component) to the event handler
        /// method (consumer).
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        private async Task LoadSelectedPageAsync(int selectedPageNumber)
        {
            try
            {
                _paginationRequestDto.PageNumber = selectedPageNumber;

                /// Makes a request to the data source for Person items that
                /// belong to the segment (page number) specified in the
                /// _paginationRequestDto field; i.e., it updates the UI after
                /// the user selects a new page button element of the
                /// pagination component.
                await RequestPeoplePaginatedAsync();
            }
            catch (Exception ex)
            {
                /// Informs the user when an unexpected error occurred in a
                /// clear and meaningful message.
                await JsRuntime.SwAlDisplayMessageAsync(
                    "Info",
                    ex.Message,
                    SwAlIconType.warning);
            }
        }

        /// <summary>
        /// Makes a request to the data source for Person items that belong to
        /// the segment (page number) specified in the
        /// <see cref="_paginationRequestDto"/> field and populates the
        /// <see cref="_paginatedResponseDto"/> field with the object value
        /// served and its metadata (description and context).
        /// </summary>
        /// <remarks>
        /// It <strong>centralizes</strong> the database query operation to
        /// retrieve data into a single place and orders the paginated query
        /// results by last name. Its functionality is consumed repeatedly. 
        /// </remarks>
        /// <returns>An asynchronous operation.</returns>
        private async Task RequestPeoplePaginatedAsync()
        {
            try
            {
                _paginatedResponseDto =
                        await ApiService.People
                            .GetPaginatedAsync(_paginationRequestDto);

                /// Populates (and updates) the _dbPeoplePaginated field with
                /// Person items that belong to the page number selected by
                /// the user.
                _dbPeoplePaginated = _paginatedResponseDto.ResponseData
                    .OrderBy(p =>
                    {
                        int indexOfEmptyCharacter = p.Name.IndexOf(' ');

                        /// Creates an open ended range that starts with
                        /// the indexed value of the empty character in
                        /// a given Person.Name property value.
                        ///
                        /// This line of code is the same as the one
                        /// shown below.
                        /// https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/ranges-indexes
                        return p.Name[(indexOfEmptyCharacter + 1)..];

                        //return p.Name.Substring(indexOfEmptyCharacter + 1);

                    }).ToList();
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
    }
}


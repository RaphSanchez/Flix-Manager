using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Client.Shared;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Users
{
    /// <summary>
    /// Retrieves the application users currently registered in the data store
    /// and presents them to the user in manageable segments or portions of
    /// data with the help of a custom <see cref="Pagination"/> component.
    /// </summary>
    public partial class IndexUsers
    {
        /// <summary>
        /// Collection of items of type <see cref="UserDto"/> that belong to
        /// the segment of data retrieved from the data source based on the
        /// pagination page number selected by the User.
        /// </summary>
        private List<UserDto>? _dbUsersPaginated = new();

        /// <summary>
        /// Defines the pagination parameters for an Http request for
        /// paginated data. It includes the page number and the number of
        /// records per page.
        /// </summary>
        /// <remarks>
        /// A single instance of <see cref="PaginationRequestDto"/> as a class
        /// level field allows centralizing the pagination parameters for the
        /// Http request. Its PageNumber property value is overwritten by the
        /// <see cref="LoadSelectedPageAsync"/> handler every time the user
        /// selects a new page number with the <see cref="Pagination"/>
        /// control.
        /// </remarks>
        private readonly PaginationRequestDto _paginationRequestDto = new()
        { RecordsPerPage = 3 };

        /// <summary>
        /// Encapsulates the object value served with an Http response that
        /// implements pagination. It includes the description and context of
        /// the paginated data (includes the metadata).
        /// </summary>
        private PaginatedResponseDto<IEnumerable<UserDto>>? _paginatedResponseDto;

        /// <summary>
        ///  Exposes one IEntityName interface for each data entity mapped to
        /// the database. 
        /// </summary>
        [Inject] private IApiService ApiService { get; set; } = null!;

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
            await LoadUsersAsync();
        }

        /// <summary>
        /// Makes a request to the data source for user Dto items that belong
        /// to the segment (page number) specified in the
        /// <see cref="_paginationRequestDto"/> field and populates the
        /// <see cref="_paginatedResponseDto"/> field with the object value
        /// served and its metadata (description and context).
        /// </summary>
        /// <remarks>
        /// It <strong>centralizes</strong> the database query operation to
        /// retrieve data into a single place and orders the paginated query
        /// results by email.
        /// </remarks>
        /// <returns>An asynchronous operation.</returns>
        private async Task LoadUsersAsync()
        {
            try
            {
                _paginatedResponseDto =
                    await ApiService?.Users
                        .GetPaginatedUsersAsync(_paginationRequestDto)!;

                _dbUsersPaginated = _paginatedResponseDto?.ResponseData
                        .OrderBy(u => u.Email)
                        .ToList();
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
            _paginationRequestDto.PageNumber = selectedPageNumber;

            /// Makes a request to the data source for user dto items that
            /// belong to the segment (page number) specified in the
            /// _paginationRequestDto field; i.e., it updates the UI after
            /// the user selects a new page button element of the
            /// pagination component.
            await LoadUsersAsync();
        }
    }
}

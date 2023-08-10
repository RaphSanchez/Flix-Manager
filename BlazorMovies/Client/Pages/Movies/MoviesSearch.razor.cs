using System.Reflection;

using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Client.Shared;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Movies
{
    /// <summary>
    /// Builds a request to the server for objects of type <see cref="Movie"/>
    /// that match the filtering criteria selected by the user and the
    /// pagination parameters specified in the request.
    /// </summary>
    /// <remarks>
    /// It implements <em>deep linking</em>; i.e., it uses the URL's query
    /// string to build the HTTP request.
    /// <para>
    /// The query result is presented to the user using a
    /// <see cref="MoviesCatalog"/> component.
    /// </para>
    /// </remarks>
    public partial class MoviesSearch
    {
        /// <summary>
        /// Encapsulates property values that can be directly related to one
        /// or more properties of a type <see cref="Movie"/> and the
        /// parameters to paginate the response. 
        /// </summary>
        /// <remarks>
        /// A single instance of a <see cref="_moviesQueryFilterDto"/> as a
        /// class level field allows centralizing the filtering criteria and
        /// the pagination parameters for the Http request. 
        /// </remarks>
        private readonly MoviesQueryFilterDto _moviesQueryFilterDto = new()
        {
            PaginationPageNumber = 1,
            PaginationRecordsPerPage = 3
        };

        /// <summary>
        /// Encapsulates the object value served with an Http response that
        /// implements pagination. It includes its metadata (description and
        /// context).  
        /// </summary>
        private PaginatedResponseDto<IEnumerable<Movie>>? _paginatedResponseDto;

        /// <summary>
        /// Stores the movie items to be rendered.
        /// </summary>
        /// <remarks>
        /// Does not need to be initialized because items of type
        /// <see cref="Movie"/> are rendered with the <see cref="MoviesCatalog"/>
        /// component which is capable of handling empty and/or null collections.
        /// </remarks>
        private List<Movie> _movies = null!;

        /// <summary>
        /// Stores the genre items that are used as options for the Genres
        /// field in the markup section. 
        /// </summary>
        /// <remarks>
        /// It needs to be initialized. Otherwise, the
        /// <see cref="MoviesSearch"/> component throws an
        /// ArgumentNullException during initialization because the Genres
        /// field loads before the Genre items are retrieved from the database.
        /// </remarks>
        private List<Genre?> _genres = new();

        /// <summary>
        /// Custom service is the entry point to the Application/Server-Api to
        /// have access to available operations for data entity types. 
        /// </summary>
        [Inject] public IApiService ApiService { get; set; } = null!;

        /// <summary>
        /// Built-in type provides an abstraction for querying and managing
        /// URI navigation (front-end). 
        /// </summary>
        [Inject] public NavigationManager NavManager { get; set; } = null!;

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

        /// <summary>
        /// Queries the database for data necessary to populate the
        /// <see cref="MoviesSearch"/> form fields.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            try
            {
                _genres = (await ApiService.Genres.GetAllAsync())
                    .ToList();

                #region Deep Linking

                /// Gets the URL from the web browser.
                string browserUrlString = NavManager.Uri;

                /// Collection of <Key, Value> pairs that can be mapped to
                /// members of a MoviesQueryStringDto used as filtering
                /// criteria and pagination parameters.
                ///
                /// Returns null if the browser's URL is null or does not
                /// contain a query string.
                Dictionary<string, string>? urlQueryDictionary =
                    UrlUtilities.DecodeUrlQueryToDictionary(browserUrlString);

                /// If urlQueryDictionary is null, it means the browser's URL
                /// does not have a query string.
                if (urlQueryDictionary != null)
                {
                    /// Maps the <Key, Value> pairs in a Dictionary to members
                    /// of a type MoviesQueryFilterDto used as filtering
                    /// criteria and pagination parameters.
                    MapUrlQueryDictToQueryDto(urlQueryDictionary);
                }

                #endregion

                /// Makes a data request for Movie records that match the
                /// filtering criteria specified in the form fields, instantiates-
                /// updates a local collection of Movie items with the query
                /// result, and overwrites the web browser's URL with data that
                /// describes the operation details to the user. It forces the
                /// component to re-render because the collection of Movie items
                /// is overwritten. 
                await RequestMoviesPaginatedAsync();
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
        /// Handler for the @onclick event of the Clear Fields button element.
        /// It resets field values. 
        /// </summary>
        private async Task ClearFieldsAsync()
        {
            /// Re-sets the members of the MoviesQueryFilterDto which are
            /// bound to the MoviesSearch form fields. 
            foreach (PropertyInfo pInfo in _moviesQueryFilterDto
                         .GetType().GetProperties())
            {
                /// Excludes pagination records per page parameter.
                if (pInfo.CanWrite

                    && pInfo.Name !=
                    nameof(_moviesQueryFilterDto.PaginationRecordsPerPage))
                {
                    /// Re-sets the current property value to null because
                    /// all MoviesQueryFilterDto members are nullable.
                    ///
                    /// PaginationPageNumber parameter value is set to 0.
                    pInfo.SetValue(_moviesQueryFilterDto, default);
                }
            }

            /// Must be assigned a value. Otherwise, it is set to its default
            /// (zeroe) and does not display the current page number on the
            /// address bar after clearing the form fields. 
            _moviesQueryFilterDto.PaginationPageNumber = 1;

            /// Value for _moviesQueryFilterDto.Genre must match the value
            /// assigned to the "selected" attribute of the first (or default)
            /// option of the "select" HTML element for the Selected Genre
            /// field in the markup section.
            ///
            /// The first (or default) option of the "select" element renders
            /// the "--Select Genre--" text.
            ///
            /// Overwriting the value re-sets the Genre field to
            /// "--Select Genre--".
            _moviesQueryFilterDto.Genre = string.Empty;

            /// Reloads the Movie items after the form fields have been
            /// re-set.
            ///
            /// Makes a data request for Movie records that match the
            /// filtering criteria specified in the form fields, instantiates-
            /// updates a local collection of Movie items with the query
            /// result, and overwrites the web browser's URL with data that
            /// describes the operation details to the user. It forces the
            /// component to re-render because the collection of Movie items
            /// is overwritten. 
            await RequestMoviesPaginatedAsync();
        }

        /// <summary>
        /// Handler for the @onclick event of the Search Movies button
        /// element. It builds a request to the server for objects of type
        /// <see cref="Movie"/> that match the filtering criteria selected by
        /// the user and the pagination parameters specified in the request.
        /// </summary>
        /// <remarks>
        /// The query result is presented to the user using a
        /// <see cref="MoviesCatalog"/> component.
        /// </remarks>
        private async Task SearchMoviesAsync()
        {

            /// If a new request is generated, the query results should be
            /// presented to the user starting with the first segment of
            /// data. Otherwise, the segment of the new query result
            /// rendered is the one stored at the moment of submitting the
            /// form fields. 
            _moviesQueryFilterDto.PaginationPageNumber = 1;

            /// Reloads the Movie items after the form fields have been re-set.
            ///
            /// Makes a data request for Movie records that match the
            /// filtering criteria specified in the form fields, instantiates-
            /// updates a local collection of Movie items with the query
            /// result, and overwrites the web browser's URL with data that
            /// describes the operation details to the user. It forces the
            /// component to re-render because the collection of Movie items
            /// is overwritten.
            await RequestMoviesPaginatedAsync();
        }

        /// <summary>
        /// Handles the @onkeydown event of the Movie Title field which is an
        /// HTML element of type: <code>&lt;input&gt;</code>
        /// </summary>
        /// <param name="e">Provides access to the KeyboardEventArgs class
        /// which supplies information about a keyboard event that is being
        /// raised.</param>
        private async Task OnEnterKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                await SearchMoviesAsync();
            }
        }

        /// <summary>
        /// Makes a data request for Movie records that match the filtering
        /// criteria selected by the user in the form fields, instantiates a
        /// local collection of Movie items with the query result, and
        /// overwrites the web browser's URL with data that describes the
        /// operation details to the user.
        /// </summary>
        /// <returns>An asynchronous operation.</returns>
        private async Task RequestMoviesPaginatedAsync()
        {
            try
            {
                /// Makes a data request for Movie items that match the filtering
                /// criteria and paginates the result adhering to the pagination
                /// parameters passed to satisfy its formal input parameter. 
                _paginatedResponseDto = await ApiService.Movies
                    .FilterPaginateMoviesAsync(_moviesQueryFilterDto);

                /// Instantiates the collection of Movie items to be rendered or
                /// presented to the user. If forces a new rendering of the
                /// component each time it is assigned new values; i.e., each time
                /// a new request is sent to the Application/Server-Api. 
                _movies = _paginatedResponseDto.ResponseData
                    .ToList();

                /// Constructs an encoded query string with the filtering
                /// criteria and pagination parameters specified in the
                /// MovieQueryFilterDto. 
                string queryString = GetQueryString();

                /// Builds the full URL concatenating the scheme and authority
                /// (or domain) provided by the built-in NavigationManager,
                /// the relative path (movies/search), and the encoded query
                /// string.
                if (!string.IsNullOrEmpty(queryString))
                {
                    /// NavigateTo method controls the browser's URL. 
                    NavManager.NavigateTo("movies/search" + queryString);
                }
            }
            catch(Exception ex)
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

                /// Redirects the user to the Flix Manager routable component.
                /// It does not bypass client side routing and does not force
                /// the browser to load the new page from the server.
                ///
                /// If user offline, the PWA mechanism does not load the page
                /// and gets stuck in an endless loop trying to reload unless
                /// it is redirected to another page.
                NavManager?.NavigateTo("movies/flix-manager", false);
            }
        }

        /// <summary>
        /// Builds a dictionary collection of &lt;Key, Value&gt; pairs using
        /// members of the <see cref="MoviesQueryFilterDto"/> which are used
        /// as filtering criteria and pagination parameters to make a request
        /// to the database. The items in the dictionary are used to produce
        /// an encoded query string. 
        /// </summary>
        /// <returns>An encoded query string with parameters that can be
        /// mapped to <see cref="MoviesQueryFilterDto"/> members used as
        /// filtering criteria and pagiantion parameters to make a request
        /// to the database.</returns>
        private string GetQueryString()
        {
            /// Collection of <Key,Value> pairs with one item for each member
            /// of the MoviesQueryFilterDto object passed as an argument to
            /// the FilterPaginateMoviesAsync method to specify the filtering
            /// criteria and the pagination parameters. 
            ///
            /// Using a "nameof()" expression (as opposed to hard coding the
            /// name of the filtering parameter) for the Key facilitates
            /// maintanability; i.e., renaming one or more members of the
            /// MoviesQueryFilterDto type, if done properly, will not affect
            /// the end result of our query string because the Key will be
            /// updated automatically. 
            Dictionary<string, string?> queryStringsDict = new()
            {
                [nameof(_moviesQueryFilterDto.Id)] =
                    _moviesQueryFilterDto.Id.ToString(),

                [nameof(_moviesQueryFilterDto.Title)] =
                    _moviesQueryFilterDto.Title ?? string.Empty,

                [nameof(_moviesQueryFilterDto.Genre)] =
                    _moviesQueryFilterDto.Genre ?? string.Empty,

                [nameof(_moviesQueryFilterDto.UpComingReleases)] =
                    _moviesQueryFilterDto.UpComingReleases.ToString(),

                [nameof(_moviesQueryFilterDto.InTheaters)] =
                    _moviesQueryFilterDto.InTheaters.ToString(),

                [nameof(_moviesQueryFilterDto.PaginationPageNumber)] =
                    _moviesQueryFilterDto.PaginationPageNumber.ToString()
            };

            /// Constructs an encoded URL query string using a Dictionary with
            /// items that can be mapped to members of the DTO used as
            /// filtering criteria and pagination parameters.
            ///
            /// Only parameters applied as filtering criteria should contain
            /// actual values, the rest of the items in the dictionary
            /// collection should be initialized to their default values. Valid
            /// default values are "0" (zeroe), "string.Empty", and "false".
            string encodedQueryString = UrlUtilities
                .BuildEncodedQueryString(queryStringsDict);

            return encodedQueryString;
        }

        /// <summary>
        /// Maps the &lt;Key, Value&gt; pairs in a Dictionary, built from the
        /// query string of the browser's URL, to the members of a type
        /// <see cref="MoviesQueryFilterDto"/> used as filtering criteria and
        /// pagination parameters.  
        /// </summary>
        /// <param name="urlQueryDictionary">A collection of &lt;Key, Value&gt;
        /// pairs that can be mapped to members of the
        /// <see cref="MoviesQueryFilterDto"/> used as filtering criteria and
        /// pagination parameters. 
        /// </param>
        private void MapUrlQueryDictToQueryDto(
            Dictionary<string, string> urlQueryDictionary)
        {
            /// <remarks>
            /// The <see cref="UrlUtilities.DecodeUrlQueryToDictionary"/>
            /// method used to build the dictionary collection passed as
            /// argument to this method returns the &lt;Key, Value&gt; pairs
            /// in lower case. Ensure that the name of the
            /// <see cref="MoviesQueryFilterDto"/> member produced with the
            /// nameof() expression is in lower case or there will be no
            /// matches.
            /// </remarks>

            // Maps MoviesQueryFilterDto.Id
            if (urlQueryDictionary
                .ContainsKey(nameof(_moviesQueryFilterDto.Id)
                    .ToLower()))
            {
                _moviesQueryFilterDto.Id = int.TryParse(
                    urlQueryDictionary[nameof(_moviesQueryFilterDto.Id)
                        .ToLower()],
                    out int number)
                    ? number
                    : null;
            }

            // Maps MoviesQueryFilterDto.Title
            if (urlQueryDictionary
                .ContainsKey(nameof(_moviesQueryFilterDto.Title)
                    .ToLower()))
            {
                _moviesQueryFilterDto.Title =
                    urlQueryDictionary[nameof(_moviesQueryFilterDto.Title)
                            .ToLower()]
                        .ToLower();
            }

            // Maps MoviesQueryFilterDto.Genre
            if (urlQueryDictionary
                .ContainsKey(nameof(_moviesQueryFilterDto.Genre)
                    .ToLower()))
            {
                _moviesQueryFilterDto.Genre =
                    urlQueryDictionary[nameof(_moviesQueryFilterDto.Genre)
                            .ToLower()]
                        .ToLower();
            }

            // Maps MoviesQueryFilterDto.UpComingReleases
            if (urlQueryDictionary
                .ContainsKey(nameof(_moviesQueryFilterDto.UpComingReleases)
                    .ToLower()))
            {
                // ReSharper disable once SimplifyConditionalTernaryExpression
                _moviesQueryFilterDto.UpComingReleases = bool.TryParse(
                    urlQueryDictionary[
                        nameof(_moviesQueryFilterDto.UpComingReleases)
                            .ToLower()],
                    out bool boolValue)
                ? boolValue
                : null;
            }

            // Maps MoviesQueryFilterDto.InTheaters
            if (urlQueryDictionary
                .ContainsKey(nameof(_moviesQueryFilterDto.InTheaters)
                    .ToLower()))
            {
                // ReSharper disable once SimplifyConditionalTernaryExpression
                _moviesQueryFilterDto.InTheaters = bool.TryParse(
                    urlQueryDictionary[nameof(_moviesQueryFilterDto.InTheaters)
                        .ToLower()],
                    out bool boolValue)
                    ? boolValue
                    : null;
            }

            // Maps MoviesQueryFilterDto.PaginationPageNumber
            if (urlQueryDictionary
                .ContainsKey(nameof(_moviesQueryFilterDto.PaginationPageNumber)
                    .ToLower()))
            {
                _moviesQueryFilterDto.PaginationPageNumber = int.TryParse(
                    urlQueryDictionary[
                        nameof(_moviesQueryFilterDto.PaginationPageNumber)
                            .ToLower()],
                    out int number)
                    ? number
                    : 0;
            }
        }

        /// <summary>
        /// Event handler (method) passed to satisfy the
        /// <see cref="OnSelectedPageValidated"/> event handler (delegate)
        /// parameter of the <see cref="Pagination"/> component.
        /// </summary>
        /// <remarks>It overwrites the <see cref="PaginationPageNumber"/>
        /// property value of the <see cref="_moviesQueryFilterDto"/> field,
        /// used to construct the Http request, with the page number selected
        /// by the user.</remarks>
        /// <param name="pageNumberSelected">The number of the page selected
        /// by the user. Its value is captured and passed from the event
        /// handler delegate (Pagination component) to the event handler
        /// method (consumer).</param>
        /// <returns>An asynchronous operation.</returns>
        private async Task LoadSelectedPageAsync(int pageNumberSelected)
        {
            _moviesQueryFilterDto.PaginationPageNumber = pageNumberSelected;

            /// Makes a request to the data source for Movie items that belong
            /// to the segment (page number) specified in the
            /// _moviesQueryFilterDto field; i.e., it updates the UI after the
            /// user selects a new page button element of the pagination
            /// component.
            await RequestMoviesPaginatedAsync();
        }
    }
}


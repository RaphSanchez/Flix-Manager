using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.People
{
    /// <summary>
    /// Displays the actor details such as name, date of birth,
    /// biography, movies, etc.
    /// </summary>
    public partial class PersonDetails
    {
        /// <summary>
        /// Encapsulates property values that can be directly related to one
        /// or more properties of a type <see cref="Person"/>. The property
        /// values are used to query the database for a record that matches
        /// these values. 
        /// </summary>
        private PeopleQueryFilterDto? _queryFilterDto;

        /// <summary>
        /// The Person object returned with the query result. 
        /// </summary>
        private Person? _dbActor;

        /// <summary>
        /// Collection of objects of type <seealso cref="Movie"/> that are
        /// related to the <see cref="Person"/> object returned with the query
        /// result.
        /// </summary>
        private List<Movie>? _actorMovies;

        /// <summary>
        /// Route parameter.
        /// </summary>
        [Parameter] public int PersonId { get; set; }

        /// <summary>
        /// Route parameter.
        /// </summary>
        [Parameter] public string? PersonName { get; set; }

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
        /// Encapsulates custom methods to handle exceptions with clear
        /// messages to inform the end user. It allows to centralize custom
        /// messages; e.g., messages conveyed to the user when a JSException
        /// is thrown because the user attempts a get, create, update, or
        /// delete operation when the application is offline.
        /// </summary>
        [Inject] private IExceptionHandlers ExHandlers { get; set; } = null!;

        protected override async Task OnParametersSetAsync()
        {
            try
            {
                /// Instantiates the DTO with the query parameters to build
                /// the Http request.
                _queryFilterDto = new(id: PersonId, name: PersonName);

                /// Queries the database for records whose property values
                /// match the DTO parameters.
                _dbActor = (await ApiService?.People
                        .FilterAsync(_queryFilterDto)!)
                    .FirstOrDefault();

                /// Movie objects related to the Person record.
                _actorMovies = _dbActor?.Movies?
                    .OrderByDescending(m => m.ReleaseDate)
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
    }
}


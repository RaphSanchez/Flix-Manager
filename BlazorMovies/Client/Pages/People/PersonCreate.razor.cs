using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.People
{
    /// <summary>
    /// Allows an application user to create a Person object.
    /// </summary>
    public partial class PersonCreate
    {
        /// <summary>
        /// Instance of the Person entity that resides in
        /// BlazorMovies/Shared/EDM.
        /// </summary>
        private readonly Person? _person = new Person();

        /// <summary>
        /// The Person object successfully inserted into the
        /// database and returned within the Http response.
        /// </summary>
        private Person? _dbResponsePerson;

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
        /// Encapsulates custom methods to handle exceptions with clear and
        /// meaningful messages to inform the end user. It allows to centralize
        /// custom messages; e.g., messages conveyed to the user when a
        /// JSException is thrown because the user attempts a create, update,
        /// or delete operation when the application is offline.
        /// </summary>
        [Inject] private IExceptionHandlers ExHandlers { get; set; } = null!;

        /// <summary>
        /// Event handler for the OnValidSubmit event callback of the
        /// PersonForm component.
        /// </summary>
        /// <returns></returns>
        private async Task CreatePerson()
        {
            try
            {
                /// Inserts the Person entity, provided by the PersonForm fields,
                /// to the database and stores in a local variable the Person
                /// object returned within the Http response. The Http response
                /// includes the inserted object with a database primary key.
                _dbResponsePerson = await ApiService.People.AddAsync(_person);

                /// Displays into the console of the browser's web developer
                /// tools the Person.Name property value obtained from the
                /// PersonForm fields. In-memory object.
                Console.WriteLine($@"Form Person Name: {_person?.Name} ");

                /// Displays into the console of the browser's web developer
                /// tools the Person.Name property value obtained from the
                /// Person object successfully inserted into the database and
                /// returned in the Http response. Database object.
                Console.WriteLine($@"Database Person Name: {_dbResponsePerson?.Name}");

                /// Re-directs the user to the IndexPeople routable component
                /// which will eventually display all the currently available
                /// Person items.
                NavManager?.NavigateTo("people");
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
                        objectToCreate: _person,
                        controllerName: "genres",
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

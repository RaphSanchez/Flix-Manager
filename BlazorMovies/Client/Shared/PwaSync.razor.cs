using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;
using BlazorMovies.Client.Events;
using BlazorMovies.Client.Helpers;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Synchronizes with the network server POST, PUT, and DELETE operations
    /// executed by the application while offline. This component is required
    /// as part of the implementation of the offline feature for Progressive
    /// Web Apps (PWAs).
    /// </summary>
    /// <remarks>
    /// When a user attempts to create, edit, or delete an entity while the
    /// application is offline, the operation parameters required to build the
    /// Http request are stored in an object store of our custom IndexedDB and
    /// the <see cref="PwaSync"/> component is responsible for retrieving those
    /// records from (client-side storage) and synchronize them with the web
    /// server once a connection is reestablished.
    /// <para>
    /// It is also responsible for removing the object store record from the
    /// IndexedDB (client-side storage) if the Http request is successful; i.e,.
    /// if the operation was successfully synchronized with the web server.
    /// </para>
    /// </remarks>
    public partial class PwaSync : IDisposable
    {
        /// <summary>
        /// Total number of CREATE, DELETE, and UPDATE objects that are stored
        /// in our custom IndexedDBs waiting to be synchronized with the web 
        /// server.
        /// </summary>
        private int _numberOfPendingSynchronizations = 0;

        /// <summary>
        /// Flag used to inform the user during a synchronization operation.
        /// </summary>
        private bool _isSynchronizing = false;

        /// <summary>
        /// Represents an instance of a JavaScript runtime to which calls may
        /// be dispatched. 
        /// </summary>
        [Inject]
        public IJSRuntime JsRuntime { get; set; } = null!;

        /// <summary>
        /// Represents an instance of the implementation of the
        /// <see cref="IApiConnector"/> interface which defines the protocol 
        /// for functionality required by the IApiService  (available to the 
        /// client) to connect to the Application/Server-Api/Controllers 
        /// (unavailable to the client) to send/receive Http requests/responses.
        /// </summary>
        /// <remarks>
        /// It defines the methods that encapsulate the required code logic for
        /// the "resource methods" responsible for building the Http 
        /// requests/responses and serializing/deserializing .Net objects to 
        /// JSON format so they can travel through the internet to the 
        /// Application/Server-Api/Controllers and back.
        /// </remarks>
        [Inject]
        public IApiConnector ApiConnector { get; set; } = null!;

        /// <summary>
        /// Application state container defines an event responsible for
        /// initiating an asynchronous operation to update the number of
        /// pending create, updated, or delete operations stored in our
        /// custom IndexedDB that need to be synchronized with the server.
        /// </summary>
        [Inject]
        public ISynchronizationState SyncState { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            /// This <see cref="PwaSync"/> component subscribes to the event.
            /// The event is raised (published) from any Razor component after
            /// persisting a record into an object store of our custom
            /// IndexedDB as a result of the user attempting to perform the
            /// operation while the applicatoin was offline. Each record holds
            /// the required data (parameters) to build an Http request.
            ///
            /// It is also raised from the <see cref="MainLayout"/> component
            /// to trigger an update during app initialization.
            ///
            /// When the event is raised, the
            /// PwaSync.OnUpdateNumberOfPendingSynchronizationsAsync event
            /// handler method is invoked.
            /// 
            /// Episode 152. Comunicación entre componentes - Borrado en modo
            /// offline of Udemy course Programando en Blazor - ASP.Net Core 7
            /// by Felipe Gavilán.
            /// https://www.udemy.com/share/101ZK23@P_z_otCvbswCJciag5NxSmY35j18kowbLOCX8HqFrUZqiNaxg_M3YeBqkKGKTY0v/
            SyncState.UpdateNumberOfPendingOperationsAsync +=
                OnUpdateNumberOfPendingSynchronizationsAsync;

            /// Attempts to synchronize with the web server pending POST, PUT, and
            /// DELETE operations that the user attempted to execute while the
            /// application was offline.
            await SynchronizeAsync();
        }

        /// <summary>
        /// Click handler for the SynchronizeAsync button element. 
        /// </summary>
        /// <returns>An asynchronous operation.</returns>
        private async Task StartSynchronizationAsync()
        {
            /// Attempts to synchronize with the web server pending POST, PUT, and
            /// DELETE operations that the user attempted to execute while the
            /// application was offline.
            await SynchronizeAsync();
        }

        /// <summary>
        /// Attempts to synchronize with the web server pending POST, PUT, and
        /// DELETE operations that the user attempted to execute while the
        /// application was offline.
        /// </summary>
        /// <returns>An asynchronous operation.</returns>
        private async Task SynchronizeAsync()
        {
            /// Retrieves all the available records from our custom IndexedDB
            /// object stores (e.g., "createOperations", "updateOperations, and
            /// "deleteOperations"). These records represent operations that
            /// the user attempted to execute while the application was offline.
            /// They need to be synchronized with the web server.
            LocalDbRecordsDto localDbRecordsDto =
                await JsRuntime!.GetRecordsOfPendingOperations();

            /// Total number of objects to crate, update, and delete that are
            /// stored in our custom IndexedDB object stores.
            int numberOfPendingOperations =
                await JsRuntime!.GetNumberOfPendingSynchronizations();

            /// If no pending operations, return.
            if (numberOfPendingOperations == 0) { return; }

            /// If we've come this far, it means we have pending operations to
            /// synchronize with the web server. We need to start the process
            /// and inform the user that a synchronization attempt is in
            /// progress.
            _isSynchronizing = true;
            /// Forces the component to re-render and reflect the sync flag.
            StateHasChanged();

            /// If synchronization process is initiated while the app is offline,
            /// it will throw an HttpRequestException that includes an inner
            /// exception of type JSException.
            ///
            /// This higher level exception handler evaluates the exception and
            /// informs the user accordingly.
            try
            {
                #region Create

                /// Iterate over the ObjectsToCreate collection.
                foreach (LocalDbRecordDto record in 
                         localDbRecordsDto.ObjectsToCreate)
                {
                    /// Handles exceptions other than JSException which is
                    /// thrown when fails to fetch because no connection
                    /// with the web server is available. This approach allows
                    /// the foreach loop to continue. For example: "Movie title
                    /// already exists".
                    ///
                    /// If JSException, it throws it back to the stack to break
                    /// the loop because there is no connection to the web
                    /// server and allow the higher level exception handler to
                    /// deal with the JSException.
                    try
                    {
                        /// Encapsulates an Api resource method and the details
                        /// of building an Http POST request for the specified
                        /// controller endpoint. It contains the value serialized
                        /// as JSON (JavaScript Object Notation) in the body of the
                        /// Http request.
                        /// 
                        /// We are passing the Application/Client/Helpers
                        /// HttpClientWithJwt because POST (or create)
                        /// Application/Server-Api resources are secured.
                        await ApiConnector.InvokePostAsync<JsonElement>(
                            record.Body,
                            record.ControllerName,
                            record.RouteTemplateComplement,
                            jwtOptions: JwtOptions.IncludeJWTs);

                        /// If Http request is processed successfully, remove
                        /// the record from the IndexedDB object store. Refer
                        /// to the Application/Client/wwwroot/js Dexie.js file
                        /// for more info on the object stores.
                        await JsRuntime!
                            .DeleteLocalDbRecord(
                                table: "createOperations",
                                id: record.Id);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null
                            && ex.InnerException.GetType().ToString()
                                .Contains("JSException"))
                        {
                            /// If it is a JSException, it means it is not able
                            /// to fetch because there is no connection to the
                            /// server. It rethrows the exception back to the
                            /// stack to break the loop. This prevents removing
                            /// the pending operation from the IndexedDB.
                            throw;
                        }

                        /// Otherwise, remove the record from the IndexedDB
                        /// object store. If an exception other than a
                        /// JSException occurs, remove the record from the
                        /// IndexedDB because the Http request parameters could
                        /// be corrupted.
                        ///
                        /// Refer to the Application/Client/wwwroot/js Dexie.js
                        /// file for more info on the object stores.
                        await JsRuntime!
                            .DeleteLocalDbRecord(
                                table: "createOperations",
                                id: record.Id);

                        /// Displays a message to inform the user of the
                        /// unexpected error and allows the loop to continue.
                        await JsRuntime.SwAlDisplayMessageAsync(
                            "Warning",
                            ex.Message,
                            SwAlIconType.warning);
                    }
                }

                #endregion

                #region Update

                /// Iterate over the ObjectsToUpdate collection.
                foreach (LocalDbRecordDto record in
                         localDbRecordsDto.ObjectsToUpdate)
                {
                    /// Handles exceptions other than JSException which is
                    /// thrown when fails to fetch because no connection
                    /// with the web server is available. This approach allows
                    /// the foreach loop to continue. For example: "Not Found
                    /// Result".
                    ///
                    /// If JSException, it throws it back to the stack to break
                    /// the loop because there is no connection to the web
                    /// server and allow the higher level exception handler to
                    /// deal with the JSException. 
                    try
                    {
                        /// Encapsulates an Api resource method and the details
                        /// of building an Http PUT request for the specified
                        /// controller endpoint. It contains the value
                        /// serialized as JSON (JavaScript Object Notation) in
                        /// the body of the Http request.
                        /// 
                        /// We are passing the Application/Client/Helpers
                        /// HttpClientWithJwt because PUT (or update)
                        /// Application/Server-Api resources are secured.
                        await ApiConnector.InvokePutAsync<JsonElement>(
                            record.Body,
                            record.ControllerName,
                            record.RouteTemplateComplement,
                            jwtOptions: JwtOptions.IncludeJWTs);

                        /// If Http request is processed successfully, remove
                        /// the record from the IndexedDB object store. Refer
                        /// to the Application/Client/wwwroot/js Dexie.js file
                        /// for more info on the object stores.
                        await JsRuntime!
                            .DeleteLocalDbRecord(
                                table: "updateOperations",
                                id: record.Id);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null
                            && ex.InnerException.GetType().ToString()
                                .Contains("JSException"))
                        {
                            /// If it is a JSException, it means it is not able
                            /// to fetch because there is no connection to the
                            /// server. It rethrows the exception back to the
                            /// stack to break the loop. This prevents removing
                            /// the pending operation from the IndexedDB.
                            throw;
                        }

                        /// Otherwise, remove the record from the IndexedDB
                        /// object store. If an exception other than a
                        /// JSException occurs, remove the record from the
                        /// IndexedDB because the Http request parameters could
                        /// be corrupted.
                        ///
                        /// Refer to the Application/Client/wwwroot/js Dexie.js
                        /// file for more info on the object stores.
                        await JsRuntime!
                            .DeleteLocalDbRecord(
                                table: "updateOperations",
                                id: record.Id);


                        /// Displays a message to inform the user of the
                        /// unexpected error and allows the loop to continue.
                        await JsRuntime.SwAlDisplayMessageAsync(
                            "Warning",
                            ex.Message,
                            SwAlIconType.warning);
                    }
                }

                #endregion

                #region Delete

                /// Iterate over the ObjectsToDelete collection.
                ///
                /// DELETE OPERATIONS SHOULD BE LAST to avoid any unnecessary
                /// exceptions if there are pending create and/or update
                /// operations on an object that is ultimately deleted.
                foreach (LocalDbRecordDto record in 
                         localDbRecordsDto.ObjectsToDelete)
                {
                    /// Handles exceptions other than JSException which is
                    /// thrown when fails to fetch because no connection
                    /// with the web server is available. This approach allows
                    /// the foreach loop to continue. For example: "Not Found
                    /// Result".
                    ///
                    /// If JSException, it throws it back to the stack to break
                    /// the loop because there is no connection to the web
                    /// server and allow the higher level exception handler to
                    /// deal with the JSException.
                    try
                    {
                        /// Encapsulates an Api resource method and the details
                        /// of building an Http DELETE request for the specified
                        /// controller endpoint.
                        /// 
                        /// We are passing the Application/Client/Helpers
                        /// HttpClientWithJwt because DELETE
                        /// Application/Server-Api resources are secured.
                        await ApiConnector.InvokeDeleteAsync<JsonElement>(
                            record.ControllerName,
                            record.RouteTemplateComplement,
                            jwtOptions: JwtOptions.IncludeJWTs);

                        /// If Http request is processed successfully, remove
                        /// the record from the IndexedDB object store. Refer
                        /// to the Application/Client/wwwroot/js Dexie.js file
                        /// for more info on the object stores.
                        await JsRuntime!
                            .DeleteLocalDbRecord(
                                table: "deleteOperations",
                                id: record.Id);
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException != null
                            && ex.InnerException.GetType().ToString()
                                .Contains("JSException"))
                        {
                            /// If it is a JSException, it means it is not able
                            /// to fetch because there is no connection to the
                            /// server. It rethrows the exception back to the
                            /// stack to break the loop. This prevents removing
                            /// the pending operation from the IndexedDB.
                            throw;
                        }

                        /// Otherwise, remove the record from the IndexedDB
                        /// object store. If an exception other than a
                        /// JSException occurs, remove the record from the
                        /// IndexedDB because the Http request parameters could
                        /// be corrupted.
                        ///
                        /// Refer to the Application/Client/wwwroot/js Dexie.js
                        /// file for more info on the object stores.
                        await JsRuntime!
                            .DeleteLocalDbRecord(
                                table: "deleteOperations",
                                id: record.Id);

                        /// Displays a message to inform the user of the
                        /// unexpected error and allows the loop to continue.
                        await JsRuntime.SwAlDisplayMessageAsync(
                            "Warning",
                            ex.Message,
                            SwAlIconType.warning);
                    }
                }

                #endregion

                /// Informs the user that the synchronization process has
                /// completed.
                await JsRuntime.SwAlDisplayMessageAsync(
                    "Info",
                    "Completed synchronizing pending operations with the web " +
                    "server.",
                    SwAlIconType.success);
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and send it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Message to convey info to the user is dependent on the type
                /// of exception. 
                string customMessage =
                    ex.InnerException != null
                    && ex.InnerException
                        .GetType().ToString().Contains("JSException")
                    ? "Unable to establish a connection with the web server to " +
                      "synchronize pending operations. Will retry at a later time."
                    : ex.Message;

                /// Informs the user of the unexpected error that was
                /// during the synchronization process.
                await JsRuntime.SwAlDisplayMessageAsync(
                    "Warning",
                    customMessage,
                    SwAlIconType.warning);
            }
            finally
            {
                /// Synchronization process has completed.
                _isSynchronizing = false;

                /// Updates de total number of database records that represent
                /// operations that need to be synchronized with the web server
                /// and forces the <see cref="PwaSync"/> component to re-render
                /// and update any recently modified parameters.
                await OnUpdateNumberOfPendingSynchronizationsAsync();
            }
        }

        /// <summary>
        /// Calculates the total number of database records that represent
        /// operations that need to be synchronized with the web server.
        /// </summary>
        /// <remarks>
        /// It is passed as the event handler method of the
        /// <see
        /// cref="ISynchronizationState.UpdateNumberOfPendingOperationsAsync"/>
        /// event.
        /// <para>
        /// Subscription to the event takes place in the
        /// <see cref="OnInitializedAsync"/> lifecycle method.
        /// </para>
        /// </remarks>
        /// <returns>An asynchronous operation.</returns>
        private async Task OnUpdateNumberOfPendingSynchronizationsAsync()
        {
            _numberOfPendingSynchronizations =
                await JsRuntime!.GetNumberOfPendingSynchronizations();

            /// Forces the component to re-render and reflect the updated
            /// value.
            StateHasChanged();
        }

        /// <summary>
        /// The <see cref="PwaSync"/> component is unsubscribed from the
        /// <see
        /// cref="ISynchronizationState.UpdateNumberOfPendingOperationsAsync"/>
        /// event by the <see cref="Dispose"/> method which is called by the
        /// framework when the component is disposed. 
        /// </summary>
        /// <remarks>
        /// See <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-7.0">
        /// In-memory state container service
        /// </see>,
        /// Episode 152. Comunicación Entre Componentes - Borrado en Modo
        /// Offline of Udemy course <see cref="">
        /// Programando en Blazor - ASP.Net 7 
        /// </see> by Felipe Gavilán,
        /// <see href="https://chrissainty.com/3-ways-to-communicate-between-components-in-blazor/">
        /// 3 Ways to Communicate Between Components in Blazor</see>, and
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-7.0#component-disposal-with-idisposable-and-iasyncdisposable">
        /// Component disposal with IDisposable and IAsyncDisposable</see>.
        /// </remarks>
        public void Dispose()
        {
            SyncState.UpdateNumberOfPendingOperationsAsync -=
                OnUpdateNumberOfPendingSynchronizationsAsync;
        }
    }
}



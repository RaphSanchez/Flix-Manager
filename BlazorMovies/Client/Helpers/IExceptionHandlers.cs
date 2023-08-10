
using BlazorMovies.Client.Events;
using BlazorMovies.Client.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// Abstraction layer that establishes a contract to encapsulate custom
    /// methods to handle exceptions with clear and meaningful messages to
    /// inform the end user.
    /// <para>
    /// It allows to centralize custom messages; e.g., messages conveyed to
    /// the user when a JSException is thrown because the user attempts
    /// a create, update, or delete operation when the application is offline.
    /// </para>
    /// </summary>
    public interface IExceptionHandlers
    {
        /// <summary>
        /// Represents a message for the application user when an unexpected
        /// error occurs.
        /// </summary>
        /// <remarks>
        /// Centralizing the message content facilitates future modifications.
        /// It also allows an easier implementation of localization
        /// (translation) services.
        /// </remarks>
        string MessageUnexpectedError { get; }

        /// <summary>
        /// Represents a message for the application user when the application
        /// is offline and a create, update, or delete operation is stored in
        /// our custom IndexedDB.
        /// </summary>
        /// <remarks>
        /// Centralizing the message content facilitates future modifications.
        /// It also allows an easier implementation of localization
        /// (translation) services.
        /// </remarks>
        string MessageOperationSuccessfullyStored { get; }

        /// <summary>
        /// Represents a message to inform the application user that the
        /// process for synchronizing pending operations with the web server
        /// is completed.
        /// </summary>
        /// <remarks>
        /// Centralizing the message content facilitates future modifications.
        /// It also allows an easier implementation of localization
        /// (translation) services.
        /// </remarks>
        string MessageSuccessfulSynchronization { get; }

        /// <summary>
        /// Represents a message to inform the application user that the
        /// pending operations were not synchronized with the web server.
        /// </summary>
        /// <remarks>
        /// Centralizing the message content facilitates future modifications.
        /// It also allows an easier implementation of localization
        /// (translation) services.
        /// </remarks>
        string MessageUnableToSynchronizeWithWebServer { get; }

        /// <summary>
        /// Evaluates the exception to determine the content of the message to
        /// convey to the user.
        /// </summary>
        /// <remarks>
        /// With Progressive Web Apps (PWAs) that support working offline, if
        /// the result of an Http GET request to an API resource has not been
        /// stored in our custom cache (dynamic-cache), and the request is
        /// produced when the application is offline, an
        /// <see cref="HttpRequestException"/> is produced because it is unable
        /// to communicate with the web server and/or finding a match in our
        /// custom cache to serve the request.
        /// <para>
        /// The <see cref="HttpRequestException"/> includes an inner exception
        /// of type
        /// <see cref="System.Runtime.InteropServices.JavaScript.JSException"/>.
        /// This is the condition that determines the content of the message
        /// that will ultimately be passed to the end user.
        /// </para>
        /// <para>
        /// <strong>It should only be employed to handle exceptions for HTTP
        /// GET requests; e.g., inside an OnInitialized lifecycle method of
        /// a routable component.</strong> Catching an
        /// <see cref="HttpRequestException"/> for HTTP POST, PUT, or DELETE
        /// requests is handled differently; i.e., the request parameters are
        /// stored as a record in our custom IndexedDB to synchronize at a
        /// later time when a connection to the web server is established.
        /// </para>
        /// <para>
        /// We cannot use a type <see cref="HttpRequestException"/> to determine
        /// the content of the message because coincidentally, we throw the same
        /// type of exception from the HandleHttpRequestErrorAsync() method of
        /// the Application/Client/ApiServices/ApiManager ApiConnector class.
        /// </para>
        /// </remarks>
        /// <param name="ex">The <see cref="Exception"/> to evaluate.</param>
        /// <returns>A message to inform the end user of the error produced.
        /// </returns>
        string CreateMessageForFailedGetRequest(Exception ex);

        /// <summary>
        /// 1. Persists into our custom IndexedDB a record with the data
        /// required to build the HTTP request to perform a create operation
        /// during a synchronization process by the <see cref="PwaSync"/>
        /// component.
        /// <para>
        /// 2. Calls the event publisher method of the
        /// UpdateNumberOfPendingOperationsAsync event in the
        /// Application/Client/Events/ISynchronizationState to update the
        /// total number of create, update, and delete operations stored in
        /// our custom IndexedDB that need to be synchronized with the web
        /// server; i.e., it sends an event notification which triggers an
        /// update of the value for the number of pending operations displayed
        /// to the user by the <see cref="PwaSync"/> component.
        /// </para>
        /// <para>
        /// 3. Informs the user that the operation was successfully stored
        /// for synchronization once a connection to the network server is
        /// established.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the entity to create or persist
        /// into the database.</typeparam>
        /// <param name="objectToCreate">The data entity object to create. It
        /// can be a Data Transfer Object (DTO).
        /// </param>
        /// <param name="controllerName">The name of the controller to which
        /// the Http request should be referred to.
        /// </param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint); e.g.,
        /// $"filter?textToSearch={movieTitle}". It can be obtained from the
        /// ApiEntityName and/or the controller.
        /// <para>
        /// Note that <strong>a "create" operation does not need any additional
        /// route segments</strong>.
        /// </para>
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        Task HandleInnerJSExceptionCreateAsync<T>(
            T objectToCreate,
            string controllerName,
            string? routeTemplateComplement = null);

        /// <summary>
        /// 1. Persists into our custom IndexedDB a record with the data
        /// required to build the HTTP request to perform an update operation
        /// during a synchronization process by the <see cref="PwaSync"/>
        /// component.
        /// <para>
        /// 2. Calls the event publisher method of the
        /// UpdateNumberOfPendingOperationsAsync event in the
        /// Application/Client/Events/ISynchronizationState to update the
        /// total number of create, update, and delete operations stored in
        /// our custom IndexedDB that need to be synchronized with the web
        /// server; i.e., it sends an event notification which triggers an
        /// update of the value for the number of pending operations displayed
        /// to the user by the <see cref="PwaSync"/> component.
        /// </para>
        /// <para>
        /// 3. Informs the user that the operations was successfully stored
        /// for synchronization once a connection to the network server is
        /// established.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the entity to update or put
        /// into the database.</typeparam>
        /// <param name="objectToUpdate">The data entity object to update. It
        /// can be a Data Transfer Object (DTO).</param>
        /// <param name="controllerName">The name of the controller to which
        /// the Http request should be referred to.
        /// </param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint); e.g.,
        /// $"/{genre?.Id}". It can be obtained from the ApiEntityName and/or
        /// the controller.
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        Task HandleInnerJSExceptionUpdateAsync<T>(
            T objectToUpdate,
            string controllerName,
            string routeTemplateComplement);

        /// <summary>
        /// 1. Persists into our custom IndexedDB a record with the data
        /// required to build the HTTP request to perform a delete operation
        /// during a synchronization process by the <see cref="PwaSync"/>
        /// component.
        /// <para>
        /// 2. Calls the event publisher method of the
        /// UpdateNumberOfPendingOperationsAsync event in the
        /// Application/Client/Events/ISynchronizationState to update the
        /// total number of create, update, and delete operations stored in
        /// our custom IndexedDB that need to be synchronized with the web
        /// server; i.e., it sends an event notification which triggers an
        /// update of the value for the number of pending operations displayed
        /// to the user by the <see cref="PwaSync"/> component.
        /// </para>
        /// <para>
        /// 3. Informs the user that the operations was successfully stored
        /// for synchronization once a connection to the network server is
        /// established.
        /// </para>
        /// </summary>
        /// <param name="controllerName">The name of the controller to which
        /// the Http request should be referred to.
        /// </param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint); e.g.,
        /// $"/{genre.Id}". It can be obtained from the ApiEntityName and/or
        /// the controller.
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        Task HandleInnerJSExceptionDeleteAsync(
            string controllerName,
            string routeTemplateComplement);
    }

    /// <summary>
    /// Implements <see cref="IExceptionHandlers"/> to encapsulate custom
    /// methods to handle exceptions with clear and meaningful messages to
    /// inform the end user.
    /// <para>
    /// It allows to centralize custom messages; e.g., messages conveyed to
    /// the user when a JSException is thrown because the user attempts
    /// a create, update, or delete operation when the application is offline.
    /// </para>
    /// </summary>
    public class ExceptionHandlers : IExceptionHandlers
    {
        /// <summary>
        /// Represents an instance of a JavaScript runtime to which calls may
        /// be dispatched.
        /// </summary>
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// Application state container defines an event responsible for
        /// initiating an asynchronous operation to update the number of
        /// pending create, updated, or delete operations stored in our
        /// custom IndexedDB that need to be synchronized with the server.
        /// </summary>
        private readonly ISynchronizationState _syncState;

        public ExceptionHandlers(IJSRuntime jsRuntime,
            ISynchronizationState syncState)
        {
            _jsRuntime = jsRuntime;
            _syncState = syncState;
        }

        /// <summary>
        /// Represents a message for the application user when an unexpected
        /// error occurs.
        /// </summary>
        /// <remarks>
        /// Centralizing the message content facilitates future modifications.
        /// It also allows an easier implementation of localization
        /// (translation) services.
        /// </remarks>
        public string MessageUnexpectedError =>
            "An unexpected error occurred. Please try again.";

        /// <summary>
        /// Represents a message for the application user when the application
        /// is offline and a create, update, or delete operation is stored in
        /// our custom IndexedDB.
        /// </summary>
        /// <remarks>
        /// Centralizing the message content facilitates future modifications.
        /// It also allows an easier implementation of localization
        /// (translation) services.
        /// </remarks>
        public string MessageOperationSuccessfullyStored =>
            "No connection with the web server. Operation stored " +
            "to synchronize when a connection is reestablished.";

        /// <summary>
        /// Represents a message to inform the application user that the
        /// process for synchronizing pending operations with the web server
        /// is completed.
        /// </summary>
        /// <remarks>
        /// Centralizing the message content facilitates future modifications.
        /// It also allows an easier implementation of localization
        /// (translation) services.
        /// </remarks>
        public string MessageSuccessfulSynchronization =>
            "Completed synchronizing pending operations with the web server.";

        /// <summary>
        /// Represents a message to inform the application user that the
        /// pending operations were not synchronized with the web server.
        /// </summary>
        /// <remarks>
        /// Centralizing the message content facilitates future modifications.
        /// It also allows an easier implementation of localization
        /// (translation) services.
        /// </remarks>
        public string MessageUnableToSynchronizeWithWebServer =>
            "Unable to establish a connection with the web server to synchronize " +
            "pending operations. Will retry at a later time.";

        /// <summary>
        /// Evaluates the exception to determine the content of the message to
        /// convey to the user.
        /// </summary>
        /// <remarks>
        /// With Progressive Web Apps (PWAs) that support working offline, if
        /// the result of an Http GET request to an API resource has not been
        /// stored in our custom cache (dynamic-cache), and the request is
        /// produced when the application is offline, an
        /// <see cref="HttpRequestException"/> is produced because it is unable
        /// to communicate with the web server and/or finding a match in our
        /// custom cache to serve the request.
        /// <para>
        /// The <see cref="HttpRequestException"/> includes an inner exception
        /// of type
        /// <see cref="System.Runtime.InteropServices.JavaScript.JSException"/>.
        /// This is the condition that determines the content of the message
        /// that will ultimately be passed to the end user.
        /// </para>
        /// <para>
        /// <strong>It should only be employed to handle exceptions for HTTP
        /// GET requests; e.g., inside an OnInitialized lifecycle method of
        /// a routable component.</strong> Catching an
        /// <see cref="HttpRequestException"/> for HTTP POST, PUT, or DELETE
        /// requests is handled differently; i.e., the request parameters are
        /// stored as a record in our custom IndexedDB to synchronize at a
        /// later time when a connection to the web server is established.
        /// </para>
        /// <para>
        /// We cannot use a type <see cref="HttpRequestException"/> to determine
        /// the content of the message because coincidentally, we throw the same
        /// type of exception from the HandleHttpRequestErrorAsync() method of
        /// the Application/Client/ApiServices/ApiManager ApiConnector class.
        /// </para>
        /// </remarks>
        /// <param name="ex">The <see cref="Exception"/> to evaluate.</param>
        /// <returns>A message to inform the end user of the error produced.
        /// </returns>
        public string CreateMessageForFailedGetRequest(Exception ex)
        {
            return
                ex.InnerException != null
                && ex.InnerException.GetType().ToString().Contains("JSException")
                ? "You must be online the first time you consume this " +
                  "resource to enable offline access."
                : ex.Message;
        }

        /// <summary>
        /// 1. Persists into our custom IndexedDB a record with the data
        /// required to build the HTTP request to perform a create operation
        /// during a synchronization process by the <see cref="PwaSync"/>
        /// component.
        /// <para>
        /// 2. Calls the event publisher method of the
        /// UpdateNumberOfPendingOperationsAsync event in the
        /// Application/Client/Events/ISynchronizationState to update the
        /// total number of create, update, and delete operations stored in
        /// our custom IndexedDB that need to be synchronized with the web
        /// server; i.e., it sends an event notification which triggers an
        /// update of the value for the number of pending operations displayed
        /// to the user by the <see cref="PwaSync"/> component.
        /// </para>
        /// <para>
        /// 3. Informs the user that the operation was successfully stored
        /// for synchronization once a connection to the network server is
        /// established.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the entity to create or persist
        /// into the database.</typeparam>
        /// <param name="objectToCreate">The data entity object to create. It
        /// can be a Data Transfer Object (DTO).
        /// </param>
        /// <param name="controllerName">The name of the controller to which
        /// the Http request should be referred to.
        /// </param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint); e.g.,
        /// $"filter?textToSearch={movieTitle}". It can be obtained from the
        /// ApiEntityName and/or the controller.
        /// <para>
        /// Note that <strong>a "create" operation does not need any additional
        /// route segments</strong>.
        /// </para>
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        public async Task HandleInnerJSExceptionCreateAsync<T>(
            T objectToCreate,
            string controllerName,
            string? routeTemplateComplement = null)
        {
            /// Persists an IndexedDB record withe the data required to
            /// build the Http request to perform a create operation
            /// during a synchronization process by the PwaSync
            /// component.
            ///
            /// The route template complement can be obtained from the
            /// ApiRespository, ApiEntityName, or the controller.
            await _jsRuntime
                .PersistCreateOperationParameters(
                    entity: objectToCreate,
                    controllerName: controllerName,
                    routeTemplateComplement: routeTemplateComplement);

            /// Calls the custom event publisher method of the
            /// UpdateNumberOfPendingOperationsAsync event in the
            /// Application/Client/Events/ISynchronizationState to
            /// update the total number of create, update, and delete
            /// operations stored in our custom IndexedDB that need to
            /// be synchronized with the web server.
            ///
            /// In other words, it sends an event notification which
            /// triggers and update of the value for the number of
            /// pending operations displayed to the user by the
            /// <see cref="PwaSync"/> component. 
            await _syncState
                .PublishUpdateNumberOfPendingOperationsAsync();

            /// Informs the user that the operation was successfully stored
            /// for synchronization once a connection to the network server
            /// is established.
            await _jsRuntime
                .SwAlDisplayMessageAsync(
                    MessageOperationSuccessfullyStored);
        }

        /// <summary>
        /// 1. Persists into our custom IndexedDB a record with the data
        /// required to build the HTTP request to perform an update operation
        /// during a synchronization process by the <see cref="PwaSync"/>
        /// component.
        /// <para>
        /// 2. Calls the event publisher method of the
        /// UpdateNumberOfPendingOperationsAsync event in the
        /// Application/Client/Events/ISynchronizationState to update the
        /// total number of create, update, and delete operations stored in
        /// our custom IndexedDB that need to be synchronized with the web
        /// server; i.e., it sends an event notification which triggers an
        /// update of the value for the number of pending operations displayed
        /// to the user by the <see cref="PwaSync"/> component.
        /// </para>
        /// <para>
        /// 3. Informs the user that the operations was successfully stored
        /// for synchronization once a connection to the network server is
        /// established.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the entity to update or put
        /// into the database.</typeparam>
        /// <param name="objectToUpdate">The data entity object to update. It
        /// can be a Data Transfer Object (DTO).</param>
        /// <param name="controllerName">The name of the controller to which
        /// the Http request should be referred to.
        /// </param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint); e.g.,
        /// $"/{genre?.Id}". It can be obtained from the ApiEntityName and/or
        /// the controller.
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        public async Task HandleInnerJSExceptionUpdateAsync<T>(
            T objectToUpdate,
            string controllerName,
            string routeTemplateComplement)
        {
            /// Persists an IndexedDB record withe the data required to
            /// build the Http request to perform an update operation
            /// during a synchronization process by the PwaSync
            /// component.
            ///
            /// The route template complement can be obtained from the
            /// ApiRespository, ApiEntityName, or the controller.
            await _jsRuntime
                .PersistUpdateOperationParameters(
                    entity: objectToUpdate,
                    controllerName: controllerName,
                    routeTemplateComplement: routeTemplateComplement);

            /// Calls the custom event publisher method of the
            /// UpdateNumberOfPendingOperationsAsync event in the
            /// <see cref="ISynchronizationState"/> to update the
            /// total number of create, update, and delete operations
            /// stored in our custom IndexedDB that need to be
            /// synchronized with the web server.
            ///
            /// In other words, it sends an event notification which
            /// triggers and update of the value for the number of
            /// pending operations displayed to the user by the
            /// <see cref="PwaSync"/> component. 
            await _syncState
                .PublishUpdateNumberOfPendingOperationsAsync();

            /// Informs the user that the operation was successfully stored
            /// for synchronization once a connection to the network server
            /// is established.
            await _jsRuntime
                .SwAlDisplayMessageAsync(
                    MessageOperationSuccessfullyStored);
        }

        /// <summary>
        /// 1. Persists into our custom IndexedDB a record with the data
        /// required to build the HTTP request to perform a delete operation
        /// during a synchronization process by the <see cref="PwaSync"/>
        /// component.
        /// <para>
        /// 2. Calls the event publisher method of the
        /// UpdateNumberOfPendingOperationsAsync event in the
        /// Application/Client/Events/ISynchronizationState to update the
        /// total number of create, update, and delete operations stored in
        /// our custom IndexedDB that need to be synchronized with the web
        /// server; i.e., it sends an event notification which triggers an
        /// update of the value for the number of pending operations displayed
        /// to the user by the <see cref="PwaSync"/> component.
        /// </para>
        /// <para>
        /// 3. Informs the user that the operations was successfully stored
        /// for synchronization once a connection to the network server is
        /// established.
        /// </para>
        /// </summary>
        /// <param name="controllerName">The name of the controller to which
        /// the Http request should be referred to.
        /// </param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint); e.g.,
        /// $"/{genre.Id}". It can be obtained from the ApiEntityName and/or
        /// the controller.
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        public async Task HandleInnerJSExceptionDeleteAsync(
            string controllerName,
            string routeTemplateComplement)
        {
            /// Persists an IndexedDB record withe the data required to
            /// build the Http request to perform a create operation
            /// during a synchronization process by the PwaSync
            /// component.
            ///
            /// The route template complement can be obtained from the
            /// ApiRespository, ApiEntityName, or the controller.
            await _jsRuntime
                .PersistDeleteOperationParameters(
                    controllerName,
                    routeTemplateComplement);

            /// Calls the custom event publisher method of the
            /// UpdateNumberOfPendingOperationsAsync event in the
            /// <see cref="ISynchronizationState"/> to update the
            /// total number of create, update, and delete operations
            /// stored in our custom IndexedDB that need to be
            /// synchronized with the web server.
            ///
            /// In other words, it sends an event notification which
            /// triggers and update of the value for the number of
            /// pending operations displayed to the user by the
            /// <see cref="PwaSync"/> component. 
            await _syncState
                .PublishUpdateNumberOfPendingOperationsAsync();

            /// Informs the user that the operation was successfully stored
            /// for synchronization once a connection to the network server
            /// is established.
            await _jsRuntime
                .SwAlDisplayMessageAsync(
                    MessageOperationSuccessfullyStored);
        }
    }
}



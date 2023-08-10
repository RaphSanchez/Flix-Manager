using System.Text.Json;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Client.Shared;
using BlazorMovies.Shared.EDM;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// Custom class extends the functionality of the IJSRuntime interface
    /// by invoking specific JS functions. Having commonly required JS
    /// function invocations integrated into a single class allows changing
    /// their implementation from a single place.
    /// </summary>
    public static class IJSRuntimeExtensions
    {
        #region JS built-in functions

        /// <summary>
        /// Custom extension method invokes the window.alert(message)
        /// pre-defined JS function. The method overload below is not awaited
        /// here but it behaves the same. 
        /// </summary>
        /// <param name="jsRuntime">Represents an instance of the JS runtime
        /// to which calls may be dispatched.</param>
        /// <param name="message">The message to convey to the user.</param>
        /// <returns>No return value.</returns>
        public static async ValueTask AlertDialogBox(
            this IJSRuntime? jsRuntime,
            string message)
        {
            await jsRuntime.InvokeVoidAsync("alert", message);
        }

        /// <summary>
        /// This method overload is not awaited here and it is preferred over
        /// the one above because you should never 'await' a ValueTask more
        /// than once. 
        /// </summary>
        public static ValueTask AlertDialogBoxTwo(
            this IJSRuntime jsRuntime,
            string message)
        {
            return jsRuntime.InvokeVoidAsync("alert", message);
        }

        /// <summary>
        /// Custom extension method invokes a custom JS function from an external
        /// global .js file. The configuration script with the source path resides
        /// in the app's host page.
        /// </summary>
        /// <param name="jsRuntime">Represents an instance of the JS runtime to
        /// which calls may be dispatched.</param>
        /// <param name="message">The message to convey to the user.</param>
        /// <returns>No return value.</returns>
        public static async ValueTask ExternalJSFunction(
            this IJSRuntime jsRuntime,
            string message)
        {
            await jsRuntime.InvokeVoidAsync("my_function", message);
        }

        /// <summary>
        /// Custom extension method invokes the window.confirm(message)
        /// pre-defined JS function. 
        /// </summary>
        /// <param name="jsRuntime">Represents an instance of the JS runtime to
        /// which calls may be dispatched.</param>
        /// <param name="message">The message to convey to the user.</param>
        /// <returns>A boolean that indicates whether "OK" (true) or "Cancel"
        /// (false) was clicked in the dialog box.</returns>
        public static async ValueTask<TValue> ConfirmDeleteDialogBox<TValue>(
            this IJSRuntime jsRuntime,
            string message)
        {
            return await jsRuntime.InvokeAsync<TValue>("confirm", message);
        }

        /// <summary>
        /// Invokes a JS function responsible for monitoring the current User
        /// activity and setting the timer state accordingly. User is logged
        /// out if the timer expires before any User activity is registered.
        /// </summary>
        /// <remarks>
        /// It implements JS Isolation because it consumes the JS module that
        /// contains the JS function of interest. 
        /// </remarks>
        /// <typeparam name="T">The type of the object that defines the .Net
        /// instance method to invoke from a JS function (e.g.,
        /// MainLayout.razor).
        /// </typeparam>
        /// <param name="jsRuntime">Represents an instance of the JS runtime
        /// to which calls may be dispatched.</param>
        /// <param name="dotNetMethodContainer">The object that defines the
        /// .Net instance method to invoke from a JS function (e.g.,
        /// MainLayout.razor).</param>
        /// <returns></returns>
        public static async ValueTask InitializeInactivityTimerTask<T>(
            this IJSRuntime jsRuntime,
            T dotNetMethodContainer) where T : class
        {
            /// Stores a reference to the module's external JS file. This JS
            /// object (or file) contains the JS module with the function(s)
            /// of interest; i.e., the <see cref="IJSObjectReference"/> is
            /// the data type in Blazor that represents a JS module.
            ///
            /// By convention, the "import" identifier is a special identifier
            /// used specifically for importing a JS module. The "import" JS
            /// function imports the JS module specified in the path.
            ///
            /// It impelements <see cref="IAsyncDisposable"/>.
            await using IJSObjectReference module =
                await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./js/MainLayout.js");

            /// <see cref="DotNetObjectReference{TValue}"/> wraps a JS interop
            /// argument, indicating that the value should not be serialized
            /// as JSON but instead should be passed as a reference; i.e., it
            /// wraps the .Net object (class) that contains the instance method
            /// to invoke from a JS function.
            ///
            /// It implements IDisposable interface. Typically, to avoid
            /// leaking memory, the reference must be disposed by JS code or
            /// by .Net code. However, in this use case scenario, if disposed,
            /// the reference to the timer in the JS function is lost. 
            /*using*/
            DotNetObjectReference<T> dotNetObjectReference =
                DotNetObjectReference.Create(dotNetMethodContainer);

            /// Invokes the specified JS function from the imported JS module.
            await module.InvokeVoidAsync(
                "initializeInactivityTimer",
                dotNetObjectReference);

        }

        /// <summary>
        /// Invokes a JS function responsible for monitoring the current User
        /// activity and setting the timer state accordingly. User is logged
        /// out if timer expires before any User activity is registered. 
        /// </summary>
        /// <remarks>
        /// It loads the JS function from a global .js file which is not the
        /// recommended approach. Instead, JS functions should be placed into
        /// separate JS modules that can be imported as required. Refer to the
        /// <see cref="InitializeInactivityTimerTask{T}(IJSRuntime, T)"/>
        /// overload.
        /// <para>
        /// <see cref="DotNetObjectReference{TValue}"/> implements IDisposable
        /// interface. To avoid leaking memory, the reference must later be
        /// disposed by JS code or by .Net code.
        /// </para>
        /// </remarks>
        /// <typeparam name="T">The object that defines the .Net instance
        /// method to invoke from a JS function (e.g., MainLayout.razor).
        /// </typeparam>
        /// <param name="jsRuntime">Represents an instance of the JS runtime
        /// to which calls may be dispatched.</param>
        /// <param name="dotNetObjectReference">An instance of
        /// <see cref="DotNetObjectReference{TValue}"/> wraps a JS interop
        /// argument, indicating that the value should not be serialized as
        /// JSON but instead should be passed as a reference; i.e., it
        /// encapsulates the object that contains the .Net instance method
        /// that is invoked from a JS function (e.g., the class or Razor
        /// component that defines the .Net instance method).</param>
        /// <returns>An asynchronous operation.</returns>
        public static async ValueTask InitializeInactivityTimerTask<T>(
            this IJSRuntime jsRuntime,
            DotNetObjectReference<T> dotNetObjectReference) where T : class
        {
            /// IJSRuntime.InvokeVoidAsync() method invokes the specified JS
            /// function which in this case does not return a value.
            await jsRuntime.InvokeVoidAsync(
                "initializeInactivityTimer",
                dotNetObjectReference);
        }

        #endregion

        #region SweetAlert2 functions

        /// <summary>
        /// Custom extension method encapsulates the code logic necessary to
        /// invoke a display basic message JS function from SweetAlert. The
        /// SweetAlert library is made available through its script in the
        /// host page (Index.html) of the web root folder (wwwroot).
        /// </summary>
        /// <remarks>
        /// A similar approach to define the ValueTask is to make the method
        /// async and replace the 'return' operator with an 'await' operator.
        /// </remarks>
        /// <param name="jsRuntime">Represents an instance of the JS runtime
        /// to which calls may be dispatched.</param>
        /// <param name="message">The message to convey.</param>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous
        /// invocation operation.</returns>
        public static ValueTask SwAlDisplayMessageAsync(
            this IJSRuntime jsRuntime,
            string message)
        {
            /// The 'Swal.fire' is the JS function used to invoke functions
            /// from the SweetAlert JS library. The 'message' is the formal
            /// input parameter expected by the JS function.
            /// https://sweetalert2.github.io/#examples
            return jsRuntime.InvokeVoidAsync("Swal.fire", message);
        }

        /// <summary>
        /// Custom extension method overload that encapsulates the code logic
        /// necessary to invoke a SweetAlert JS function to display a dialog box
        /// with a title, a message, and an icon. 
        /// </summary>
        /// <remarks>
        /// A similar approach to define the ValueTask is to make the method
        /// async and replace the 'return' operator with an 'await' operator.
        /// </remarks>
        /// <param name="jsRuntime">Represents an instance of the JS runtime
        /// to which calls may be dispatched.</param>
        /// <param name="title">The title for the dialog box.</param>
        /// <param name="message">The message to convey.</param>
        /// <param name="swAlIconType`">The icon type to display in the dialog
        /// box.</param>
        /// <returns>A <see cref="ValueTask"/> that represents the asynchronous
        /// invocation operation.</returns>
        public static ValueTask SwAlDisplayMessageAsync(
            this IJSRuntime jsRuntime,
            string title, string message, SwAlIconType swAlIconType)
        {
            /// The 'Swal.fire' is the JS function used to invoke functions
            /// from the SweetAlert JS library.
            /// https://sweetalert2.github.io/#examples
            ///
            /// The <see cref="Enum.ToString"/> method creates a new string
            /// object that represents the numeric, hexadecimal, or string
            /// value of an enumeration member. 
            /// https://learn.microsoft.com/en-us/dotnet/standard/base-types/enumeration-format-strings#g-or-g
            return jsRuntime
                .InvokeVoidAsync(
                    "Swal.fire",
                    title, message,
                    swAlIconType.ToString("g").ToLower());
        }

        /// <summary>
        /// Custom extension method that encapsulates the code logic necessary
        /// to invoke a SweetAlert JS function to display a dialog box with a
        /// title, a message, an icon, a 'confirm' and a 'cancel' button
        /// elements.
        /// </summary>
        /// <param name="jsRuntime">Represents an instance of the JS runtime
        /// to which calls may be dispatched.</param>
        /// <param name="title">The title for the dialog box.</param>
        /// <param name="message">The message to convey.</param>
        /// <param name="swAlIconType`">The icon type to display in the dialog
        /// box.</param>
        /// <returns>A bool value that represents the intention of the user;
        /// e.g., true for confirm, false for cancel.</returns>
        public static async ValueTask<bool> SwAlConfirmDialogAsync(
            this IJSRuntime jsRuntime,
            string title, string message, SwAlIconType swAlIconType)
        {
            /// SweetAlert's functions expect a string for the icon type
            /// argument.
            ///
            /// The <see cref="Enum.ToString"/> method creates a new string
            /// object that represents the numeric, hexadecimal, or string
            /// value of an enumeration member.
            /// https://learn.microsoft.com/en-us/dotnet/standard/base-types/enumeration-format-strings#g-or-g
            string iconType = swAlIconType.ToString("g").ToLower();

            /// Stores a reference to the module's external JS file. This JS
            /// object (or file) contains the JS module with the function(s)
            /// of interest; i.e., the <see cref="IJSObjectReference"/> is
            /// the data type in Blazor that represents a JS module.
            ///
            /// By convention, the "import" identifier is a special identifier
            /// used specifically for importing a JS module. The "import" JS
            /// function imports the JS module specified in the path.
            ///
            /// It impelements <see cref="IAsyncDisposable"/>.
            await using IJSObjectReference module =
                await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import", "./js/SwAlModules.js");

            /// 'module' is the instance of type IJSObjectReference that stores
            /// a reference to the module's external JS file used to invoke the
            /// specified JS function. The "SwalConfirm" JS function has three
            /// formal input parameters which must be satisfied.
            bool result = await module.InvokeAsync<bool>("SwAlConfirm",
                title, message, iconType);

            return result;
        }

        #endregion

        #region DexieDB (PWA features)

        /// <summary>
        /// Delete record from the database (IndexedDB). Once a database record
        /// has been synchronized with the web server, it must be removed from
        /// the local db (IndexedDB). 
        /// </summary>
        /// <remarks>
        /// Refer to Application/Client/wwwroot/js Dexie.js file for more info
        /// on the IndexedDB and to "Episode 150. Implementando el componente
        /// sincronizador" of Udemy course <see href="https://www.udemy.com/share/101ZK23@YrCDF1LzB9xpEPReoWfAeEfW5Dgcw24qgKVUp5CCbuoWSebyL3OD9dz4D8gKjFCp/">
        /// Programando en Blazor - ASP.Net Core 7</see> by Felipe Gavilán.
        /// </remarks>
        /// <param name="jsRuntime">Represents an instance of the JS runtime to
        /// which calls may be dispatched.</param>
        /// <param name="table">The table where the record resides (IndexedDB
        /// store).</param>
        /// <param name="id">The primary key used to identify the record in
        /// the table (store).</param>
        public static async ValueTask DeleteLocalDbRecord(
            this IJSRuntime jsRuntime, string table, int id)
        {
            /// Ensure that names of the arguments are identical to the
            /// formal input parameters and in the same order of the
            /// Application/Client/wwwroot/js Dexie.js functions.
            await jsRuntime
                .InvokeVoidAsync("deleteLocalDbRecord", table, id);
        }

        /// <summary>
        /// Calculates the total number of database records that represent
        /// operations that need to be synchronized with the web server.
        /// </summary>
        /// <remarks>
        /// Refer to Application/Client/wwwroot/js Dexie.js file for more info
        /// on the IndexedDB and to "Episode 150. Implementando el componente
        /// sincronizador" of Udemy course <see href="https://www.udemy.com/share/101ZK23@YrCDF1LzB9xpEPReoWfAeEfW5Dgcw24qgKVUp5CCbuoWSebyL3OD9dz4D8gKjFCp/">
        /// Programando en Blazor - ASP.Net Core 7</see> by Felipe Gavilán.
        /// </remarks>
        /// <param name="jsRuntime">Represents an instance of the JS runtime to
        /// which calls may be dispatched.</param>
        /// <returns>The total number of database records that represent
        /// operations that need to be synchronized with the web server.
        /// </returns>
        public static async ValueTask<int> GetNumberOfPendingSynchronizations(
            this IJSRuntime jsRuntime)
        {
            return await jsRuntime
                .InvokeAsync<int>("getNumberOfPendingSynchronizations");
        }

        /// <summary>
        /// Retrieves all the available records from our custom IndexedDB stores
        /// (e.g., "createOperations", "deleteOperations"). These records
        /// represent operations that the user attempted to execute while the
        /// application was offline.
        /// </summary>
        /// <remarks>
        /// Refer to Application/Client/wwwroot/js Dexie.js file for more info
        /// on the IndexedDB and to "Episode 150. Implementando el componente
        /// sincronizador" of Udemy course <see href="https://www.udemy.com/share/101ZK23@YrCDF1LzB9xpEPReoWfAeEfW5Dgcw24qgKVUp5CCbuoWSebyL3OD9dz4D8gKjFCp/">
        /// Programando en Blazor - ASP.Net Core 7</see> by Felipe Gavilán.
        /// </remarks>
        /// <param name="jsRuntime">Represents an instance of the JS runtime to
        /// which calls may be dispatched.</param>
        /// <returns>A collection of <see cref="LocalDbRecordDto"/> object that
        /// need to be synchronized with the web server.</returns>
        public static async Task<LocalDbRecordsDto> GetRecordsOfPendingOperations(
            this IJSRuntime jsRuntime)
        {
            return await jsRuntime
                .InvokeAsync<LocalDbRecordsDto>("getRecordsOfPendingOperations");
        }

        /// <summary>
        /// Persists an IndexedDB record with the data required to build the
        /// Http request to perform a create operation. The record is produced
        /// because the operation is initiated while the application is
        /// offline.
        /// </summary>
        /// <remarks>
        /// The values of the formal input parameters are ultimately consumed
        /// by the <see cref="PwaSync"/> component. You can refer to Episode
        /// "151. Creando Géneros en modo offline" of Udemy course
        /// <see href="https://www.udemy.com/share/101ZK23@BWA7B2qaQr8iYkEhuUyGe2i-856Qbwt9t7TQjVdW20urn5unstcMIXrBftsraxFa/">
        /// Programando en Blazor - ASP.Net Core 7</see> by Felipe Gavilán.
        /// </remarks>
        /// <typeparam name="T">The type of the data entity object to create;
        /// e.g., <see cref="Movie"/> or <see cref="Genre"/>.</typeparam>
        /// <param name="jsRuntime">Represents an instance of the JS runtime to
        /// which calls may be dispatched.</param>
        /// <param name="entity">The data entity object to create.</param>
        /// <param name="controllerName">The name of the controller to which
        /// the Http request should be referred to.</param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint); e.g.,
        /// $"filter?textToSearch={movieTitle}". Note that <strong>a "create"
        /// operation does not need any additional route segments; i.e., this
        /// parameter is optional in case further on is required.</strong>.
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        public static async ValueTask PersistCreateOperationParameters<T>(
            this IJSRuntime jsRuntime,
            T entity,
            string controllerName,
            string? routeTemplateComplement = null)
        {
            /// Converts the value of a .Net type into a JSON string. The
            /// persistCreateOperationParameters in the
            /// Application/Client/wwwroot/js Dexie.js file expects a JSON
            /// string as an argument to its body parameter which is the actual
            /// record to persist into the "createOperations" object store of
            /// our custom IndexedDB.
            string body = JsonSerializer.Serialize(entity);

            /// Ensure that names of the arguments are identical to the
            /// formal input parameters and in the same order of the
            /// Application/Client/wwwroot/js Dexie.js functions.
            await jsRuntime.InvokeVoidAsync(
                "persistCreateOperationParameters",
                body,
                controllerName,
                routeTemplateComplement);
        }

        /// <summary>
        /// Persists an IndexedDB record with the data required to build the
        /// Http request to perform an update operation. The record is produced
        /// because the operation is initiated while the application is
        /// offline.
        /// </summary>
        /// <remarks>
        /// The values of the formal input parameters are ultimately consumed
        /// by the <see cref="PwaSync"/> component. You can refer to Episode
        /// </remarks>
        /// <typeparam name="T">The type of the data entity object to update;
        /// e.g., <see cref="Movie"/> or <see cref="Genre"/>.</typeparam>
        /// <param name="jsRuntime">Represents an instance of the JS runtime to
        /// which calls may be dispatched.</param>
        /// <param name="entity">The data entity object to update.</param>
        /// <param name="controllerName">The name of the controller to which
        /// the Http request should be referred to.</param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint); For example:
        /// <code>$"/{entityId}"</code> You can refer to the ApiEntity class and
        /// the controller endpoint to check the route segment.
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        public static async ValueTask PersistUpdateOperationParameters<T>(
            this IJSRuntime jsRuntime,
            T entity,
            string controllerName,
            string routeTemplateComplement)
        {
            /// Converts the value of a .Net type into a JSON string. The
            /// persistUpdateOperationParameters in the
            /// Application/Client/wwwroot/js Dexie.js file expects a JSON
            /// string as an argument to its body parameter which is the actual
            /// record to persist into the "updateOperations" object store of
            /// our custom IndexedDB.
            string body = JsonSerializer.Serialize(entity);

            /// Ensure that names of the arguments are identical to the
            /// formal input parameters and in the same order of the
            /// Application/Client/wwwroot/js Dexie.js functions.
            await jsRuntime.InvokeVoidAsync(
                "persistUpdateOperationParameters",
                body,
                controllerName,
                routeTemplateComplement);
        }

        /// <summary>
        /// Persists an IndexedDB record with the data required to build the
        /// Http request to perform a delete operation. The record is produced
        /// because the operation is initiated while the application is
        /// offline.
        /// </summary>
        /// <remarks>
        /// The values of the formal input parameters are ultimately consumed
        /// by the <see cref="PwaSync"/> component. You can refer to Episode
        /// "151. Creando Géneros en modo offline" of Udemy course
        /// <see href="https://www.udemy.com/share/101ZK23@BWA7B2qaQr8iYkEhuUyGe2i-856Qbwt9t7TQjVdW20urn5unstcMIXrBftsraxFa/">
        /// Programando en Blazor - ASP.Net Core 7</see> by Felipe Gavilán.
        /// </remarks>
        /// <param name="jsRuntime">Represents an instance of the JS runtime to
        /// which calls may be dispatched.</param>
        /// <param name="controllerName">The name of the controller to which
        /// the Http request should be referred to.</param>
        /// <param name="routeTemplateComplement">Any additional route segments
        /// to add to the route template to build the URL that maps to a
        /// particular controller action (endpoint). For example:
        /// <code>$"/{movieId}"</code> You can refer to the ApiEntity class and
        /// the controller endpoint to check the route segment.  
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        public static async ValueTask PersistDeleteOperationParameters(
            this IJSRuntime jsRuntime,
            string controllerName,
            string routeTemplateComplement)
        {
            /// Ensure that names of the arguments are identical to the
            /// formal input parameters and in the same order of the
            /// Application/Client/wwwroot/js Dexie.js functions.
            await jsRuntime.InvokeVoidAsync(
                "persistDeleteOperationParameters",
                controllerName,
                routeTemplateComplement);
        }
        #endregion

        #region Push Notifications

        /// <summary>
        /// Encapsulates the JS code logic to use the Notification API
        /// interface to determine the state of the push notifications
        /// permission.
        /// <para>
        /// The state can be granted, denied, or default. The latter meaning
        /// that the application user has neither granted nor denied permission
        /// to receive push notifications from the web application (Flix Manager).
        /// </para>
        /// </summary>
        /// <remarks>
        /// Refer to Application/Client/wwwroot/js push-notifications.js file
        /// for more info on the web push notification functions and to
        /// "Episode 155. Push API - Frontend" of Udemy course
        /// <see href="https://www.udemy.com/share/101ZK23@YrCDF1LzB9xpEPReoWfAeEfW5Dgcw24qgKVUp5CCbuoWSebyL3OD9dz4D8gKjFCp/">
        /// Programando en Blazor - ASP.Net Core 7</see> by Felipe Gavilán.
        /// </remarks>
        /// <param name="jsRuntime">Represents an instance of the JS runtime
        /// to which calls may be dispatched.</param>
        /// <returns>A string value of 'denied', 'granted', or 'default'. The
        /// latter meaning that the application user has neither granted nor
        /// denied permission yet.
        /// </returns>
        public static async ValueTask<string>
            GetStatusNotificationPermissionAsync(this IJSRuntime jsRuntime)
        {
            /// Ensure that the JS function identifier is exactly the same as
            /// the one in the push-notifications.js file.
            return await jsRuntime.InvokeAsync<string>(
                "getStatusNotificationPermission");
        }

        /// <summary>
        /// Encapsulates the JS code logic to use the Notification API
        /// interface to subscribe the current user to the push service of the
        /// user agent (a computer program representing a person; e.g., a
        /// browser). 
        /// </summary>
        /// <param name="jsRuntime">Represents an instance of the JS runtime
        /// to which calls may be dispatched.</param>
        /// <returns>A <see cref="PushSubscriptionDetails"/> object with the
        /// subscription details in Base64 format to target a specific
        /// application user to send a push notification. The object should be
        /// persisted into the PushSubscriptionsDetails database table.
        /// </returns>
        public static async ValueTask<PushSubscriptionDetails?>
            SubscribeUserToPushNotificationsAsync(this IJSRuntime jsRuntime)
        {
            /// Ensure that the JS function identifier is exactly the same as
            /// the one in the push-notifications.js file.
            return await jsRuntime.InvokeAsync<PushSubscriptionDetails>(
                "subscribeUserToPushNotifications");
        }

        /// <summary>
        /// Encapsulates the JS code logic to use the Notification API
        /// interface to unsubscribe the current user from the push service of
        /// the user agent (a computer program representing a person; e.g., a
        /// browser).
        /// </summary>
        /// <param name="jsRuntime">Represents an instance of the JS runtime
        /// to which calls may be dispatched.</param>
        /// <returns>A <see cref="PushSubscriptionDetails"/> object with the
        /// subscription details in Base64 format to target a specific
        /// application user to send a push notification. The object should be
        /// removed from the PushSubscriptionsDetails database table.
        /// </returns>
        public static async ValueTask<PushSubscriptionDetails?>
            UnsubscribeUserFromPushNotificationsAsync(this IJSRuntime jsRuntime)
        {
            /// Ensure that the JS function identifier is exactly the same as
            /// the one in the push-notifications.js file.
            PushSubscriptionDetails PushSubscriptionDetails =
                await jsRuntime.InvokeAsync<PushSubscriptionDetails>(
                    "unsubscribeUserFromPushNotifications");

            return PushSubscriptionDetails;
        }

        #endregion
    }


    /// <summary>
    /// Custom class extends the functionality of the IJSObjectReference interface used
    /// to import JS modules from external .js files for a particular component (JS
    /// isolation). This method is just for illustrative purposes because it is not
    /// consumed but it could be used for a custom JS module for the DeleteMovie() method
    /// of the MoviesList component.  
    /// </summary>
    public static class IJSObjectReferenceExtensions
    {
        public static async ValueTask<bool> ConfirmDeleteImportJSModule(
            this IJSObjectReference jsObject,
            string jsFunction, string itemToDelete)
        {
            return await jsObject.InvokeAsync<bool>(jsFunction, itemToDelete);
        }
    }
}



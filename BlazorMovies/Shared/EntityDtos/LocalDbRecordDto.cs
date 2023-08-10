
using System.Text.Json;

namespace BlazorMovies.Shared.EntityDtos
{
    /// <summary>
    /// A record (or row) in one of the tables (stores) of our custom IndexedDB
    /// database (client-side storage) that represents an operation to CREATE,
    /// UPDATE, or DELETE an object while the application is offline. 
    /// </summary>
    /// <remarks>
    /// It is used to synchronize these operations with the web server once a
    /// connection is established. Its members will be passed as arguments to
    /// satisfy the formal input parameters of the resource methods in the
    /// Application/Client/ApiServices/ApiManager ApiConnector responsible for
    /// building the Http requests.
    /// <para>
    /// Refer to Application/Client/wwwroot/js Dexie.js file for more info on
    /// the IndexedDB and to Episode 150. Implementando el Componente
    /// Sincronizador of Udemy course <see href="https://www.udemy.com/share/101ZK23@IEBl2pf6lvRHAjqlgYU7NgEeEy5lo7Y5eszAMyUt4o_gJQQsrkCcwupCIigtPLTF/">
    /// Programando en Blazor - ASP.Net Core 7 
    /// </see> by Felipe Gavilán.
    /// </para>
    /// </remarks>
    public class LocalDbRecordDto
    {
        /// <summary>
        /// The field used as the primary key for identification of the stored
        /// object.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the Application/Server-Api controller of the resource
        /// (data entity) of interest; e.g., "genres" or "movies".
        /// </summary>
        /// <remarks>
        /// <strong>Ensure that names of formal input parameters are identical
        /// with Application/Client/wwwroot/js Dexie.js functions and with
        /// Application/Client/Helpers IJSRuntimeExtensions DexieDB features.
        /// Otherwise, model binding will likely present errors.</strong>
        /// <para>
        /// This field is used to build the Uri that the Http request must be
        /// sent to; i.e., the Uri that the routing middleware will use to
        /// match with the route template of the controller endpoint responsible
        /// for executing the operation. 
        /// </para>
        /// </remarks>
        public string ControllerName { get; set; } = null!;

        /// <summary>
        /// Any additional route segments to add to the route template to build
        /// the Uri that maps to a particular controller action (endpoint). It
        /// can include simple route segments (strings) and/or route parameters
        /// (inside curly braces) with or without constraints. For example:
        /// $"filter?textToSearch={movieTitle}".
        /// </summary>
        /// <remarks>
        /// <strong>Ensure that names of formal input parameters are identical
        /// with Application/Client/wwwroot/js Dexie.js functions and with
        /// Application/Client/Helpers IJSRuntimeExtensions DexieDB features.
        /// Otherwise, model binding will likely present errors.</strong>
        /// </remarks>
        public string RouteTemplateComplement { get; set; } = null!;

        /// <summary>
        /// The actual data entity object to create, update, or delete. A 
        /// <see cref="JsonElement"/> can represent any .Net object with any
        /// number of members (e.g., properties). 
        /// </summary>
        public JsonElement Body { get; set; }
    }

    /// <summary>
    /// Collection of records from either one of our custom IndexedDB database
    /// tables (client-side storage) that store all the related operations that
    /// need to be synchronized with the web server. One database table for
    /// CREATE, one for DELETE, and one for UPDATE operations. 
    /// </summary>
    /// <remarks>
    /// Refer to Application/Client/wwwroot/js Dexie.js file for more info on
    /// the databases and their stores (database tables) and to Episode 150.
    /// Implementando el Componente Sincronizador of Udemy course <see href="https://www.udemy.com/share/101ZK23@IEBl2pf6lvRHAjqlgYU7NgEeEy5lo7Y5eszAMyUt4o_gJQQsrkCcwupCIigtPLTF/">
    /// Programando en Blazor - ASP.Net Core 7 
    /// </see> by Felipe Gavilán.
    /// </remarks>>
    public class LocalDbRecordsDto
    {
        /// <summary>
        /// Collection of records from our custom "createOperations" store
        /// (database table) that need to be synchronized with the web server.
        /// </summary>
        /// <remarks>
        /// Property name must be identical to the property name of the
        /// getRecordsPendingOperations() function in the
        /// Application/Client/wwwroot/js/Dexie.js file. Otherwise JSON
        /// deserializing will fail
        /// in Application/Client/Helpers/IJSRuntimeExtensions
        /// GetPendingOperations() method.
        /// </remarks>
        public List<LocalDbRecordDto> ObjectsToCreate { get; set; } = new();

        /// <summary>
        /// Collection of records from our custom "deleteOperations" store
        /// (database table) that need to be synchronized with the web server.
        /// </summary>
        /// <remarks>
        /// Property name must be identical to the property name of the
        /// getRecordsPendingOperations() function in the
        /// Application/Client/wwwroot/js/Dexie.js file. Otherwise JSON
        /// deserializing will fail
        /// in Application/Client/Helpers/IJSRuntimeExtensions
        /// GetPendingOperations() method.
        /// </remarks>
        public List<LocalDbRecordDto> ObjectsToDelete { get; set; } = new();

        /// <summary>
        /// Collection of records from our custom "updateOperations" store
        /// (database table) that need to be synchronized with the web server.
        /// </summary>
        /// <remarks>
        /// Property name must be identical to the property name of the
        /// getRecordsPendingOperations() function in the
        /// Application/Client/wwwroot/js/Dexie.js file. Otherwise JSON
        /// deserializing will fail
        /// in Application/Client/Helpers/IJSRuntimeExtensions
        /// GetPendingOperations() method.
        /// </remarks>
        public List<LocalDbRecordDto> ObjectsToUpdate { get; set; } = new();

        /// <summary>
        /// Counts the total number of objects to create, delete, and update
        /// that are stored in our custom IndexedDB stores waiting to be 
        /// synchronized with the web server once a connection is established.
        /// </summary>
        /// <remarks>
        /// The instructor employs this method but it is unnecessary because
        /// we already have a getNumberOfPendingSynchronizations() function
        /// in the Dexie.js file which is invoked through the
        /// BlazorMovies/Client/Helpers IJSRuntimeExtensions class.
        /// <para>
        /// For more info you can refer to Episode 150. Implementando el
        /// Componente Sincronizador of Udemy course <see href="https://www.udemy.com/share/101ZK23@IEBl2pf6lvRHAjqlgYU7NgEeEy5lo7Y5eszAMyUt4o_gJQQsrkCcwupCIigtPLTF/">
        /// Programando en Blazor - ASP.Net Core 7 
        /// </see> by Felipe Gavilán.
        /// </para>
        /// </remarks>
        /// <returns>The number of create, update, and delete operations that
        /// are waiting to be synchronized with the web server.</returns>
        public int GetNumberOfPendingOperations()
        {
            int result = 0;

            result += ObjectsToCreate.Count;
            result += ObjectsToDelete.Count;
            result += ObjectsToUpdate.Count;

            return result;
        }
    }
}



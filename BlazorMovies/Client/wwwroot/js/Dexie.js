
/// JS functions to operate Dexie.js library which is a wrapper for IndexedDB,
/// a client-side (web browser) storage used to enable Progressive Web App
/// (PWA) offline features; i.e., store POST, PUT, and DELETE operations to
/// synchronize at a later time when the connection to a network server is 
/// available.
/// https://dexie.org/docs/Tutorial/Hello-World
/// https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API
/// Episode 149. Introducción a IndexedDB of Udemy course Programando en Blazor
/// - ASP.Net Core 7 by Felipe Gavilán. 
/// https://www.udemy.com/share/101ZK23@SISffxUyMuU19GXVJv5hXV6O951dBnUmqoh18WH91Pbb3jmO9hNJYr7K2pc1Mt-P/

// Declare DB instance; i.e., create the database.
// https://dexie.org/docs/API-Reference#quick-reference
var db = new Dexie("PwaDB");
var dbVersion = 1;

// Define the database schema; i.e., create stores (similar to database tables)
// for entities that user attempts to create, delete, or update while the 
// application is offline. Their explicitly defined "id" field will be used
// as a primary key to reference a record (or row). Unlike SQL-RDBMSs, which
// use fixed-column tables, IndexedDB is a JS oriented database that allows
// storing and retrieving objects that are indexed with a key and almost any
// kind of object can be stored.  
// https://developer.mozilla.org/en-US/docs/Web/API/IndexedDB_API#key_concepts_and_usage
// Episode 149. Introducción a IndexedDB of Udemy course Programando en Blazor
// - ASP.Net Core 7 by Felipe Gavilán. 
// https://www.udemy.com/share/101ZK23@SISffxUyMuU19GXVJv5hXV6O951dBnUmqoh18WH91Pbb3jmO9hNJYr7K2pc1Mt-P/
db.version(dbVersion).stores({
    createOperations: 'id++',
    updateOperations: 'id++',
    deleteOperations: 'id++'
});

// Retrieves all the available records from the database (IndexedDB).
// Episode 150. Implementando el Componente Sincronizador of Udemy course 
// Programando en Blazor - ASP.Net Core 7 by Felipe Gavilán. 
// https://www.udemy.com/share/101ZK23@SISffxUyMuU19GXVJv5hXV6O951dBnUmqoh18WH91Pbb3jmO9hNJYr7K2pc1Mt-P/
// Property names must be identical to the property names of 
// Application/Shared/EntityDtos/RecordsLocalDb class. Otherwise JSON 
// deserializing will fail in Application/Client/Helpers/IJSRuntimeExtensions
// GetPendingOperations() method.
async function getRecordsOfPendingOperations() {
    return await {
        ObjectsToCreate: await db.createOperations.toArray(),
        ObjectsToUpdate: await db.updateOperations.toArray(),
        ObjectsToDelete: await db.deleteOperations.toArray()
    };
}

// Delete record from the database (IndexedDB). Once a database record has 
// been synchronized with the web server, it must be removed from the local
// db (IndexedDB). 
// Episode 150. Implementando el componente sincronizador of Udemy course 
// Programando en Blazor - ASP.Net Core 7 by Felipe Gavilán.
// https://www.udemy.com/share/101ZK23@YrCDF1LzB9xpEPReoWfAeEfW5Dgcw24qgKVUp5CCbuoWSebyL3OD9dz4D8gKjFCp/
async function deleteLocalDbRecord(table, id) {
    await db[table].where({ "id": id }).delete();
}

// Calculates the total number of database records that represent operations
// that need to be synchronized with the web server.
// Episode 150. Implementando el componente sincronizador of Udemy course 
// Programando en Blazor - ASP.Net Core 7 by Felipe Gavilán.
// https://www.udemy.com/share/101ZK23@YrCDF1LzB9xpEPReoWfAeEfW5Dgcw24qgKVUp5CCbuoWSebyL3OD9dz4D8gKjFCp/
async function getNumberOfPendingSynchronizations() {
    const pendingToCreate = await db.createOperations.count();
    const pendingToUpdate = await db.updateOperations.count();
    const pendingToDelete = await db.deleteOperations.count();

    return pendingToCreate + pendingToUpdate + pendingToDelete;
}

// Persists an IndexedDB record with the data required to build the Http request
// to perform a create operation. The record is produced because the operation
// is initiated while the application is offline.
//
// The values of the formal input parameters are consumed by the 
// Application/Client/Shared PwaSync component which in turn employs a resource
// method of the ApiConnector class to build the Http request.
//
// The JSON.parse() method converts a JavaScript Object Notation (JSON) string
// into an object. The PersistCreateOperation method of the 
// Application/Client/Helpers IJSRuntimeExtensions class converts a .Net type
// to a JSON string to satisfy the body parameter of this method.
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/JSON/parse
// Episode 151. Creando géneros en modo offline of Udemy course "Programando en
// Blazor - ASP.Net Core 7" by Felipe Gavilán.
// https://www.udemy.com/share/101ZK23@BWA7B2qaQr8iYkEhuUyGe2i-856Qbwt9t7TQjVdW20urn5unstcMIXrBftsraxFa/
//
// The "put" method overwrites any previously existing values to avoid
// duplicates.
// https://dexie.org/docs/API-Reference#quick-reference
//
// Since we are using explicitly "named parameters", we must ensure that the
// names are identical with the argument names passed in 
// Application/Client/Helpers/IJSRuntimeExtensions DexieDB features. Otherwise,
// JSInterop will throw exception because model binding will fail.
async function persistCreateOperationParameters(
    body, controllerName, routeTemplateComplement) {
    await db.createOperations.put({
        body: JSON.parse(body),
        controllerName: controllerName,
        routeTemplateComplement: routeTemplateComplement
    });
}

// Persists an IndexedDB record with the data required to build the Http request
// to perform an update operation. The record is produced because the operation
// is initiated while the application is offline.
//
// The values of the formal input parameters are consumed by the 
// Application/Client/Shared PwaSync component which in turn employs a resource
// method of the ApiConnector class to build the Http request.
//
// The JSON.parse() method converts a JavaScript Object Notation (JSON) string
// into an object. The PersistUpdateOperation method of the 
// Application/Client/Helpers IJSRuntimeExtensions class converts a .Net type
// to a JSON string to satisfy the body parameter of this method.
// https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/JSON/parse
// 
// The "add" method does not overwrite any previously existing values because
// the user might want to update an object multiple times. It supports 
// cumulative updates while the application is offline.
// https://dexie.org/docs/API-Reference#quick-reference
//
// Since we are using explicitly "named parameters", we must ensure that the
// names are identical with the argument names passed in 
// Application/Client/Helpers/IJSRuntimeExtensions DexieDB features. Otherwise,
// JSInterop will throw exception because model binding will fail.
async function persistUpdateOperationParameters(
    body, controllerName, routeTemplateComplement) {
    await db.updateOperations.add({
        body: JSON.parse(body),
        controllerName: controllerName,
        routeTemplateComplement: routeTemplateComplement
    });
}

// Persists an IndexedDB record with the data required to build the Http request
// to perform a delete operation. The record is produced because the operation
// is initiated while the application is offline.
//
// The values of the formal input parameters are consumed by the 
// Application/Client/Shared PwaSync component which in turn employs a resource
// method of the ApiConnector class to build the Http request.
//
// Episode 151. Creando géneros en modo offline of Udemy course "Programando en
// Blazor - ASP.Net Core 7" by Felipe Gavilán.
// https://www.udemy.com/share/101ZK23@BWA7B2qaQr8iYkEhuUyGe2i-856Qbwt9t7TQjVdW20urn5unstcMIXrBftsraxFa/
// 
// The "put" method overwrites any previously existing values to avoid
// duplicates.
// https://dexie.org/docs/API-Reference#quick-reference
//
// Since we are using explicitly "named parameters", we must ensure that the
// names are identical with the argument names passed in 
// Application/Client/Helpers/IJSRuntimeExtensions DexieDB features. Otherwise,
// JSInterop will throw exception because model binding will fail.
async function persistDeleteOperationParameters(
    controllerName, routeTemplateComplement) {
    await db.deleteOperations.put({
        controllerName: controllerName,
        routeTemplateComplement: routeTemplateComplement
    });
}



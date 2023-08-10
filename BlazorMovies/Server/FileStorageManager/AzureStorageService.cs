using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BlazorMovies.Server.FileStorageManager
{
    /// <summary>
    /// Implements the IFileStorageService interface; i.e., it
    /// provides the specific functionality to upload, download,
    /// edit, and delete blobs from an Azure Storage Account. For
    /// example, images, documents, files, video, audio, and
    /// restore or analysis data.
    /// </summary>
    /// <remarks>
    /// Every Blob (Binary Large Object) stored has an address that
    /// includes the account name. The base address for the objects
    /// in the storage account is a combination of the account name
    /// and the blob endpoint; e.g.,
    /// <para>
    /// https://storageaccount.blob.core.windows.net/containername/blobname
    /// </para>
    /// <para>
    /// The Azure SDK for .Net uses a <dfn>Client</dfn> object
    /// instance to access and manipulate Azure blob storage
    /// resources such as an storage account (BlobServiceClient),
    /// a container (BlobContainerClient) or a blob (BlobClient).
    /// </para>
    /// </remarks>
    public class AzureStorageService : IFileStorageService
    {
        /// <summary>
        /// Stores a reference to the AzureStorageConnection string.
        /// </summary>
        private readonly string? _connectionString;

        /// <summary>
        /// Injects a dependency to the IConfiguration interface.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a mirror usage from the Application/Server-Api/Startup
        /// class where the database connection string is configured.
        /// </para>
        /// <para>
        /// The procedure followed is from Udemy course Programming in
        /// Blazor - ASP.Net Core 5 by Felipe Gavilán ep. 69 Saving an
        /// image in Azure Storage. For .Net's suggested approach visit:
        /// </para>
        /// <para>
        /// <see href="https://docs.microsoft.com/en-us/dotnet/azure/sdk/dependency-injection">
        /// Dependency Injection with the Azure SDK for .Net</see>
        /// </para>
        /// <para>
        /// <see href="https://devblogs.microsoft.com/azure-sdk/best-practices-for-using-azure-sdk-with-asp-net-core/">
        /// Best practices for using Azure SDK with ASP.Net Core.</see>
        /// </para>
        /// </remarks>
        /// <param name="configuration">The IConfiguration interface
        /// represents a set of key/value application configuration
        /// properties.</param>
        public AzureStorageService(IConfiguration configuration)
        {
            /// Retrieves the AzureStorageConnection string registered
            /// in the Application/Server-Api/appsettings.json (development)
            /// file.
            _connectionString = configuration
                .GetConnectionString(
                    "AzureStorageConnection");
        }

        /// <summary>
        /// Uploads a blob into an Azure Storage Account.
        /// </summary>
        /// <param name="content">The data to be stored (or uploaded) to the
        /// Azure storage account container.</param>
        /// <param name="extension">The file extension; i.e., the file type.
        /// </param>
        /// <param name="containerName">The container (folder) name for the 
        /// blob (or data content).</param>
        /// <returns>A string representation of the blob's (data) base address.
        /// A combination of the account name and the Azure storage blob
        /// endpoint. E.g.:
        /// <para>
        /// https://blazormovies0.blob.core.windows.net/images-people/771476bf-d598-4998-9f69-7ba7524856bc.jpg
        /// </para>
        /// </returns>
        public async Task<string> SaveFile(
            byte[] content,
            string extension,
            string containerName)
        {
            /// Creates an instance of type BlobContainerClient that can be used
            /// to gain access to and manipulate an Azure storage container.
            ///
            /// A container organizes a set of blobs. Similar to a directory (or
            /// folder) in a file system.
            BlobContainerClient client = new BlobContainerClient(
                _connectionString, containerName);

            /// Creates a new container under the specified storage account. If a
            /// container with the same name already exists, it is not changed.
            await client.CreateIfNotExistsAsync();

            /// Sets the permissions for the container to indicate whether the
            /// blob container data may be accessed publicly.
            await client.SetAccessPolicyAsync(PublicAccessType.Blob);

            /// Concatenates a globally unique identifier with the file extension
            /// of the data to be stored. 
            string fileName = $"{Guid.NewGuid()}{extension}";

            /// Creates an instance of type BlobClient that can be used to gain
            /// access to and manipulate a blob (binary large object).
            BlobClient blob = client.GetBlobClient(fileName);

            /// Creates a stream with the memory as a backing store. It
            /// uses the byte[] to create the memory stream that contains the
            /// content to upload.  
            await using MemoryStream ms = new MemoryStream(content);

            /// Creates a new block blob or updates the content of an existing
            /// block blob by overwriting any existing data. It returns an
            /// Azure.Response<typeparam name="T">T</typeparam> describing the
            /// state of the updated block blob.
            Response<BlobContentInfo> response = await blob.UploadAsync(ms);

            /// Gets the blob's primary BlobBaseClient.Uri endpoint represented
            /// as a string. 
            return blob.Uri.ToString();
        }

        /// <summary>
        /// Deletes a blob from an Azure Storage Account.
        /// </summary>
        /// <param name="fileRoute">The file route to the blob (or data
        /// content).</param>
        /// <param name="containerName">The container (folder) name for the 
        /// blob (or data content).</param>
        /// <returns>An asynchronous operation.</returns>
        public async Task DeleteFile(string fileRoute, string containerName)
        {
            if (string.IsNullOrEmpty(fileRoute))
                return;

            /// Creates an instance of type BlobContainerClient that can be used
            /// to gain access to and manipulate an Azure storage container.
            ///
            /// A container organizes a set of blobs. Similar to a directory (or
            /// folder) in a file system.
            BlobContainerClient client = new BlobContainerClient(
                _connectionString, containerName);

            /// Creates a new container under the specified storage account. If a
            /// container with the same name already exists, it is not changed.
            await client.CreateIfNotExistsAsync();

            /// Gets a reference to the file name and extension of the specified
            /// path string. 
            string fileName = Path.GetFileName(fileRoute);

            /// Creates an instance of type BlobClient that can be used to gain
            /// access to and manipulate a blob (binary large object).
            BlobClient blob = client.GetBlobClient(fileName);

            /// Marks the specified blob for deletion, the blob is later deleted
            /// during garbage collection. The response includes an Http status
            /// code and a set of response headers. E.g., a successful operation
            /// returns a status code 202 (Accepted).
            ///
            /// In order to delete a blob, you must delete all of its snapshots.
            /// You can delete both at the same time using IncludeSnapshots.
            /// https://docs.microsoft.com/en-us/rest/api/storageservices/delete-blob
            Response<bool> response = await blob
                .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }

        /// <summary>
        /// Updates a blob from an Azure Storage Account.
        /// </summary>
        /// <remarks>
        /// It employs the DeleteFile() and SaveFile() methods.
        /// </remarks>
        /// <param name="content">The data to be updated in the
        /// Azure storage account container.</param>
        /// <param name="extension">The file extension; i.e., the file type.
        /// </param>
        /// <param name="containerName">The container (folder) name for the 
        /// blob (or data content).</param>
        /// <param name="fileRoute">The file route to the blob (or data
        /// content).</param>
        /// <returns>A string representation of the blob's (data) base address.
        /// A combination of the account name and the Azure storage blob
        /// endpoint. E.g.:
        /// <para>
        /// https://blazormovies0.blob.core.windows.net/images-people/771476bf-d598-4998-9f69-7ba7524856bc.jpg
        /// </para>
        /// </returns>
        public async Task<string> EditFile(
            byte[] content,
            string extension,
            string containerName,
            string fileRoute)
        {
            if (!string.IsNullOrEmpty(fileRoute))
            {
                await DeleteFile(fileRoute, containerName);
            }

            return await SaveFile(content, extension, containerName);
        }

        /// <summary>
        /// Creates a collection of names of the blobs (objects) stored in a
        /// given Azure storage account container. 
        /// </summary>
        /// <remarks>
        /// See <see href="https://stackoverflow.com/questions/32057636/how-to-get-a-list-of-all-the-blobs-in-a-container-in-azure">
        /// How to get a list of all blobs in a container in Azure?</see> and
        /// <see href="https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blobs-list">
        /// List blobs with .Net</see>.
        /// </remarks>
        /// <param name="containerName">The name of the blob container in the
        /// storage account.</param>
        /// <returns>A collection of the names of the blobs found in the
        /// container.</returns>
        public async Task<List<string>?> GetFileNamesInContainerAsync(
            string containerName)
        {
            /// Creates an instance of type BlobContainerClient that can
            /// be used to gain access to and manipulate an Azure storage
            /// container.
            ///
            /// A container organizes a set of blobs. Similar to a
            /// directory (or folder) in a file system.
            BlobContainerClient client = new BlobContainerClient(
                _connectionString, containerName);

            /// Retrieves an async sequence of blobs that enumerates the
            /// values a <see cref="Page{T}"/> at a time.
            ///
            /// You can use the formal input parameter "prefix" to specify
            /// a string with one or more characters. Azure storage then
            /// returns only the blobs whose names start with that prefix.
            IAsyncEnumerable<Page<BlobItem>> blobResultSegments =
                client.GetBlobsAsync()
                    .AsPages(pageSizeHint: 50);

            List<string> blobNames = new();

            await foreach (Page<BlobItem> blobPage in blobResultSegments)
            {
                foreach (BlobItem blobItem in blobPage.Values)
                {
                    /// Creates an instance of type BlobClient that can be used
                    /// to gain access to and manipulate a blob (binary large
                    /// object).
                    BlobClient blob = client.GetBlobClient(blobItem.Name);

                    /// Returns an encoded form of the Uri that points to the
                    /// blob object in the specified container.
                    string absoluteUri = blob.Uri.AbsoluteUri;

                    blobNames.Add(blobItem.Name);
                }
            }

            return blobNames;
        }

        /// <summary>
        /// Copies a blob from a source container to a destination container
        /// keeping the same blob name. If the blob already exists in the
        /// destination container, it is replaced and its metadata is
        /// overwritten. 
        /// </summary>
        /// <remarks>
        /// It assumes both containers are in the same Azure storage account
        /// because it has the connection string to the storage account
        /// hardcoded.
        /// <para>
        /// See <see href="https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-copy">
        /// Copy a blob with .NET</see>.
        /// </para>
        /// </remarks>
        /// <param name="fileName">The name of the blob to copy.</param>
        /// <param name="sourceContainerName">The name of the container that
        /// is the source of the data.</param>
        /// <param name="destinationContainerName">The name of the container
        /// that is the destination for the copied data.</param>
        /// <returns>True if the "x-ms-copy-status" header value of the
        /// response is "success". Otherwise, false.</returns>
        public async Task<bool> CopyFileAsync(
            string fileName,
            string sourceContainerName,
            string destinationContainerName)
        {
            /// BlobContainerClient can be used to gain access to and
            /// manipulate an Azure storage container. A container
            /// organizes a set of blobs. Similar to a directory (or
            /// folder) in a file system.
            ///
            /// GetBlobClient creates a BlobClient object that can be
            /// used to gain access to and manipulate a blob (binary large
            /// object).
            BlobClient sourceBlob = new BlobContainerClient(
                    _connectionString,
                    sourceContainerName)
                    .GetBlobClient(fileName);

            BlobClient destinationBlob = new BlobContainerClient(
                    _connectionString,
                    destinationContainerName)
                    .GetBlobClient(fileName);

            /// Executes an asynchronous copy of the data from the source blob
            /// to the destination blob. 
            CopyFromUriOperation copyOperation =
                await destinationBlob.StartCopyFromUriAsync(sourceBlob.Uri);

            /// Periodically calls UpdateStatusAsync on the server until
            /// HasCompleted status is true. It returns the final result
            /// of the operation. 
            await copyOperation.WaitForCompletionAsync();

            /// Checks for the latest status of the copy operation.
            Response copyOperationResponse =
                await copyOperation.UpdateStatusAsync();

            /// Attempts to find the "x-ms-copy-status" header from the
            /// response and evaluates is its value is "success". 
            return copyOperationResponse.Headers
                       .TryGetValue(
                           "x-ms-copy-status",
                           out string? copyStatusValue)
                   && copyStatusValue.Equals("success");
        }

        /// <summary>
        /// Copies the whole range of blobs in a source container to a
        /// destination container keeping the same blob names. If a blob
        /// already exists in the destination container, it is replaced
        /// and its metadata is overwritten. 
        /// </summary>
        /// <remarks>
        /// It assumes both containers are in the same Azure storage account
        /// because the <see cref="CopyFileAsync"/> custom method employed
        /// has the connection string to the storage account hardcoded.
        /// <para>
        /// See <see href="https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-copy">
        /// Copy a blob with .NET</see>.
        /// </para>
        /// </remarks>
        /// <param name="sourceContainerName">The name of the container that
        /// is the source of the data.</param>
        /// <param name="destinationContainerName">The name of the container
        /// that is the destination for the copied data.</param>
        /// <returns>An asynchronous operation.</returns>
        public async Task CopyContainerContentAsync(
            string sourceContainerName,
            string destinationContainerName)
        {
            /// Custom method retrieves the names of the blobs stores in the
            /// container.
            List<string>? blobNames =
                await GetFileNamesInContainerAsync(sourceContainerName);

            foreach (string blobName in blobNames)
            {
                /// Custom method copies a blob from source container to a
                /// destination container. It returns true if copy operation
                /// is successful. 
                bool copyOperationSuccessful = await CopyFileAsync(
                    blobName,
                    sourceContainerName,
                    destinationContainerName);
            }
        }

        /// <summary>
        /// Deletes the data content from a container in an Azure storage
        /// account. The custom method employed <see cref="DeleteFile"/>,
        /// eliminates all the blobs' snapshots too.
        /// </summary>
        /// <remarks>
        /// See <see href="https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-delete">
        /// Delete and restore a blob with .Net</see>.
        /// </remarks>
        /// <param name="containerName">The name of the blob container in the
        /// storage account with the data to be removed.</param>
        /// <returns>An asynchronous operation.</returns>
        public async Task DeleteContainerContentAsync(string containerName)
        {
            foreach (string blobName in
                     (await GetFileNamesInContainerAsync(containerName))!)
            {
                /// Creates an instance of type BlobContainerClient that can
                /// be used to gain access to and manipulate an Azure storage
                /// container.
                ///
                /// A container organizes a set of blobs. Similar to a
                /// directory (or folder) in a file system.
                BlobContainerClient client = new BlobContainerClient(
                    _connectionString, containerName);

                /// Creates an instance of type BlobClient that can be used to
                /// gain access to and manipulate a blob (binary large object).
                BlobClient blob = client.GetBlobClient(blobName);

                /// Returns an ecoded form of the Uri that points to the blob
                /// object in the specified container.
                string absoluteUri = blob.Uri.AbsoluteUri;

                /// Custom method deletes a blob from a container in an Azure
                /// storage account. It eliminates all of its snapshots too. 
                await DeleteFile(absoluteUri, containerName);
            }
        }
    }
}



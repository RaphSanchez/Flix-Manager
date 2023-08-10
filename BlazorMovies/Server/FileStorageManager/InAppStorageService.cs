
namespace BlazorMovies.Server.FileStorageManager
{
    /// <summary>
    /// Implements the IFileStorageService interface; i.e., it provides
    /// the specific functionality to save, retrieve, update, and delete
    /// application assets in a container that resides in the application's
    /// web server root directory (Application/Server-Api/wwwroot).
    /// <remarks>
    /// The web server root directory is capable of serving static files
    /// (e.g., images, documents, files, video, audio, and restore or
    /// analysis data) to the client.
    /// <para>
    /// Anything outside of the web root folder is not web-addressable.
    /// This setup provides an additional level of security that prevents
    /// accidental exposing of project files. 
    /// </para>
    /// <para>
    /// This service is intended for case scenarios where data manipulation
    /// will be performed within an on-premises physical drive (e.g., in
    /// your computer). 
    /// </para>
    /// </remarks>
    /// </summary>
    public class InAppStorageService : IFileStorageService
    {
        /// <summary>
        /// Provides information about the web hosting environment that the
        /// application is running in. E.g., the absolute path to the app's
        /// web root directory.
        /// </summary>
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Provides access to the intrinsic HttpContext.Request,
        /// HttpContext.Response, and HttpContext.Server properties with
        /// current information about an individual Http request/response.
        /// E.g., The Http scheme (protocol) and host (may include port
        /// number). 
        /// </summary>
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Injects a dependency to the IWebHostEnvironment and the
        /// IHttpContextAccessor.
        /// </summary>
        /// <param name="env">The web hosting environment that the
        /// app is running. E.g., local host or web host.</param>
        /// <param name="httpContextAccessor">Provides access to the
        /// intrinsic HttpContext.Request, HttpContext.Response, and
        /// HttpContext.Server properties with current information
        /// about an individual Http request/response.</param>
        public InAppStorageService(
            IWebHostEnvironment env,
            IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Stores application assets in a container that resides in the
        /// application's web server root directory
        /// (Application/Server-Api/wwwroot). 
        /// </summary>
        /// <param name="content">The data object to store.</param>
        /// <param name="extension">The file extension; i.e., the file
        /// type.</param>
        /// <param name="containerName">The container (folder) name for the
        /// data object.</param>
        /// <returns>A string representation of the path that points to the
        /// location of the data object.</returns>
        public async Task<string> SaveFile(
            byte[] content,
            string extension,
            string containerName)
        {
            /// Constructs the file name. It concatenates a globally unique
            /// identifier with the file extension of the data to be stored.
            string fileName = $"{Guid.NewGuid()}{extension}";

            /// Constructs the folder path to store the data object.
            /// Concatenates the absolute path to the application's webroot
            /// directory (Application/Server-Api) with the name of the
            /// container (directory). 
            string folder = Path.Combine(_env.WebRootPath, containerName);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            /// Constructs the absolute path to store the data object. It
            /// concatenates the absolute path to the web root directory
            /// with the name of the container (or folder) and the file
            /// name.
            string fileDirectory = Path.Combine(folder, fileName);

            /// Creates a new file, writes the specified byte array to the
            /// file, and closes the file. If the target file already exists,
            /// it is overwritten.
            await File.WriteAllBytesAsync(fileDirectory, content);

            /// Gets the scheme (e.g., https) and host (e.g., localhost:7077)
            /// of the current Http request (the client's http request to store
            /// the data object).  
            string httpRequestUrl =
                $"{_httpContextAccessor.HttpContext?.Request.Scheme}://" +
                $"{_httpContextAccessor.HttpContext?.Request.Host}";

            /// Constructs the absolute path to retrieve or access the data
            /// object from an Http request (client request). It concatenates
            /// the client's Http request (scheme and host) with the folder
            /// name and the data object's file name. 
            string externalFileRoute = Path.Combine(
                httpRequestUrl,
                containerName,
                fileName);

            return externalFileRoute;
        }

        /// <summary>
        /// Deletes a data object stored in a container that resides in the
        /// application's web server root directory
        /// (Application/Server-Api/wwwroot). 
        /// </summary>
        /// <param name="fileRoute">The path that points to the location of
        /// the data object to delete.</param>
        /// <param name="containerName">The container (folder) name for the
        /// data object.</param>
        /// <returns>An asynchronous operation.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task DeleteFile(
            string fileRoute,
            string containerName)
        {
            /// Gets the file name and extension from the specified path. 
            string fileName = Path.GetFileName(fileRoute);

            /// Constructs the absolute path of the location of the data
            /// object.
            string fileDirectory = Path.Combine(_env.WebRootPath,
                containerName,
                fileName);


            if (File.Exists(fileDirectory))
            {
                /// Deletes the specified file. 
                File.Delete(fileDirectory);
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Updates a data object stored in a container that resides in the
        /// application's web server root directory
        /// (Application/Server-Api/wwwroot). 
        /// </summary>
        /// <remarks>
        /// It employs the DeleteFile() and SaveFile() methods.
        /// </remarks>
        /// <param name="content">The data object with the new content.
        /// </param>
        /// <param name="extension">The file extension; i.e., the file type.
        /// </param>
        /// <param name="containerName">The container (folder) name for the
        /// data object.</param>
        /// <param name="fileRoute">The path that points to the location of
        /// the data object to modify.</param>
        /// <returns>A string representation of the path that points to the
        /// location of the data object.</returns>
        /// <exception cref="NotImplementedException"></exception>
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
        /// Creates a collection of names of the files stored in a local
        /// directory. 
        /// </summary>
        /// <param name="containerName">The name of the directory where the
        /// data is stored.</param>
        /// <returns>A collection of the names of the files found in the
        /// container (directory).</returns>
        public async Task<List<string>?> GetFileNamesInContainerAsync(
            string containerName)
        {
            /// Constructs the folder path of the directory to retrieve the
            /// objects from. Concatenates the absolute path to the
            /// application's webroot directory (Application/Server-Api) with
            /// the name of the container (directory). 
            string directory = Path.Combine(_env.WebRootPath, containerName);

            /// Store the collection of file names.
            List<string>? fileNames = new();

            if (Directory.Exists(directory))
            {
                foreach (string file in Directory.EnumerateFiles(directory))
                {
                    string fileName = Path.GetFileName(file);

                    fileNames.Add(fileName);
                }
            }

            return fileNames;
        }

        /// <summary>
        /// Copies a file from a source container (e.g., local directory) to
        /// a destination container keeping the same file name. If the file
        /// already exists in the destination container, it is replaced.
        /// </summary>
        /// <remarks>
        /// See <see href="https://learn.microsoft.com/en-us/dotnet/standard/io/asynchronous-file-i-o">
        /// Asynchronous File I/O</see>.
        /// </remarks>
        /// <param name="fileName">The name of the file to copy.</param>
        /// <param name="sourceContainerName">The name of the directory that
        /// is the source of the data.</param>
        /// <param name="destinationContainerName">The name of the directory
        /// that is the destination for the copied data.</param>
        /// <returns>True if the copy operation is successful. Otherwise,
        /// false.</returns>
        public async Task<bool> CopyFileAsync(
            string fileName,
            string sourceContainerName,
            string destinationContainerName)
        {
            /// Absolute path of the source directory.
            string sourceDirectory =
                Path.Combine(_env.WebRootPath, sourceContainerName);

            /// Absolute path of the destination directory.
            string destinationDirectory =
                Path.Combine(_env.WebRootPath, destinationContainerName);

            /// Creates a new directory if destinationContainerName does not
            /// exist. Use with caution because if destinationContainerName
            /// has a spelling error, it will create a new directory.
            //if (!Directory.Exists(destinationFolder))
            //    Directory.CreateDirectory(destinationFolder);

            if (Directory.Exists(sourceDirectory))
            {
                /// Two FileStream objects to copy files asynchronously from
                /// one directory to another. 
                await using FileStream sourceStream = File.Open(
                    Path.Combine(sourceDirectory, fileName),
                    FileMode.Open);

                /// Creates or overwrites a file in the specified path. 
                await using FileStream destinationStream =
                    File.Create(Path.Combine(destinationDirectory, fileName));

                /// Asynchronously reads the bytes from the current stream and
                /// writes them to another stream. 
                await sourceStream.CopyToAsync(destinationStream);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Copies the whole range of files in a source container (e.g., local
        /// directory) to a destination container keeping the same file names.
        /// If a file already exists in the destination container, it is
        /// overwritten.
        /// </summary>
        /// <param name="sourceContainerName">The name of the container that
        /// is the source of the data.</param>
        /// <param name="destinationContainerName">The name of the container
        /// that is the destination for the copied data.</param>
        /// <returns>An asynchronous operation.</returns>
        public async Task CopyContainerContentAsync(
            string sourceContainerName,
            string destinationContainerName)
        {
            /// Custom method retrieves the names of the files stored in the
            /// container.
            List<string>? fileNames =
                await GetFileNamesInContainerAsync(sourceContainerName);

            foreach (string fileName in fileNames)
            {
                /// Custom method copies a file from a source container to a
                /// destination container. It returns true if copy operation
                /// is successful.
                bool copyOperationSuccessful = await CopyFileAsync(
                    fileName, sourceContainerName, destinationContainerName);
            }
        }

        /// <summary>
        /// Deletes the data content from a local directory. 
        /// </summary>
        /// <param name="containerName">The name of the directory with the
        /// data to be removed.</param>
        /// <returns>An asynchronous operation.</returns>
        public async Task DeleteContainerContentAsync(string containerName)
        {
            /// Absolute path of the directory that contains the data to be
            /// removed.
            string directory =
                Path.Combine(_env.WebRootPath, containerName);

            /// Custom method retrieves the names of the files stored in the
            /// directory
            List<string>? fileNames =
                await GetFileNamesInContainerAsync(containerName);

            if (Directory.Exists(directory))
            {
                foreach (string fileName in fileNames)
                {
                    /// Deletes the specified file.
                    File.Delete($"{directory}\\{fileName}");
                }
            }
        }
    }
}



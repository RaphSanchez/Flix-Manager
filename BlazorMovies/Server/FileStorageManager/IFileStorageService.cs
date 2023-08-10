
namespace BlazorMovies.Server.FileStorageManager
{
    /// <summary>
    /// Encapsulates functionality to upload, download and delete
    /// data objects from a cloud service. For example, images,
    /// documents, files, video, audio, and restore or analysis
    /// data.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Uploads a data object into a cloud service.
        /// </summary>
        /// <param name="content">The data to be stored (or uploaded) to the
        /// storage account container.</param>
        /// <param name="extension">The file extension; i.e., the file type.
        /// </param>S
        /// <param name="containerName">The container (folder) name for the 
        /// data content.</param>
        /// <returns>A string representation of the object's URL; e.g.,
        /// <para>
        /// https://blazormovies0.blob.core.windows.net/images-people/771476bf-d598-4998-9f69-7ba7524856bc.jpg
        /// </para></returns>
        Task<string> SaveFile(
            byte[] content,
            string extension,
            string containerName);

        /// <summary>
        /// Deletes a data object from a cloud service.
        /// </summary>
        /// <param name="fileRoute">The file route to the data content.</param>
        /// <param name="containerName">The container (folder) name for the 
        /// data content.</param>
        /// <returns>An asynchronous operation.</returns>
        Task DeleteFile(
            string fileRoute,
            string containerName);

        /// <summary>
        /// Downloads a data object from a cloud service.
        /// </summary>
        /// <remarks>
        /// It employs the DeleteFile() and SaveFile() methods.
        /// </remarks>
        /// <param name="content">The data to be updated in the
        /// storage account container.</param>
        /// <param name="extension">The file extension; i.e., the file type.
        /// </param>
        /// <param name="containerName">The container (folder) name for the 
        /// data content.</param>
        /// <param name="fileRoute">The file route to the data content.</param>
        /// <returns>A string representation of the object's URL; e.g.,
        /// <para>
        /// https://blazormovies0.blob.core.windows.net/images-people/771476bf-d598-4998-9f69-7ba7524856bc.jpg
        /// </para></returns>
        Task<string> EditFile(
            byte[] content,
            string extension,
            string containerName,
            string fileRoute);

        /// <summary>
        /// Creates a collection of names of the objects (e.g., files or blobs)
        /// stored in a given container (e.g., local directory or Azure storage
        /// account container).
        /// </summary>
        /// <param name="containerName">The name of the container where the
        /// data is stored.</param>
        /// <returns>A collection of the names of the objects found in the
        /// container.</returns>
        Task<List<string>?> GetFileNamesInContainerAsync(string containerName);

        /// <summary>
        /// Copies an object from a source container (e.g., local directory
        /// or Azure storage account container) to a destination container
        /// keeping the same object name. If the object already exists in the
        /// destination container, it is replaced and its metadata is
        /// overwritten. 
        /// </summary>
        /// <param name="fileName">The name of the object to copy.</param>
        /// <param name="sourceContainerName">The name of the container that
        /// is the source for the data.</param>
        /// <param name="destinationContainerName">The name of the container
        /// that is the destination for the data.</param>
        /// <returns>True if the copy operation is successful. Otherwise,
        /// false.</returns>
        Task<bool> CopyFileAsync(
            string fileName,
            string sourceContainerName,
            string destinationContainerName);

        /// <summary>
        /// Copies the whole range of objects in a source container (e.g., a
        /// local directory or an Azure storage account container) to a
        /// destination container keeping the same object names. If an object
        /// already exists in the destination container, it is overwritten.
        /// </summary>
        /// <param name="sourceContainerName">The name of the container that
        /// is the source of the data.</param>
        /// <param name="destinationContainerName">The name of the container
        /// that is the destination for the copied data.</param>
        /// <returns>An asynchronous operation.</returns>
        Task CopyContainerContentAsync(
            string sourceContainerName,
            string destinationContainerName);

        /// <summary>
        /// Deletes the data content from a container (e.g., a local directory
        /// or an Azure storage account container).
        /// </summary>
        /// <param name="containerName">The name of the container with the
        /// data to be removed.</param>
        /// <returns>An asynchronous operation.</returns>
        Task DeleteContainerContentAsync(string containerName);
    }
}



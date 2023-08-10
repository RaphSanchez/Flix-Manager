using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Allows the user to insert an image into the web browser by selecting
    /// an image file. It embeds the selected image into the web browser using
    /// Base64 encoding. 
    /// </summary>
    /// <remarks>
    /// Employs an <see cref="OnImageSelected"/> event callback that notifies
    /// the parent component when a new image file has been selected and it
    /// supplies the text representation of this image in Base64 encoding.
    /// <para>
    /// <strong>Warning!!</strong> It does NOT scan the uploaded file(s) with
    /// an antivirus/antimalware software.
    /// </para>
    /// </remarks>
    public partial class UploadImage
    {
        /// <summary>
        /// Text representation, encoded in Base64, of the binary data
        /// (byte array) of the uploaded file(s).
        /// </summary>
        private string _imageBase64 = null!;

        /// <summary>
        /// Text representation of the Uniform Resource Locator (URL)
        /// that points to the current Person.PictureUrl image.
        /// </summary>
        private string _imageUrl = null!;

        /// <summary>
        /// The InputFile component was not assigned a "multiple"
        /// attribute which means it can only accept 1 file. Here
        /// we use GetMultipleFiles for demo but you should use the
        /// "File" property instead unless you define a "multiple"
        /// attribute for the InputFile component.
        /// </summary>
        private IReadOnlyList<IBrowserFile> _imageFiles = null!;

        /// <summary>
        /// Stores the data read with the ReadAsync() method
        /// of the selected file (type IBrowserFile).
        /// </summary>
        byte[] bytesRead = Array.Empty<byte>();

        /// <summary>
        /// The name assigned to the Image-Picture field in the parent
        /// component.
        /// </summary>
        [Parameter]
        public string ImageFieldName { get; set; } = "Image";

        /// <summary>
        /// Informs the parent component (consumer) that a new file
        /// (image) has been selected and that it must do something
        /// with it; e.g., assign the new image to the current Person
        /// object cached by the parent component. It passes back the text
        /// representation of the new image in Base64 encoding.
        /// </summary>
        /// <remarks>
        /// This event handler delegate is called from the LoadFilesAsync()
        /// event handler (method) after the uploaded file(s) has been
        /// encoded to Base64.
        /// </remarks>
        [Parameter]
        public EventCallback<string> OnImageSelected { get; set; }

        /// <summary>
        /// If the parent (consumer) is the Edit Person component, the
        /// UploadImage component should render the Person.Picture.
        /// </summary>
        /// <remarks>
        /// To do so, it passes the URL (string) of the stored image
        /// into an img HTML element. If this parameter is null, the
        /// code to render the image is not executed.
        /// </remarks>
        [Parameter]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Handles the image files selected by the user through an
        /// InputFile component. It employs an IBrowserFile type
        /// that represents the data of a file and converts the binary
        /// data (bytes) into a text representation encoded in Base64
        /// which is embedded into the browser (UI).
        /// </summary>
        /// <remarks>
        /// Note that the InputFile built-in component only executes
        /// form-validation by default as opposed to field-validation.
        /// The consumer of this component (PersonForm) performs an
        /// EditContext.Validate() in its OnImageSelected() handler to
        /// force validation and update any validation error messages.
        /// </remarks>
        /// <param name="args">Provides access to the selected file
        /// list and details about each file. The LoadFilesAsync event
        /// handler method must match the signature of the OnChange
        /// event of the InputFile component.</param>
        /// <returns>An asynchronous operation.</returns>
        private async Task LoadFilesAsync(InputFileChangeEventArgs args)
        {
            /// If you intend to upload large files, refer to
            /// Programming/03-CSharpAdvanced/Section2/11-AsyncAwaitModel-Plinq-HttpClient/WebContent app.

            _imageFiles = args.GetMultipleFiles(1);

            /// Note that this code logic stores the uploaded file into
            /// a local string variable with the string representation of
            /// the byte array data but encoded in Base64. For an example
            /// on how to save the file into disk refer to:
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/file-uploads?view=aspnetcore-5.0&pivots=server
            /// where it is shown how to AVOID SECURITY RISKS!!!
            foreach (IBrowserFile imageFile in _imageFiles)
            {
                FileInfo fInfo = new FileInfo(imageFile.Name);
                if (!fInfo.Extension.HasValidImageExtension())
                {
                    await jsRuntime
                        .AlertDialogBox("Valid file types are: " +
                                        ".jpeg, .jpg, or .png");
                    break;
                }

                if (imageFile.Size >= 1024000)
                {
                    await jsRuntime.SwAlDisplayMessageAsync(
                            title: "Warning",
                            message: "Image file size cannot be greater than 1 MB.",
                            swAlIconType: SwAlIconType.warning);
                    break;
                }

                bytesRead = new byte[imageFile.Size];

                /// Opens the stream for reading the uploaded file and
                /// stores it in the byte[] passed as argument. Default
                /// maxAllowedSize of file is 512,000 bytes / 1024 =
                /// 500KB. This example defines a max value of 1MB.
                ///
                /// Note that FOR SECURITY REASONS, the stream that represents
                /// the file's bytes SHOULD NOT BE READ DIRECTLY INTO MEMORY.
                /// Instead, it should BE COPIED TO AN EXTERNAL STORE, scanned,
                /// and only then consumed. For more info visit:
                /// https://docs.microsoft.com/en-us/aspnet/core/blazor/file-uploads?view=aspnetcore-5.0&pivots=webassembly#file-streams
                await imageFile.OpenReadStream(1024000).ReadAsync(bytesRead);

                /// Converts a byte[] (bytesRead) to its equivalent string
                /// representation that is encoded with Base64 digits.
                _imageBase64 = Convert.ToBase64String(bytesRead);

                /// Dispatches an event notification to the parent component
                /// and it passes the text representation of the image file(s)
                /// encoded in Base64.
                await OnImageSelected.InvokeAsync(_imageBase64);

                /// If the user selects a new image file for the Person.Picture,
                /// then the previously existing ImageUrl should be null to avoid
                /// embedding two images into the browser.
                _imageUrl = null;

                /// Apparently not needed. The instructor included it but it
                /// does not update the Person.Picture validation error messages
                /// of the PeopleCreate component and everything else seems to be
                /// working fine withou it. You replaced it with a call to the
                /// EditContext.Validate() method from the OnImagesSelected event
                /// handler of the parent (consumer) component PersonForm.
                //StateHasChanged();
            }
        }

        protected override void OnInitialized()
        {
            /// Determines if the Person.PictureUrl passed by the
            /// parent (consumer) component to satisfy the ImageUrl
            /// parameter is encoded in Base64.
            ///
            /// Each time the user selects a new image for the current
            /// Person object, the LoadFilesAsync() method takes the
            /// image file selected by the user and encodes it to
            /// Base64. It then invokes the OnImageSelected event callback
            /// to pass the encoded string to the parent component which
            /// in turn embeds the Base64 image into the web browser.
            ///
            /// If the Person object is "saved", the image is stored
            /// in the database as a string encoded in Base64. As a
            /// result, to render the Person image correctly when the
            /// PersonEdit component retrieves a Person object from
            /// the database, this UploadImage component has to check
            /// if the stored Person.PictureUrl is a Base64 encoded
            /// image or a string with a URL that points to an image
            /// file.
            bool isBase64Encoded = ImageUrl.IsBase64();

            /// Stores the current Person.PictureUrl string in the
            /// appropriate local variable used by the conditional
            /// statements in the markup section to render the image.
            switch (isBase64Encoded)
            {
                case false:
                    _imageUrl = ImageUrl;
                    _imageBase64 = null;
                    break;

                case true:
                    _imageUrl = null;
                    _imageBase64 = ImageUrl;
                    break;
            }
        }
    }
}

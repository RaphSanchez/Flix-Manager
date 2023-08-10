using BlazorMovies.Shared.AuthZHelpers;
using BlazorMovies.Shared.EDM;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorMovies.Client.Pages.People
{
    /// <summary>
    /// Consumed by the <see cref="PersonCreate"/> and <see cref="PersonEdit"/>
    /// routable components to display information on the <see cref="Person"/>
    /// element to create or delete.
    /// </summary>
    /// <remarks>
    /// The <see cref="PersonForm"/> creates an <see cref="EditContext"/> based
    /// on the assigned model instance. The <see cref="EditContext"/> tracks
    /// metadata about the edit process including which fields have been
    /// modified and the current validation messages. It can be used to bind a
    /// form to data.
    /// <para>
    /// Assign either an EditContext or a Model to an <see cref="EditForm"/>
    /// component but never both. <see cref="EditContext"/> is required if you
    /// intend to implement custom validation CSS class attributes.
    /// </para>
    /// </remarks>
    public partial class PersonForm
    {
        /// <summary>
        /// An <EditForm/> creates an EditContext based on the assigned
        /// model instance. The EditContext tracks metadata about the
        /// edit process including which fields have been modified and
        /// the current validation messages. It can be used to bind a
        /// form to data.
        /// </summary>
        /// <remarks>
        /// Assign either an EditContext or a Model to an <EditForm/>
        /// component but never both. EditContext is required if you intend
        /// to implement custom validation CSS class attributes.
        /// </remarks>
        private EditContext EditContext { get; set; } = null!;

        /// <summary>
        /// Stores a pre-existing Person.Picture during initialization and
        /// is used to satisfy the ImageUrl parameter of the UploadImage
        /// component. It is set to null if and when the user uploads a
        /// new image file.
        /// </summary>
        private string _existingImageUrl = null!;

        [Parameter] 
        public Person Person { get; set; } = null!;

        /// <summary>
        /// Executes a parent component's method when this child
        /// component's OnValidSubmit event is raised.
        /// </summary>
        [Parameter]
        public EventCallback OnValidSubmit { get; set; }

        /// <summary>
        /// Represents the authorization policy to enforce with the
        /// <see cref="AuthorizeView"/> component that secures the button
        /// element to "Save Changes" dependent on whether the
        /// <see cref="PersonForm"/> component was invoked to create or to
        /// delete a <see cref="BlazorMovies.Shared.EDM.Person"/> item.
        /// </summary>
        private string _authorizationPolicy = string.Empty;

        protected override void OnInitialized()
        {
            /// Binds the  PersonForm component to an instance of type
            /// Person (entity).
            EditContext = new EditContext(Person);

            if (!string.IsNullOrEmpty(Person.PictureUrl))
            {
                /// Stores an existing image file path into a local variable.
                /// This image file is passed to satisfy the ImageURL parameter
                /// of the UploadImage component responsible for embedding the
                /// image into the web browser.
                _existingImageUrl = Person.PictureUrl;

                /// Setting the Person.Picture to null prevents the system
                /// from sending the image file through the web API every
                /// time the user edits a field. If the user selects a new
                /// image file, the OnImageSelected() handler will take care
                /// of sending the new image file through the web API.
                /// Note that it will produce a data validation error for the
                /// Person.Picture if decorated with a "Required" annotation.
                /// You have to choose between a more efficient Web API and
                /// having data validation for the Person.Picture field.
                //Person.Picture = null;
            }
        }

        protected override void OnParametersSet()
        {
            /// Allocates the appropriate authorization policy after this
            /// PersonForm component has received its parameters from the
            /// parent component (from its consumer).
            ///
            /// If Person.Id is zeroe, it means the intention is to create a
            /// new Person object. Otherwise, the purpose is to edit an
            /// existing Person because the Person object already has an Id
            /// value.
            _authorizationPolicy = Person.Id == 0
                ? AuthZPolicies.ApiCreateContent
                : AuthZPolicies.ApiEditContent;
        }

        /// <summary>
        /// Stores the text representation in Base64 encoding of the image
        /// file selected by the user and captured by the
        /// OnImageSelected<string> event callback parameter of the
        /// <UploadImage> component consumed here for the Picture field.
        /// </summary>
        /// <param name="imageBase64">The string representation encoded in
        /// Base64 of the image file uploaded by the user.</param>
        private void OnImageSelected(string imageBase64)
        {
            Person.PictureUrl = imageBase64;

            /// If the user uploads a new image file, the local variable
            /// responsible for storing the pre-existing image file is
            /// set to null.
            _existingImageUrl = null;

            /// The <InputFile> built-in component employed in your
            /// UploadImage component, only executes form-validation
            /// by default as opposed to executing field-validation.
            /// You need to force an EditContext.Validate whenever the
            /// user selects a new image file or the validation error
            /// messages of the <InputFile> fields are not reset unless
            /// the user submits the form.
            EditContext.Validate();
        }
    }
}

using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.AuthZHelpers;
using BlazorMovies.Shared.EDM;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorMovies.Client.Pages.Genres
{
    /// <summary>
    /// Consumed by the <see cref="GenreCreate"/> and <see cref="GenreEdit"/>
    /// routable components to display information on the <see cref="Genre"/>
    /// element to create or delete.
    /// </summary>
    /// <remarks>
    /// The <see cref="GenreForm"/> creates an <see cref="EditContext"/> based
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
    public partial class GenreForm
    {
        /// <summary>
        /// Genre to create or edit with the Genre Form component.
        /// </summary>
        [Parameter]
        public Genre Genre { get; set; }

        /// <summary>
        /// Executes a parent component's method when this child
        /// component's OnValidSubmit event is raised. 
        /// </summary>
        [Parameter]
        public EventCallback OnValidSubmit { get; set; }

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
        private EditContext editContext;

        /// <summary>
        /// Represents the authorization policy to enforce with the
        /// <see cref="AuthorizeView"/> component that secures the button
        /// element to "Save Changes" dependent on whether the
        /// <see cref="GenreForm"/> component was invoked to create or to
        /// delete a <see cref="BlazorMovies.Shared.EDM.Genre"/> item.
        /// </summary>
        private string _authorizationPolicy = string.Empty;

        protected override void OnInitialized()
        {
            /// Binds the GenreForm component to an instance of type
            /// Genre (entity).
            editContext = new EditContext(Genre);

            /// Associates the supplied FieldCSSClassProvider with the supplied
            /// EditContext. This customizes the CSS class names used within the
            /// EditContext.
            editContext.SetFieldCssClassProvider(new CustomFieldClassProvider());
        }

        protected override void OnParametersSet()
        {
            /// Allocates the appropriate authorization policy after this
            /// GenreForm component has received its parameters from the
            /// parent component (from its consumer).
            ///
            /// If Genre.Name is either null or empty, it means the intention
            /// is to create a new Genre. Otherwise, the purpose is to edit an
            /// existing Genre.
            _authorizationPolicy = string.IsNullOrEmpty(Genre.Name)
                ? AuthZPolicies.ApiCreateContent
                : AuthZPolicies.ApiEditContent;
        }
    }
}


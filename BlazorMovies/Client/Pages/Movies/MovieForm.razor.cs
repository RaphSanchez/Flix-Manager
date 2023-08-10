using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Pages.Movies
{
    public partial class MovieForm
    {
        /// <summary>
        /// An &lt;EditForm&gt; creates an EditContext based on the assigned
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
        private EditContext? _editContext;

        /// <summary>
        /// Stores a pre-existing Movie.PosterPath during initialization
        /// and is used to satisfy the ImageUrl parameter of the UploadImage
        /// component. It is set to null if and when the user uploads a
        /// new image file.
        /// </summary>
        private string? _existingPosterPathUrl;

        /// <summary>
        /// Each object (e.g., item of type Genre) in the UnSelectedGenres
        /// parameter satisfied by the consumer (ancestor) is mapped
        /// during initialization to this collection type which is a custom
        /// Data Transfer Object. Each object in the (mapped) collection has
        /// a &lt;Key, Value&gt; pair. For example:
        /// &lt;Genre.Id, Genre.Name&gt;
        /// </summary>
        private List<MultipleSelectorDto>? _mappedUnselectedOptions = new();

        /// <summary>
        /// Each object (e.g., item of type Genre) in the SelectedGenres
        /// parameter satisfied by the consumer (ancestor) is mapped
        /// during initialization to this collection type which is a custom
        /// Data Transfer Object. Each object in the (mapped) collection has
        /// a &lt;Key, Value&gt; pair. For example:
        /// &lt;Genre.Id, Genre.Name&gt;
        /// </summary>
        private List<MultipleSelectorDto>? _mappedSelectedOptions = new();

        /// <summary>
        ///  Exposes one IEntityName interface for each data entity mapped to
        /// the database. 
        /// </summary>
        [Inject] private IApiService? ApiService { get; set; }

        /// <summary>
        /// Provides an abstraction for querying and managing URI navigation.
        /// </summary>
        [Inject] private NavigationManager? NavManager { get; set; }

        /// <summary>
        /// Represents an instance of a JavaScript runtime to which calls may
        /// be dispatched. 
        /// </summary>
        [Inject] private IJSRuntime? JsRuntime { get; set; }

        /// <summary>
        /// The Movie object to bind the EditContext of the &lt;EditForm&gt;
        /// built-in component.
        /// </summary>
        [Parameter]
        public Movie? Movie { get; set; }

        /// <summary>
        /// Executes a parent component's method when this child
        /// component's OnValidSubmit event is raised.
        /// </summary>
        [Parameter]
        public EventCallback<MovieEssentialsDto> OnValidSubmit { get; set; }

        /// <summary>
        /// Represents the collection of "UnSelected" options. This
        /// parameter is passed as an argument to satisfy the
        /// "UnSelected" parameter of the MultipleSelector component.
        /// </summary>
        [Parameter]
        public List<Genre>? UnSelectedGenres { get; set; }

        /// <summary>
        /// Represents the collection of "Selected" options. This
        /// parameter is passed as an argument to satisfy the
        /// "Selected" parameter of the MultipleSelector component.
        /// </summary>
        [Parameter]
        public List<Genre>? SelectedGenres { get; set; }

        protected override void OnInitialized()
        {
            /// Binds the  MovieForm component to an instance of type
            /// Movie (data entity).
            _editContext = new EditContext(Movie!);

            _editContext.SetFieldCssClassProvider(new CustomFieldClassProvider());

            if (!string.IsNullOrEmpty(Movie?.PosterPath))
            {
                /// Stores an existing image file path into a local variable.
                /// This image file is passed to satisfy the ImageURL parameter
                /// of the UploadImage component responsible for embedding the
                /// image into the web browser.
                _existingPosterPathUrl = Movie.PosterPath;

                /// Setting the Movie.PosterPath to null prevents the system
                /// from sending the image file through the web API every
                /// time the user edits a field. If the user selects a new
                /// image file, the OnPosterSelected() handler will take care
                /// of sending the new image file through the web API.
                /// Note that it will produce a data validation error for the
                /// Movie.PosterPath if decorated with a "Required"
                /// annotation. You have to choose between a more efficient
                /// Web API and having data validation for the Person.Picture
                /// field.
                //Movie.PosterPath = null;
            }

            /// Mapping function maps each UnSelectedGenres item of type Genre
            /// to an item in the _mappedUnselectedOptions collection with a
            /// custom type (DTO). The resulting collection is fed to the
            /// MultipleSelector component.
            _mappedUnselectedOptions = UnSelectedGenres?
                .Select(g => new MultipleSelectorDto(g.Id, g.Name))
                .ToList();

            /// Mapping function maps each SelectedGenres item of type Genre
            /// to an item in the _mappedSelectedOptions collection with a
            /// custom type (DTO). The resulting collection is fed to the
            /// MultipleSelector component.
            _mappedSelectedOptions = SelectedGenres?
                .Select(g => new MultipleSelectorDto(g.Id, g.Name))
                .ToList();
        }

        /// <summary>
        /// Stores the text representation in Base64 encoding of the image
        /// file selected by the user and captured by the
        /// OnImageSelected()
        /// EventCallback<typeparam name="string">&lt;string&gt;</typeparam>
        /// parameter of the &lt;UploadImage&gt; component consumed here for
        /// the Poster field.
        /// </summary>
        /// <param name="imageBase64">The string representation encoded in
        /// Base64 of the image file uploaded by the user.</param>
        private void OnPosterSelected(string imageBase64)
        {
            Movie!.PosterPath = imageBase64;

            /// If the user uploads a new image file, the local variable
            /// responsible for storing the pre-existing image file is
            /// set to null.
            _existingPosterPathUrl = null;

            /// The <InputFile> built-in component, employed in your
            /// UploadImage component, only executes form-validation
            /// by default as opposed to executing field-validation.
            /// You need to force an EditContext.Validate whenever the
            /// user selects a new image file. Otherwise, the validation
            /// error messages of the <InputFile> fields are not reset
            /// until the user submits the form.
            _editContext?.Validate();
        }

        #region TypeaheadDragSelect component

        /// <summary>
        /// A collection of type Person that contains any previously
        /// selected actors for the current Movie item.
        /// </summary>
        [Parameter]
        public List<Person>? SelectedActors { get; set; }

        /// <summary>
        /// Search function invoked from the Typeahead component. Replaces
        /// an EventCallback because callbacks don't have a return type. It
        /// is passed as argument to satisfy the Typeahead SearchMethodFunc
        /// parameter that expects an asynchronous method with an input
        /// parameter of type <em>string</em> and a return type
        /// Task<typeparam name="IEnumerable">&lt;IEnumerable
        /// <typeparam name="Person">&lt;Person&gt;</typeparam>&gt;</typeparam>
        /// items.
        /// </summary>
        /// <param name="searchText">The text typed by the user into the
        /// HTML input element of the Typeahead component.</param>
        /// <returns>The matching items (suggestions).</returns>
        private async Task<IEnumerable<Person>> SearchActors(string searchText)
        {
            try
            {
                /// Encapsulates property values that can be directly related
                /// to one or more properties of a Person type.
                PeopleQueryFilterDto peopleDto = new(name: searchText);

                /// Queries the database to obtain Person items whose property
                /// values match the values encapsulated in the 
                /// PeopleQueryFilterDto passed as an argument.
                return await ApiService?.People.FilterAsync(peopleDto)!;
            }
            catch(Exception ex)
            {
                string customExceptionMessage =
                    ex.InnerException != null
                    && ex.InnerException
                        .GetType().ToString().Contains("JSException")
                        ? "You must be online to use this feature."
                        : ex.Message;

                await JsRuntime.SwAlDisplayMessageAsync(
                    "Info",
                    customExceptionMessage,
                    SwAlIconType.warning);

                return new List<Person>();
            }
        }

        #endregion

        /// <summary>
        /// Handler for the built-in <em>OnValidSubmit</em> callback invoked
        /// when the MovieForm is submitted.
        /// </summary>
        /// <remarks>
        /// It performs any required preprocessing on input data. When finished,
        /// it invokes the custom OnValidSubmit parameter bound to an event
        /// handler that notifies the parent (consumer) component that the
        /// form has been validated.
        /// </remarks>
        /// <returns>An asynchronous operation.</returns>
        private async Task OnDataAnnotationsValidated()
        {
            /// Stores the primary keys of any Genre items related to the
            /// current Movie object.
            int[]? movieRelatedGenres = null;

            /// Stores the primary keys and Person.CharacterName values
            /// of any Person items (actors) related to the current Movie
            /// object.
            Dictionary<int, string?>? movieRelatedActors = null;

            if (_mappedSelectedOptions != null)
            {
                /// Projects each element of the mapped Genre items
                /// selected by the user into an array of primary keys
                /// of type Int32.
                movieRelatedGenres = _mappedSelectedOptions
                    .Select(mSO => mSO.Key)
                    .ToArray();
            }

            if (SelectedActors != null)
            {
                /// Creates a Dictionary<int, string> with a couple
                /// properties of the Person items selected by the user.
                movieRelatedActors = SelectedActors
                .ToDictionary(a => a.Id, a => a.TempCharacterName);
            }

            /// Builds a MovieEssentialsDto object that encapsulates the
            /// Movie object and any related data (entities).
            MovieEssentialsDto movieDto = new(
                Movie,
                movieRelatedGenres,
                movieRelatedActors);

            /// Invokes the delegate associated with this binding and dispatches
            /// an event notification to the appropriate component (consumer).
            await OnValidSubmit.InvokeAsync(movieDto);
        }
    }
}

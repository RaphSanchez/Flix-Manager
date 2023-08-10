using BlazorMovies.Shared.EDM;
using Microsoft.AspNetCore.Components;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Comprises two sections. The one on the left receives markdown input
    /// from the browser and the one on the right consumes the
    /// <see cref="RenderMarkdown"/> component for converting the markdown
    /// text to HTML, sanitizing it, and rendering the result into the web
    /// browser.
    /// </summary>
    /// <remarks>
    /// It is limited to be consumed by the PersonForm because its markdown
    /// input text area field is bound to the Biography property of a type
    /// <see cref="Person"/>.
    /// <para>
    /// For any other type (or class), you can use the
    /// <see cref="InputMarkdownDerived"/> component which is more flexible
    /// because it can be bound to any type (or class).
    /// </para>
    /// </remarks>
    public partial class InputMarkdown
    {
        /// <summary>
        /// The name provided for the field; e.g., Biography
        /// </summary>
        [Parameter]
        public string? FieldName { get; set; }

        /// <summary>
        /// The Model that contains the Model.Property to bind the
        /// InputTextArea.Value to; e.g., Person.Biography.
        /// </summary>
        [Parameter]
        public Person? Person { get; set; }
    }
}

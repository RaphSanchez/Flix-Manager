using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Custom component comprises two sections. The one on the left receives
    /// markdown input from the browser and the one on the right consumes the
    /// <see cref="RenderMarkdown"/> component to convert the markdown text
    /// to HTML, sanitize it, and render the result into the web browser.
    /// </summary>
    /// <remarks>
    /// It derives from a built-in <see cref="InputTextArea"/> component. This
    /// approach allows to post-pone the definition of the type (object) to
    /// bind the underlying &lt;InputTextArea&gt; value to because it passes
    /// the "CurrentValue" property inherited from the InputTextArea base class.
    /// <para>
    /// It is the parent (consumer of the <see cref="InputMarkdownDerived"/>
    /// component) the one responsible for defining the property value to bind
    /// to by explicitly declaring the attribute-value pair; e.g.,
    /// <code>&lt;InputMarkdownDerived @bind-Value="Movie.Summary"</code>
    /// </para>
    /// </remarks>
    public partial class InputMarkdownDerived : InputTextArea
    {
        /// <summary>
        /// The name provided for the field; e.g., Biography or Summary.
        /// </summary>
        [Parameter]
        public string? FieldName { get; set; }
    }
}

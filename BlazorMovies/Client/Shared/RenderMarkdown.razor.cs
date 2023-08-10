using Ganss.Xss;

using Markdig;

using Microsoft.AspNetCore.Components;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Converts markdown text to HTML, sanitizes the produced HTML, and
    /// producing an instance of type <see cref="MarkupString"/> that Blazor
    /// can render into the web browser.
    /// </summary>
    /// <remarks>
    /// This component is consumed by the <see cref="InputMarkdown"/> and the
    /// <seealso cref="InputMarkdownDerived"/> components. 
    /// </remarks>
    public partial class RenderMarkdown
    {
        /// Important!!
        /// The correct order for the procedure is:
        /// 1. Convert input text to HTML.
        /// 2. Sanitize the output.
        /// 3. Construct the MarkupString.

        /// <summary>
        /// Private backing field for MarkdownContent property.
        /// </summary>
        private string? _markdownContent;

        /// <summary>
        /// Stores the sanitized HTML constructed as an instance
        /// of type MarkupString that Blazor can render.
        /// </summary>
        protected MarkupString HTMLContent { get; private set; }

        /// <summary>
        /// The string content to convert to HTML and sanitize.
        /// </summary>
        [Parameter]
        public string MarkdownContent
        {
            get => _markdownContent ?? string.Empty;

            set
            {
                _markdownContent = value;
                HTMLContent = ConvertStringToMarkupString(_markdownContent);
            }
        }

        /// <summary>
        /// Takes a string value (markdown), converts it to an HTML string,
        /// sanitizes it, and constructs an instance of MarkupString with
        /// the sanitized HTML string.
        /// </summary>
        /// <param name="value">The string value (markdown) to convert.</param>
        /// <returns>An instance of MarkupString that Blazor can render.</returns>
        private MarkupString ConvertStringToMarkupString(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                /// Convert markdown string to HTML
                string html = Markdown.ToHtml(value,
                    new MarkdownPipelineBuilder()
                        .UseAdvancedExtensions()
                        .Build());

                /// New instance of the HTML sanitizer. You can also
                /// register it as a service in the Dependency Injection
                /// container if you plan to use it frequently.
                HtmlSanitizer htmlSanitizer = new HtmlSanitizer();

                /// Sanitize HTML before rendering
                string sanitizedHtmlString = htmlSanitizer.Sanitize(html);

                /// Return sanitized HTML as a MarkupString that Blazor
                /// can render.
                return new MarkupString(sanitizedHtmlString);
            }

            return new MarkupString();
        }
    }
}

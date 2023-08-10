using Microsoft.AspNetCore.Components;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Templated component tests a collection of items of type
    /// <typeparamref name="TItem"/> for nullability and emptiness. It accepts
    /// UI templates as parameters used for its rendering logic. Dependent on
    /// the state of the collection of items, it renders a
    /// <see cref="NullLoadingTemplate"/>, an <see cref="EmptyTemplate"/>, a
    /// <see cref="RenderItemTemplate"/>, or a
    /// <seealso cref="WholeListTemplate"/> passed by the consumer.
    /// </summary>
    /// <remarks>
    /// If none of these formal input parameters are met, it provides a
    /// default template dependent on the matching state (null, empty, or with
    /// items).
    /// </remarks>
    /// <typeparam name="TItem">The type of the collection of items passed for
    /// testing and rendering.</typeparam>
    public partial class TestCollectionNullOrEmpty<TItem>
    {
        
        /// <remarks>
        /// A render fragment parameter represents a segment of UI to render. It is
        /// satisfied by providing specific UI content that will be rendered. Naming
        /// convention if you only have one RenderFragment parameter is to name it
        /// ChildContent. Else, use meaningful names with 'Template' as a suffix.
        /// </remarks>

        /// <summary>
        /// Allows its consumer to provide specific rendering configuration when the
        /// collection is null.
        /// </summary>
        [Parameter] public RenderFragment? NullLoadingTemplate { get; set; }

        /// <summary>
        /// Allows its consumer to provide specific rendering configuration when the
        /// the collection is empty.
        /// </summary>
        [Parameter] public RenderFragment? EmptyTemplate { get; set; }

        /// <summary>
        /// Allows its consumer to provide specific rendering configuration for each
        /// item in the collection.
        /// </summary>
        /// <remarks>
        /// Rendering configuration is assigned individually to each item collection.
        /// </remarks>
        [Parameter] public RenderFragment<TItem>? RenderItemTemplate { get; set; }
    
        /// <summary>
        /// Allows its consumer to provide rendering configuration for the collection
        /// as a whole. 
        /// </summary>/
        /// <remarks>
        /// Rendering configuration is usually used for &lt;table&gt; elements and
        /// alike. 
        /// </remarks>
        [Parameter] public RenderFragment? WholeListTemplate { get; set; }

        /// <summary>
        /// Generic list parameter allows you to postpone the specification of the
        /// type of collection to work with.
        /// </summary>
        [Parameter] public List<TItem>? ItemsList { get; set; }
    }
}

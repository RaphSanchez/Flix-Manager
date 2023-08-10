
namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// Its members can be passed as values to styling attributes of any
    /// DOM element. The StylingValues class is made available throughout
    /// the application by Cascading Values and Parameters. The 'MainLayout'
    /// layout component is the ancestor component and the 'Counter' and
    /// 'Index' components consume the cascading object value (StylingValues class). 
    /// </summary>
    public class StylingValues
    {
        /// <summary>
        /// Note that if you assign default values to the properties, the
        /// "select" DOM element used to provide multiple options will not
        /// display the options correctly; i.e., the first select element
        /// (e.g., Color) will display the first option in the element
        /// but the second select element (e.g., FontSize) will appear
        /// blank unless the user clicks on the down arrow to view any
        /// available option values. 
        /// </summary>
        public string? Color { get; set; } 

        public string? FontSize { get; set; } 
    }
}



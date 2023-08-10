
using Microsoft.AspNetCore.Components.Forms;

namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// Supplies CSS class names for form fields to represent their validation
    /// state; i.e., it applies your previously created custom CSS styles in
    /// the global app.css file for "valid" and "invalid" states of the fields
    /// of a form. 
    /// </summary>
    /// <remarks>You can apply custom styles to only a subset of the fields of
    /// a form by implementing a conditional statement. Refer to Custom validation
    /// CSS class attributes for more info:
    /// https://docs.microsoft.com/en-us/aspnet/core/blazor/forms-validation?view=aspnetcore-5.0#custom-validation-css-class-attributes-1
    /// </remarks>
    public class CustomFieldClassProvider : FieldCssClassProvider
    {
        /// <summary>
        /// Gets a string that indicates the status of the specified field as
        /// a CSS class.
        /// </summary>
        /// <returns>A CSS class name string</returns>
        public override string GetFieldCssClass(EditContext editContext,
            in FieldIdentifier fieldIdentifier)
        {
            /// GetValidationMessages gets the current validation messages for
            /// the specified field. If there are any messages at all, the field
            /// is not valid. 
            bool isValid =
                !editContext.GetValidationMessages(fieldIdentifier).Any();

            if (editContext.IsModified(fieldIdentifier))
            {
                /// Your custom CSS classes represent the status of a given field.
                return isValid ? "modified valid-field" : "modified invalid-field";
            }
            else
            {
                /// Your custom CSS classes represent the status of a given field.
                return isValid ? "valid-field" : "invalid-field";
            }
        }
    }
}



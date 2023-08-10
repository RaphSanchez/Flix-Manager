using System.ComponentModel.DataAnnotations;

using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Shared.CustomAttributes
{
    /// <summary>
    /// Custom model validation attribute ensures that the Movie.InTheaters
    /// property value is false when Movie.ReleaseDate is in the future.
    /// </summary>
    /// <remarks>
    /// This approach includes localization for the
    /// <see cref="ValidationAttribute.ErrorMessage"/> when data validation
    /// was unsuccessful; i.e., it attempts to provide the error message
    /// translated into the user's current culture if supported by the
    /// application.
    /// <para>
    /// See <see href="https://iamrks-dev.medium.com/create-custom-validation-attribute-with-localization-support-in-c-net-core-3-1-343ba7a4b8ca">
    /// Create Custom Validation Attribute with localization support in C#
    /// .Net Core 3.1</see>.
    /// </para>
    /// </remarks>
    public class Movie_EnsureInTheatersAttribute : ValidationAttribute
    {
        // Validates the specified value with the validation rules
        // defined here. Returns the results of a validation request.
        protected override ValidationResult IsValid(
            object? value,
            ValidationContext validationContext)
        {
            /// Retrieves the model object from the ValidationContext
            /// where this custom validation attribute is invoked.
            /// 
            /// The ValidationContext provides additional information
            /// such as the model instance created by model biding. The
            /// currentMovie variable represents a Movie object that
            /// contains the data from the form submission.
            Movie? currentMovie = validationContext.ObjectInstance as Movie;

            /// Tests the condition on the current Movie object by invoking
            /// data validaton logic defined in the related data entity class.
            ///
            /// The actual validation logic is decoupled from the ASP.Net core
            /// MVC technology.
            bool isValid = currentMovie!.ValidateInTheaters();

            /// Employs a custom method to supply a localized error message
            /// and includes a fallback (or default culture) error message.
            if (!isValid)
                return new ValidationResult(GetErrorMessage(validationContext));

            return ValidationResult.Success!;
        }

        /// <summary>
        /// Designed to satisfy the formal input parameter of a
        /// <see cref="ValidationResult"/> object for cases where the data
        /// validation was unsuccessful. 
        /// </summary>
        /// <param name="validationContext">Describes the context in which a
        /// validation check is performed.</param>
        /// <returns>An error message to satisfy the formal input parameter of
        /// a <see cref="ValidationResult"/> object for cases where the data
        /// validation was unsuccessful.</returns>
        private string GetErrorMessage(ValidationContext validationContext)
        {
            /// Attempts to retrieve a
            /// <see cref="ValidationAttribute.ErrorMessage"/> property value,
            /// if the consumer of the
            /// <see cref="Movie_EnsureInTheatersAttribute"/> did not supply a
            /// value, it attempts to retrieve a
            /// <see cref="ValidationAttribute.ErrorMessageResourceName"/>
            /// property value, if the consumer did not provide a value either,
            /// it supplies a hard coded string as the fallback error message
            /// for the <see cref="ValidationResult"/> object.
            return !string.IsNullOrEmpty(ErrorMessage)
                ? ErrorMessage
                : !string.IsNullOrEmpty(ErrorMessageResourceName)
                    ? ErrorMessageString
                    : "Cannot be in theaters if release date is in the future.";
        }
    }

    #region Not Localized

    /// <summary>
    /// Custom model validation attribute ensures that the Movie.InTheaters
    /// property value is false when Movie.ReleaseDate is in the future.
    /// </summary>
    /// <remarks>
    /// This approach does not include localization for the Error Message.
    /// </remarks>
    //public class Movie_EnsureInTheatersAttribute : ValidationAttribute
    //{
    //    // Validates the specified value with the validation rules
    //    // defined here. Returns the results of a validation request.
    //    protected override ValidationResult IsValid(
    //        object? value,
    //        ValidationContext validationContext)
    //    {
    //        /// Retrieves the model object from the ValidationContext
    //        /// where this custom validation attribute is invoked.
    //        /// 
    //        /// The ValidationContext provides additional information
    //        /// such as the model instance created by model biding. The
    //        /// currentMovie variable represents a Movie object that
    //        /// contains the data from the form submission.
    //        Movie? currentMovie = validationContext.ObjectInstance as Movie;

    //        /// Tests the condition on the current Movie object by invoking
    //        /// data validation logic defined in the related data entity class.
    //        ///
    //        /// The actual validation logic is decoupled from the ASP.Net core
    //        /// MVC technology.
    //        bool isValid = currentMovie!.ValidateInTheaters();

    //        if (!isValid)
    //            return new ValidationResult(
    //                "Cannot be in theaters if release date is in the future.");

    //        return ValidationResult.Success!;
    //    }
    //}

    #endregion
}

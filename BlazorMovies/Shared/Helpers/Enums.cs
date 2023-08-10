// ReSharper disable InconsistentNaming
namespace BlazorMovies.Shared.Helpers
{
    /// <summary>
    /// Defines the available options for the Http authorization headers when
    /// building Http requests/responses. 
    /// </summary>
    /// <remarks>
    /// Used as the type for the formal input parameter that determines the
    /// <see cref="HttpClient"/> service to employ by the
    /// Application/Client/ApiServices/ApiManager ApiConnector class to
    /// build (serialize/deserialize) the Http requests/responses. E.g., one
    /// that attaches authorization JWTs to the requests or one that doesn't.
    /// </remarks>
    public enum JwtOptions
    {
        IncludeJWTs,
        OmitJWTs
    }

    /// <summary>
    /// Defines the available predefined icon types in the SweetAlert2 JS
    /// library.
    /// </summary>
    /// <remarks>
    /// See <see href="https://sweetalert2.github.io/#icons">Icons</see> and
    /// <see cref="https://youtu.be/lo6xPG_N2Ew">5-Blazor: The Power of the
    /// JavaScript Ecosystem - SweetAlert2</see> by Felipe Gavilán.
    /// </remarks>
    public enum SwAlIconType
    {
        /// Warning!!! Common convention for naming enums is with initial
        /// upper case but SweetAlert functions expect lower case strings
        /// passed as arguments for the icon type in their functions. 
        success, 
        error,
        warning,
        info,
        question
    }
}



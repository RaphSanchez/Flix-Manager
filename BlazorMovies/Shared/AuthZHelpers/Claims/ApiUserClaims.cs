using System.Security.Claims;

namespace BlazorMovies.Shared.AuthZHelpers.Claims
{
    /// <summary>
    /// Defines custom authorization claims for controlling access to the
    /// Application/Server-Api/Controllers Users controller
    /// resources and Application/Client Razor components designed for the
    /// UserManager UI.
    /// </summary>
    public static class ApiUserClaims
    {
        /// <summary>
        /// Represents the Type and Value for an authorization claim that
        /// identifies the bearer as an application User creator.
        /// </summary>
        public static ApiUserClaim ApiUserCreator = 
            new("user.creator", "creator");

        /// <summary>
        /// Represents the Type and Value for an authorization claim that
        /// identifies the bearer as an application User reader.
        /// </summary>
        public static ApiUserClaim ApiUserReader =
            new("user.reader", "reader");

        /// <summary>
        /// Represents the Type and Value for an authorization claim that
        /// identifies the bearer as an application User editor.
        /// </summary>
        public static ApiUserClaim ApiUserEditor = 
            new("user.editor", "editor");
        
        /// <summary>
        /// Represents the Type and Value for an authorization claim that
        /// identifies the bearer as an application User cleaner.
        /// </summary>
        public static ApiUserClaim ApiUserCleaner = 
            new("user.cleaner", "cleaner");

        /// <summary>
        /// A static property that returns a collection of all the
        /// authorization claims available for controlling access to the Users
        /// controller resources and Application/Client Razor components
        /// designed for the UserManager UI.
        /// </summary>
        public static List<Claim> AllClaims = new List<Claim>()
        {
            /// Claim name-type: "user.creator".
            /// Claim value: "creator".
            /// Token Issuer: Application/Server-Api IdentityServer engine.
            /// Recipient: ClaimsPrincipal requesting access token.
            /// Naming convention is:
            /// "resource.operation.constraint".
            new Claim(ApiUserCreator.ClaimType, 
                ApiUserCreator.ClaimValues.First()),

            new Claim(ApiUserReader.ClaimType,
                ApiUserReader.ClaimValues.First()),

            new Claim(ApiUserEditor.ClaimType,
                ApiUserEditor.ClaimValues.First()),

            new Claim(ApiUserCleaner.ClaimType, 
                ApiUserCleaner.ClaimValues.First())
        };
    }

    /// <summary>
    /// Represents the <see cref="ClaimType"/> and <see cref="ClaimValue"/> of
    /// a custom UserApi authorization claim. It derives from
    /// <see cref="AuthZClaim"/>.
    /// </summary>
    public class ApiUserClaim : AuthZClaim
    {
        public ApiUserClaim(string claimType, 
            params string[] claimValues) 
                : base(claimType, claimValues)
        {
        }
    }
}



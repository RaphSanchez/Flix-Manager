using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;

using BlazorMovies.Shared.AuthZHelpers.Claims;

namespace BlazorMovies.Shared.AuthZHelpers
{
    /// <summary>
    /// Wraps all the custom authorization claim types and provides collections
    /// of custom authorization claims. E.g., <see cref="ApiContentClaims"/> or
    /// <see cref="ApiUserClaims"/>.
    /// </summary>
    public static class AuthZClaims
    {
        /// <summary>
        /// Private backing field for <see cref="AllAuthZClaims"/> property.
        /// </summary>
        private static readonly List<Claim> AllAuthZClaimsBackingField = new();

        /// <summary>
        /// A static field that returns a collection of all the
        /// authorization claims available for controlling access to
        /// Application/Server-Api content resources (e.g., GenresController,
        /// PeopleController, and MoviesController) and/or
        /// Application/Server-Api/Areas/Identity resources.
        /// </summary>
        public static List<Claim> AllApiContentClaims = new()
        {
            /// Claim name-type: "content.creator".
            /// Claim value: "creator".
            /// Token Issuer: Application/Server-Api IdentityServer engine.
            /// Recipient: ClaimsPrincipal requesting access token.
            /// Naming convention is:
            /// "resource.operation.constraint".
            new Claim(ApiContentClaims.ApiContentCreator.ClaimType,
                ApiContentClaims.ApiContentCreator.ClaimValues.First()),

            new Claim(ApiUserClaims.ApiUserReader.ClaimType,
                ApiUserClaims.ApiUserReader.ClaimValues.First()),

            new Claim(ApiContentClaims.ApiContentEditor.ClaimType,
                ApiContentClaims.ApiContentEditor.ClaimValues.First()),

            new Claim(ApiContentClaims.ApiContentCleaner.ClaimType,
                ApiContentClaims.ApiContentCleaner.ClaimValues.First())
        };

        /// <summary>
        /// A static field that returns a collection of all the
        /// authorization claims available for controlling access to
        /// Application/Server-Api user resources (e.g., UsersController).
        /// </summary>
        public static List<Claim> AllApiUserClaims = new()
        {
            /// Claim name-type: "user.creator".
            /// Claim value: "creator".
            /// Token Issuer: Application/Server-Api IdentityServer engine.
            /// Recipient: ClaimsPrincipal requesting access token.
            /// Naming convention is:
            /// "resource.operation.constraint".
            new Claim(ApiUserClaims.ApiUserCreator.ClaimType,
                ApiUserClaims.ApiUserCreator.ClaimValues.First()),

            new Claim(ApiUserClaims.ApiUserEditor.ClaimType,
                ApiUserClaims.ApiUserEditor.ClaimValues.First()),

            new Claim(ApiUserClaims.ApiUserCleaner.ClaimType,
                ApiUserClaims.ApiUserCleaner.ClaimValues.First())
        };

        /// <summary>
        /// A static property that takes all custom authorization claim types
        /// registered (e.g. <see cref="ApiContentClaims"/>,
        /// <see cref="ApiUserClaims"/>) and returns them in a collection of
        /// type <see cref="System.Security.Claims.Claim"/>. 
        /// </summary>
        public static List<Claim> AllAuthZClaims
        {
            get
            {
                /// Clears collection to avoid duplicates when user refreshes
                /// or reloads a routable component that directly or indirectly
                /// consumes this property; e.g.,
                /// Application/Client/Pages/Users UserEdit routable component.
                AllAuthZClaimsBackingField.Clear();

                AllAuthZClaimsBackingField.AddRange(AllApiContentClaims);

                AllAuthZClaimsBackingField.AddRange(AllApiUserClaims);

                return AllAuthZClaimsBackingField;
            }
        }
    }

    /// <summary>
    /// Base class that represents the <see cref="ClaimType"/> and
    /// <see cref="ClaimValues"/> of a custom authorization claim; e.g.,
    /// <see cref="ApiContentClaim"/> or <see cref="ApiUserClaim"/>.
    /// </summary>
    public class AuthZClaim
    {
        /// <summary>
        /// The Claim name-type.
        /// </summary>
        public string ClaimType { get; private set; }

        /// <summary>
        /// The Claim values.
        /// </summary>
        public string[] ClaimValues { get; private set; }

        public AuthZClaim(string claimType, params string[] claimValues)
        {
            ClaimType = claimType;
            ClaimValues = claimValues;
        }
    }
}


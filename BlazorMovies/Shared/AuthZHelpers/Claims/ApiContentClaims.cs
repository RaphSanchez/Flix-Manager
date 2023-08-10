using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;


namespace BlazorMovies.Shared.AuthZHelpers.Claims
{
    /// <summary>
    /// Defines custom authorization claims for controlling access to the
    /// Application/Server-Api/Controllers content resources (e.g.,
    /// GenresController, PeopleController, MoviesController),
    /// Application/Server-Api/Areas/Identity resources, and/or
    /// Application/Client Razor components.
    /// </summary>
    public class ApiContentClaims
    {
        /// Represents the Type and Value for an authorization claim that
        /// identifies the bearer as an application content creator.
        public static ApiContentClaim ApiContentCreator =
            new("content.creator", "creator");

        /// <summary>
        /// Represents the Type and Value for an authorization claim that
        /// identifies the bearer as an application content editor.
        /// </summary>
        public static ApiContentClaim ApiContentEditor =
            new("content.editor", "editor");

        /// <summary>
        /// Represents the Type and Value for an authorization claim that
        /// identifies the bearer as an application content cleaner.
        /// </summary>
        public static ApiContentClaim ApiContentCleaner =
            new("content.cleaner", "cleaner");
    }

    /// <summary>
    /// Represents the <see cref="ClaimType"/> and <see cref="ClaimValue"/> of
    /// a custom ContentApi authorization claim. It derives from
    /// <see cref="AuthZClaim"/>.
    /// </summary>
    public class ApiContentClaim : AuthZClaim
    {
        public ApiContentClaim(string claimType,
            params string[] claimValues)
            : base(claimType, claimValues)
        {

        }
    }
}



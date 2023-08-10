using System.Security.Claims;
using BlazorMovies.Shared.EDM;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Identity;

namespace BlazorMovies.Server.Helpers
{
    /// <summary>
    /// Your <see cref="IdentityProfileService"/> configures IdentityServer to
    /// include profile information (e.g., the database claims) associated with
    /// the ClaimsPrincipal (current User) into the Id token and the access
    /// token.
    /// <para>
    /// For more info visit:
    /// <see href="https://docs.duendesoftware.com/identityserver/v6/fundamentals/claims/">
    /// Claims</see>,
    /// <see href="https://github.com/DuendeSoftware/Samples/blob/main/IdentityServer/v6/UserInteraction/ProfileService/IdentityServer/CustomProfileService.cs">
    /// DuendeSoftware Samples
    /// </see>, MSDN documentation on
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0">
    /// Profile Service</see> and
    /// <see href="https://www.udemy.com/share/102l0i3@XKuS_4L2sx5qiRNAPGo49UF2Wf2N9DZrFJbOsXe5NlVkqkuRBDwJylshNjBFE-pA/">
    /// Episode 101. Adding Claims in IdentityServer 4
    /// </see> of Udemy course "Programming in Blazor - ASP.Net Core 5" by
    /// Felipe Gavilan.
    /// </para>
    /// </summary>
    /// <remarks>
    /// The parent class <see cref="IProfileService"/> allows IdentityServer to
    /// connect to the User and Profile store (database). 
    /// </remarks>
    public class IdentityProfileService : IProfileService
    {
        /// <summary>
        /// Provides the APIs for managing User in a persistence store.
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Abstraction for a factory to create a <see cref="ClaimsPrincipal"/>
        /// from a User. 
        /// </summary>
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;

        public IdentityProfileService(
            UserManager<ApplicationUser> userManager,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
        {
            _userManager = userManager;
            _claimsFactory = claimsFactory;
        }

        /// <summary>
        /// Creates a reference to a given User in the data store, retrieves
        /// the collection of claims that have been assigned to the that User,
        /// creates a <see cref="ClaimsPrincipal"/> and adds the claims
        /// retrieved from the database. Finally, it sets the issued claims
        /// (includes) in the tokens of the <see cref="ClaimsPrincipal"/>
        /// (current User).
        /// </summary>
        /// <remarks>
        /// This method is called whenever claims about the User are requested;
        /// e.g., during token creation or via the userinfo endpoint. 
        /// </remarks>
        /// <param name="context">Describes the profile data request such as
        /// the subject, the client, the caller, and the requested claim  types.
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            /// <remarks>
            /// Profile Service implementation obtained from Udemy course
            /// "Programming in Blazor - ASP.Net Core 5" by Felipe Gavilan
            /// Episode: 101. Adding Claims in Identity Server 4.
            /// </remarks>

            /// Gets the subject ("sub") identifier.
            string userId = context.Subject.GetSubjectId();

            /// If it exists, it retrieves the User from the data store with
            /// the specified userId.
            ApplicationUser dbUser = await _userManager.FindByIdAsync(userId);

            if (dbUser == null)
            {
                throw new ArgumentNullException(paramName: $"{userId}", 
                    message: $"User with Id: {userId} cannot be found");
            }

            /// Retrieves the collection of claims that belong to the specified
            /// database User.
            IList<Claim> claimsDb = await _userManager.GetClaimsAsync(dbUser);

            /// Creates a ClaimsPrincipal using the User retrieved from the
            /// database.
            ClaimsPrincipal claimsPrincipal =
                await _claimsFactory.CreateAsync(dbUser);

            /// Enables access to the public interface of the current
            /// ClaimsPrincipal.Claims collection.
            List<Claim> claims = claimsPrincipal.Claims.ToList();

            /// Adds the ApplicationUser.Claims (database) to the
            /// ClaimsPrincipal.Claims (context).
            claims.AddRange(claimsDb);

            /// Sets the IssuedClaims for the ProfileDataRequest context. These
            /// set of claims are included into the Id (AuthN) and/or Access
            /// (AuthZ) tokens of the ClaimsPrincipal (current user). 
            context.IssuedClaims = claims;

            #region Example for retrieving specific JwtClaims by type

            /// Retrieves specific JwtClaimTypes; e.g., Role, Scope, 
            /// Session, Picture, etc. 
            /// MSDN Profile Service:
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#profile-service
            //IEnumerable<Claim> roleClaims = context.Subject.FindAll(JwtClaimTypes.);
            //context.IssuedClaims.AddRange(roleClaims);

            #endregion
        }

        /// <summary>
        /// Checks if the User is still "active", if not, no new tokens will be
        /// created. Even if the User has an active session with IdentityServer.
        /// </summary>
        /// <param name="context">Context that describes the IsActiveCheck.
        /// </param>
        /// <returns>An asynchronous operation.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task IsActiveAsync(IsActiveContext context)
        {
            /// Gets the subject identifier.
            string userId = context.Subject.GetSubjectId();

            /// Retrieves a User, if any, with the specified userId.
            ApplicationUser user = await _userManager.FindByIdAsync(userId);

            /// Sets a value indicating whether the subject is active and can
            /// receive tokens.
            context.IsActive = user != null;

        }
    }
}


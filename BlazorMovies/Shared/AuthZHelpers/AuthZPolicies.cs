using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using BlazorMovies.Shared.AuthZHelpers.Claims;

using Microsoft.AspNetCore.Authorization;

namespace BlazorMovies.Shared.AuthZHelpers
{  
    /// <summary>
    /// Defines authorization policies that can be applied globally, to Blazor
    /// routable components, Razor pages, MVC controllers, and/or controller
    /// actions.
    /// </summary>
    /// <remarks>
    /// An AuthZ policy is registered as part of the AuthZ service and can
    /// have one or more requirements. All requirements must be met for policy
    /// evaluation to succeed and you can create your own custom handler with
    /// a collection of data parameters to evaluate the current principal;
    /// i.e., with multiple custom requirements with custom data parameters.
    /// <para>
    /// AuthZ policies ensure that the Api (or any protected resource such as
    /// a Razor page, a controller, or an action method) will check for the
    /// presence of a claim, with a value that matches the registered value,
    /// in the access token, authorization token, presented by the Client-User
    /// trying to access the resource. This is known as "Claims-based
    /// authorization".
    /// </para>
    /// <para>
    /// When an "identity" is created, it may be assigned one or more "scopes"
    /// and "claims" issued by a trusted party (e.g., Application/Server-Api
    /// IdentityServer engine or an external identity provider like Google). A
    /// claim is a value pair that represents what the subject is, not what it
    /// can do.
    /// </para>
    /// <para>
    /// The access token will contain a given "scope" or "claim" and  its value
    /// only if the Application/Client-User requests it and if the
    /// Application/Server-Api IdentityServer allows the Client/User to have
    /// that claim. 
    /// </para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-6.0">
    /// Claims-based authorization in ASP.Net Core</see>,
    /// <see href="https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/">
    /// Configuring Policy-based Authorization with Blazor</see>, 
    /// <see href="https://stackoverflow.com/questions/61153224/use-authorization-roles-and-policies-in-blazor-webassembly-with-identity">
    /// Use Authorization Roles and Policies in Blazor WebAssembly with
    /// Identity</see>, and
    /// <see href="https://docs.duendesoftware.com/identityserver/v6/apis/aspnetcore/authorization/">
    /// Authorization based on Scopes and other Claims</see>.
    /// </remarks>
    public static class AuthZPolicies
    {
        #region ApiContent properties

        /// <summary>
        /// Represents the authorization policy name for Api.Create.Content.
        /// This approach avoids having to hard code policy names elsewhere.
        /// </summary>
        /// <remarks>
        /// The authorization policy allows securing the application globally,
        /// Blazor routable components, Razor pages, MVC controllers, and/or
        /// controller actions that perform create operations. E.g., endpoints
        /// decorated with an [HttpPost] verb and/or Application/Client/Pages
        /// routable components that allow the User to perform create
        /// operations.
        /// <para>
        /// Naming convention is: "resource.operation.constraint".
        /// </para>
        /// </remarks>
        public const string ApiCreateContent = "Api.Create.Content";

        /// <summary>
        /// Represents the authorization policy name for Api.Edit.Content.
        /// </summary>
        /// <remarks>
        /// The authorization policy allows securing the application globally,
        /// Blazor routable components, Razor pages, MVC controllers, and/or
        /// controller actions that perform create operations. E.g., endpoints
        /// decorated with an [HttpPut] or [HttpPatch] verb and/or
        /// Application/Client/Pages routable components that allow the User
        /// to perform edit operations.
        /// <para>
        /// Naming convention is: "resource.operation.constraint".
        /// </para>
        /// </remarks>
        public const string ApiEditContent = "Api.Edit.Content";

        /// <summary>
        /// Represents the authorization policy name for Api.Delete.Content.
        /// </summary>
        /// <remarks>
        /// The authorization policy allows securing the application globally,
        /// Blazor routable components, Razor pages, MVC controllers, and/or
        /// controller actions that perform create operations. E.g., endpoints
        /// decorated with an [HttpDelete] verb and/or Application/Client/Pages
        /// routable components that allow the User to perform delete
        /// operations.
        /// <para>
        /// Naming convention is: "resource.operation.constraint".
        /// </para>
        /// </remarks>
        public const string ApiDeleteContent = "Api.Delete.Content";

        #endregion

        #region ApiUser properties

        /// <summary>
        /// Represents the authorization policy name for Api.Create.User.
        /// This approach avoids having to hard code policy names elsewhere.
        /// </summary>
        /// <remarks>
        /// The authorization policy allows securing the application globally,
        /// Blazor routable components, Razor pages, MVC controllers, and/or
        /// controller actions that perform create operations. E.g., endpoints
        /// decorated with an [HttpPost] verb and/or Application/Client/Pages
        /// routable components that allow operations to create new application
        /// Users.
        /// <para>
        /// Naming convention is: "resource.operation.constraint".
        /// </para>
        /// </remarks>
        public const string ApiCreateUser = "Api.Create.User";

        /// <summary>
        /// Represents the authorization policy name for Api.Read.User.
        /// This approach avoids having to hard code policy names elsewhere.
        /// </summary>
        /// <remarks>
        /// The authorization policy allows securing the application globally,
        /// Blazor routable components, Razor pages, MVC controllers, and/or
        /// controller actions that perform read operations. E.g., endpoints
        /// decorated with an [HttpGet] verb and/or Application/Client/Pages
        /// routable components that allow operations to retrieve application
        /// Users.
        /// <para>
        /// Naming convention is: "resource.operation.constraint".
        /// </para>
        /// </remarks>
        public const string ApiReadUser = "Api.Read.User";

        /// <summary>
        /// Represents the authorization policy name for Api.Edit.User.
        /// </summary>
        /// <remarks>
        /// The authorization policy allows securing the application globally,
        /// Blazor routable components, Razor pages, MVC controllers, and/or
        /// controller actions that perform create operations. E.g., endpoints
        /// decorated with an [HttpPut] or [HttpPatch] verb and/or
        /// Application/Client/Pages routable components that allow operations
        /// to edit application Users.
        /// <para>
        /// Naming convention is: "resource.operation.constraint".
        /// </para>
        /// </remarks>
        public const string ApiEditUser = "Api.Edit.User";

        /// <summary>
        /// Represents the authorization policy name for Api.Delete.User.
        /// </summary>
        /// <remarks>
        /// The authorization policy allows securing the application globally,
        /// Blazor routable components, Razor pages, MVC controllers, and/or
        /// controller actions that perform create operations. E.g., endpoints
        /// decorated with an [HttpDelete] verb and/or Application/Client/Pages
        /// routable components that allow operations to delete application
        /// Users.
        /// <para>
        /// Naming convention is: "resource.operation.constraint".
        /// </para>
        /// </remarks>
        public const string ApiDeleteUser = "Api.Delete.User";

        #endregion

        #region ApiContent Policy Builders

        /// <summary>
        /// Builds an authorization policy that requires an authenticated User
        /// and the "content.creator" claim with a "creator" value.
        /// </summary>
        /// <returns>An authorization policy with the requirements specified
        /// in this instance.</returns>
        public static AuthorizationPolicy BuildApiCreateContentPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                /// Claim name-type: "content.creator"
                /// Claim value: "creator"
                /// Issuer (token): Application/Server-Api IdentityServer engine.
                /// Recipient: ClaimsPrincipal requesting an access token
                .RequireClaim(ApiContentClaims.ApiContentCreator.ClaimType,
                    ApiContentClaims.ApiContentCreator.ClaimValues.First())
                .Build();
        }

        /// <summary>
        /// Builds an authorization policy that requires an authenticated User
        /// and the "content.editor" claim with an "editor" value.
        /// </summary>
        /// <returns>An authorization policy with the requirements specified
        /// in this instance.</returns>
        public static AuthorizationPolicy BuildApiEditContentPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                /// Claim name-type: "content.editor"
                /// Claim value: "editor"
                /// Issuer (token): Application/Server-Api IdentityServer engine.
                /// Recipient: ClaimsPrincipal requesting an access token.
                .RequireClaim(ApiContentClaims.ApiContentEditor.ClaimType,
                    ApiContentClaims.ApiContentEditor.ClaimValues.First())
                .Build();
        }

        /// <summary>
        /// Builds an authorization policy that requires an authenticated User
        /// and the "content.cleaner" claim with a "cleaner" value.
        /// </summary>
        /// <returns>An authorization policy with the requirements specified
        /// in this instance.</returns>
        public static AuthorizationPolicy BuildApiDeleteContentPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                /// Claim name-type: "webapi.access.delete"
                /// Claim value: "webapi.sweeper"
                /// Token issuer: Application/Server-Api IdentityServer engine.
                /// Recipient: ClaimsPrincipal requesting an access token.
                /// Naming convention is:
                /// "resource.operation.constraint"
                .RequireClaim(ApiContentClaims.ApiContentCleaner.ClaimType,
                    ApiContentClaims.ApiContentCleaner.ClaimValues.First())
                .Build();
        }

        #endregion

        #region ApiUser Policy Builders

        /// <summary>
        /// Builds an authorization policy that requires an authenticated User
        /// and the "user.creator" claim with a "creator" value.
        /// </summary>
        /// <returns>An authorization policy with the requirements specified
        /// in this instance.</returns>
        public static AuthorizationPolicy BuildApiCreateUserPolicy()
        {

            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                /// Claim name-type: "user.creator"
                /// Claim value: "creator"
                /// Issuer (token): Application/Server-Api IdentityServer engine.
                /// Recipient: ClaimsPrincipal requesting an access token
                .RequireClaim(ApiUserClaims.ApiUserCreator.ClaimType, 
                    ApiUserClaims.ApiUserCreator.ClaimValues.First())
                .Build();
        }

        /// <summary>
        /// Builds an authorization policy that requires an authenticated User
        /// and the "user.reader" claim with a "reader" value.
        /// </summary>
        /// <returns>An authorization policy with the requirements specified
        /// in this instance.</returns>
        public static AuthorizationPolicy BuildApiReadUserPolicy()
        {
            
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                /// Claim name-type: "user.reader"
                /// Claim value: "reader"
                /// Issuer (token): Application/Server-Api IdentityServer engine.
                /// Recipient: ClaimsPrincipal requesting an access token
                .RequireClaim(ApiUserClaims.ApiUserReader.ClaimType,
                    ApiUserClaims.ApiUserReader.ClaimValues.First())
                .Build();
        }

        /// <summary>
        /// Builds an authorization policy that requires an authenticated User
        /// and the "user.editor" claim with an "editor" value.
        /// </summary>
        /// <returns>An authorization policy with the requirements specified
        /// in this instance.</returns>
        public static AuthorizationPolicy BuildApiEditUserPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                /// Claim name-type: "user.editor"
                /// Claim value: "editor"
                /// Issuer (token): Application/Server-Api IdentityServer engine.
                /// Recipient: ClaimsPrincipal requesting an access token.
                .RequireClaim(ApiUserClaims.ApiUserEditor.ClaimType, 
                    ApiUserClaims.ApiUserEditor.ClaimValues.First())
                .Build();
        }

        /// <summary>
        /// Builds an authorization policy that requires an authenticated User
        /// and the "user.cleaner" claim with a "cleaner" value.
        /// </summary>
        /// <returns>An authorization policy with the requirements specified
        /// in this instance.</returns>
        public static AuthorizationPolicy BuildApiDeleteUserPolicy()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                /// Claim name-type: "user.cleaner"
                /// Claim value: "cleaner"
                /// Token issuer: Application/Server-Api IdentityServer engine.
                /// Recipient: ClaimsPrincipal requesting an access token.
                /// Naming convention is:
                /// "resource.operation.constraint"
                .RequireClaim(ApiUserClaims.ApiUserCleaner.ClaimType,
                    ApiUserClaims.ApiUserCleaner.ClaimValues.First())
                .Build();
        }

        #endregion

        /// <summary>
        /// Encapsulates the code logic to add the authorization policies
        /// registered in the <see cref="AuthZPolicies"/> class.
        /// </summary>
        /// <param name="options">Provides programmatic configuration used by
        /// the <see cref="IAuthorizationService"/> and
        /// <see cref="IAuthorizationPolicyProvider"/>.</param>
        /// <returns>Programmatic configuration used by the
        /// <see cref="IAuthorizationService"/> and
        /// <see cref="IAuthorizationPolicyProvider"/> with the authorization
        /// policies registered in the <see cref="AuthZPolicies"/> class.
        /// </returns>
        public static AuthorizationOptions AddAuthZPolicies(
            this AuthorizationOptions options)
        {
            /// Any Client or User trying to access a resource (e.g., Blazor
            /// routable component, Razor Page, controller, or action method)
            /// secured with an authorization attribute that includes any of
            /// these policies, must have the "scope" or "claim" within its
            /// id or access token. Otherwise, it won't be granted access to
            /// the resource.

            #region ApiContent AuthZ Policies

            /// Authorization Policy name: "Api.Create.Content".
            /// Used to secure Application/Server-Api/Controllers endpoints
            /// that perform create operations; i.e., decorated with an
            /// [HttpPost] verb and/or Application/Client/Pages routable
            /// components that allow the User to perform create operations.
            options
                .AddPolicy(ApiCreateContent,
                    BuildApiCreateContentPolicy());

            /// Authorization Policy name: "Api.Edit.Content".
            /// Used to secure Application/Server-Api/Controllers endpoints
            /// that perform edit operations; i.e., decorated with an
            /// [HttpPut] or [HttpPatch] verb and/or Application/Client/Pages
            /// routable components that allow the User to perform edit
            /// operations.
            options
                .AddPolicy(ApiEditContent,
                    BuildApiEditContentPolicy());

            /// Authorization Policy name: "Api.Delete.Content".
            /// Used to secure Application/Server-Api/Controllers endpoints
            /// that perform delete operations; i.e., decorated with an
            /// [HttpDelete] verb and/or Application/Client/Pages routable
            /// components that allow the User to perform delete operations.
            options
                .AddPolicy(ApiDeleteContent,
                    BuildApiDeleteContentPolicy());

            #endregion

            #region ApiUser AuthZ Policies

            /// Authorization Policy name: "Api.Create.User".
            /// Used to secure Application/Server-Api/Controllers User
            /// controller endpoints that perform operations to create
            /// application Users and/or assign custom authorization claims;
            /// i.e., decorated with an [HttpPost] verb and/or
            /// Application/Client/Pages routable components that allow
            /// creating new application users and/or assigning custom
            /// authorization claims to an application user.
            options
                .AddPolicy(ApiCreateUser, BuildApiCreateUserPolicy());

            /// Authorization Policy name: "Api.Read.User".
            /// Used to secure Application/Server-Api/Controllers User
            /// controller endpoints that perform operations to read
            /// application Users; i.e., decorated with an [HttpGet] verb
            /// and/or Application/Client/Pages routable components that allow
            /// retrieving application users from the data store.
            options
                .AddPolicy(ApiReadUser,
                    BuildApiReadUserPolicy());

            /// Authorization Policy name: "Api.Edit.User".
            /// Used to secure Application/Server-Api/Controllers User
            /// controller endpoints that perform operations to edit
            /// application Users and/or custom authorization claims; i.e.,
            /// decorated with an [HttpPut] or [HttpPatch] verb and/or
            /// Application/Client/Pages routable components that allow
            /// editing application users and/or their authorization claims.
            options
                .AddPolicy(ApiEditUser, BuildApiEditUserPolicy());

            /// Authorization Policy name: "Api.Delete.User".
            /// Used to secure Application/Server-Api/Controllers User
            /// controller endpoints that perform operations to delete
            /// application User and/or custom authorization claims; i.e.,
            /// decorated with an [HttpDelete] verb and/or
            /// Application/Client/Pages routable components that allow
            /// deleting application users and/or custom authorization
            /// claims.
            options
                .AddPolicy(ApiDeleteUser, BuildApiDeleteUserPolicy());

            #endregion

            return options;
        }
    }
}


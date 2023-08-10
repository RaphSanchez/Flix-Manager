using System.Security.Claims;

using BlazorMovies.Shared.AuthZHelpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.QueryFilterDtos;
using Microsoft.AspNetCore.Identity;

namespace BlazorMovies.Client.ApiServices.IRepositories
{
    /// <summary>
    /// Entry point for the IApiService and IUnitOfWork interfaces because they
    /// expose one IEntityName interface for each data entity type in the
    /// Entity Domain Model (EDM).
    /// </summary>
    /// <remarks>
    /// <see cref="IUsers"/> implements the <see cref="IRepository{TEntity}"/>
    /// interface which has general functionality applicable to all data
    /// entities. It also extends the general functionality with operations
    /// that are specific to the entity type passed to satisfy its type
    /// parameter.
    /// <para>
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in the
    /// result of a query with EF's "Include" extension method. 
    /// </para>
    /// </remarks>
    public interface IUsers : IRepository<ApplicationUser>
    {
        #region Post-Create methods


        #endregion

        #region Get-Read methods

        /// <summary>
        /// Retrieves a sequence of user records in segments (or portions of
        /// data) that adhere to the specifications of the PaginationRequestDto. 
        /// </summary>
        /// <remarks>
        /// The sequence of type <see cref="UserDto"/> represents records of
        /// type <see cref="ApplicationUser"/>.
        /// </remarks>
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page. </param>
        /// <returns>
        /// An object of type PaginatedResponseDto with database records after
        /// paginating the result of the database query. It includes the
        /// description and context of the paginated data (metadata). 
        /// </returns>
        Task<PaginatedResponseDto<IEnumerable<UserDto>>?> GetPaginatedUsersAsync(
            PaginationRequestDto paginationRequestDto);

        /// <summary>
        /// Retrieves a collection of all the custom <see cref="AuthZClaim"/>
        /// items available for controlling access to application resources and
        /// flags the authorization claims currently assigned to the user
        /// passed to satisfy its formal input parameter.
        /// </summary>
        /// <returns>
        /// A collection of all the custom <see cref="AuthZClaim"/>
        /// items available for controlling access to resources. It marks the
        /// authorization claims currently assigned to the user.</returns>
        Task<UserClaimsDto?> GetUserAuthZClaimsAsync(string userId);

        /// <summary>
        /// Queries the data store for a given <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="userId">The <see cref="ApplicationUser"/>.Id of the
        /// user to retrieve from the data store.</param>
        /// <returns>A <see cref="UserDto"/> that represents an
        /// <see cref="ApplicationUser"/> Id and Email.</returns>
        Task<UserDto?> GetUserAsync(string userId);
        #endregion

        #region Put-Update methods

        /// <summary>
        /// Updates the custom authorization claims of a given User.
        /// </summary>
        /// <param name="userClaimsDto">A type that represents an
        /// <see cref="ApplicationUser"/>.Id and a collection of
        /// <see cref="AuthZClaimDto"/> items.</param>
        /// <returns>An ActionResult with a type <see cref="bool"/> that
        /// represents the result of the operation.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<bool> UpdateUserClaimsAsync(
            UserClaimsDto? userClaimsDto);

        #endregion

        #region Delete methods

        #endregion
    }
}


using System.Text;
using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.Helpers.ServiceExtensions;
using BlazorMovies.Shared.AuthZHelpers;
using Microsoft.AspNetCore.Mvc;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OutputCaching;

namespace BlazorMovies.Server.Controllers
{
    /// <summary>
    /// Responsible for responding to Application/Client Http requests made
    /// for data and/or operations related to the
    /// <see cref="ApplicationUser"/> type.
    /// </summary>
    /// <remarks>
    /// The <see cref="ApiControllerAttribute"/> enables model binding on the
    /// controller to automatically bind the data from an Http request to the
    /// corresponding action method's parameter(s).
    /// <para>
    /// The <see cref="Route"/> attribute determines the URI of the resource
    /// at the controller level; e.g.,
    /// https://localhost:7077/api/users
    /// </para>
    /// </remarks>
    [ApiController]
    //[OutputCache(PolicyName = nameof(CachingServices.NoCachePolicy))]
    [Route("api/[Controller]")]
    public class UsersController : ControllerBase
    {
        /// <summary>
        /// Exposes one IEntityName interface for each data entity mapped to
        /// the database. It keeps track of changes made to in-memory objects
        /// during a business transaction and persists those changes to the
        /// database when completed.
        /// </summary>
        private readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Constructor requests object instances to the dependency injection
        /// container and uses local variables to store their reference.
        /// </summary>
        /// <param name="unitOfWork">The unit of work that exposes the
        /// available functionality through the IEntityName interfaces.</param>
        public UsersController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region Post-Create actions


        #endregion

        #region Get-Read actions

        /// <summary>
        /// Handles an Http GET request issued to the controller's route (URI):
        /// "GET /api/users".
        /// </summary>
        /// <param name="paginationRequestDto">The pagination specifications
        /// (which page and how many records) for an Http request for paginated
        /// data.</param>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{T}"/> automatically
        /// serializes the object to JSON format and writes the JSON into the
        /// response body of the response message.
        /// </remarks>
        /// <returns>A type that wraps the object value and a StatusCode that
        /// informs the user the status of the request.</returns>
        [Authorize(Policy = AuthZPolicies.ApiReadUser)]
        [HttpGet]
        public async Task<ActionResult<PaginatedResponseDto<IEnumerable<UserDto>>>>
            GetPaginatedUsersTask(
                [FromQuery] PaginationRequestDto paginationRequestDto)
        {
            try
            {
                /// Retrieves a sequence of database records in segments (or
                /// portions of data) that adheres to the specifications
                /// outlined in the paginationRequestDto.
                ///
                /// It includes the metadata (description and context of the
                /// paginated data). 
                PaginatedResponseDto<IEnumerable<ApplicationUser>> paginatedRecords =
                    await _unitOfWork.Users
                        .GetPaginatedAsync(paginationRequestDto);

                /// Maps ApplicationUser to UserDto items; i.e., it flattens
                /// the response data.
                IEnumerable<UserDto> paginatedUsers = paginatedRecords
                    .ResponseData.Select(appUser =>
                        new UserDto()
                        {
                            Id = appUser.Id,
                            Email = appUser.Email
                        });

                /// Builds the PaginatedResponse with the metadata and the
                /// flattened data. 
                PaginatedResponseDto<IEnumerable<UserDto>> paginatedUsersResponse =
                    new(paginatedUsers, paginatedRecords.PaginationMetadata);

                if (!paginatedUsersResponse.ResponseData.Any())
                {
                    /// Returns a NotFound result that produces a
                    /// StatusCodes.Status404NotFound response and passes a
                    /// custom message which will be consumed in the front-end
                    /// to inform the user.
                    return NotFound("No content was found in the database.");
                }

                /// ActionResult<T> automatically serializes the object value
                /// to JSON format and writes it into the response body of the
                /// response message along with the StatusCodes.Status200OK
                /// response. Status codes tell the caller the status of the
                /// request.
                return Ok(paginatedUsersResponse);
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and send it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Replaces the exception with the StatusCode with information
                /// of what went wrong to inform the caller (client). 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while trying to paginate " +
                    "data. Please try again.");
            }
        }

        /// <summary>
        /// Handles an Http GET request issued to the controller's route
        /// template: "GET /api/users/user-claims".
        /// </summary>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{T}"/> automatically
        /// serializes the object to JSON format and writes the JSON into the
        /// response body of the response message.
        /// </remarks>
        /// <param name="userId">The <see cref="ApplicationUser"/>.Id of the
        /// user to retrieve from the data store.</param>
        /// <returns>A type that wraps the object value and a StatusCode that
        /// informs the user the status of the request; e.g., a collection of
        /// custom authorization claims and a StatusCode 200OK.</returns>
        [Authorize(Policy = AuthZPolicies.ApiReadUser)]
        [HttpGet("user-claims")]
        public async Task<ActionResult<UserClaimsDto>> GetUserAuthZClaimsTask(
            [FromQuery] string userId)
        {
            try
            {
                /// Retrieves a collection of all the custom AuthZClaim items
                /// available for controlling application resources and flags
                /// the ones currently assigned to the user passed to satisfy
                /// its formal input parameter.
                UserClaimsDto? userClaimsDto = await _unitOfWork.Users
                    .GetUserAuthZClaimsAsync(userId);

                /// Returns a NotFound result that produces a
                /// StatusCodes.Status404NotFound response and passes a
                /// custom message which will be consumed in the front-end
                /// to inform the user.
                if (!userClaimsDto!.AuthZClaimDtos.Any())
                    return NotFound("No content was found in the database.");

                /// ActionResult<T> automatically serializes the object value
                /// to JSON format and writes it into the response body of the
                /// response message along with the StatusCodes.Status200OK
                /// response. Status codes tell the caller the status of the
                /// request.
                return Ok(userClaimsDto);
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and send it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Replaces the exception with the StatusCode with information
                /// of what went wrong to inform the caller (client). 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while retrieving data from " +
                    "the database. Please try again.");
            }
        }

        /// <summary>
        /// Handles an Http GET request issued to the controller's route
        /// template: "GET /api/users/get-user".
        /// </summary>
        /// <remarks>
        /// If successful, the <see cref="ActionResult{T}"/> automatically
        /// serializes the object to JSON format and writes the JSON into the
        /// response body of the response message.
        /// </remarks>
        /// <param name="userId">The <see cref="ApplicationUser"/>.Id of the
        /// user to retrieve from the data store.</param>
        /// <returns>An ActionResult that represents the result of the action
        /// method.</returns>
        [Authorize(Policy = AuthZPolicies.ApiReadUser)]
        [HttpGet("get-user")]
        public async Task<ActionResult<UserDto>> GetUserTask(
            [FromQuery] string userId)
        {
            try
            {
                /// GetUserAsync method throws an ArgumentNullException with
                /// custom message if no ApplicationUser matches the search
                /// criteria.
                ///
                /// GetUserAsync throws ArgumentNullException if user is not
                /// found.
                UserDto? userDto = await _unitOfWork.Users.GetUserAsync(userId);
                
                return userDto!;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and send it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Replaces the exception with the StatusCode with information
                /// of what went wrong to inform the caller (client). 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while retrieving data from " +
                    "the database. Please try again.");
            }
        }

        #endregion

        #region Put-Update actions

        /// <summary>
        /// Handles an Http PUT request issued to the controller's route
        /// segment: "POST /api/users/update-claims".
        /// </summary>
        /// <param name="userClaimsDto">Route parameter expects a DTO that
        /// wraps the <see cref="ApplicationUser"/>.Id and a collection of
        /// <see cref="AuthZClaimDto"/> items that represent the
        /// <see cref="AuthZClaims"/> to assign to the
        /// <see cref="ApplicationUser"/>. It is passed to the Server-Api
        /// in the request body with JSON format; i.e., implements "model
        /// binding" using the "request body".
        /// <para>
        /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-6.0">
        /// Model Binding in ASP.Net Core</see>.
        /// </para></param>
        /// <returns>An ActionResult with a type <see cref="bool"/> that
        /// represents the result of the operation.</returns>
        [Authorize(Policy = AuthZPolicies.ApiEditUser)]
        [HttpPut("update-claims")]
        public async Task<ActionResult<bool>> UpdateUserClaimsTask(
            [FromBody] UserClaimsDto? userClaimsDto)
        {
            try
            {
                /// Uses the Unit of Work to begin tracking the in-memory
                /// operations performed on the data entity encapsulated
                /// by the UserClaimsDto object as argument in the route
                /// parameter (request body) of this action (method).
                ///
                /// UpdateUserClaimsAsync method throws an ArgumentNullException
                /// with custom message if no ApplicationUser in the database
                /// matches. 
                bool result = await _unitOfWork.Users
                    .UpdateUserClaimsAsync(userClaimsDto);

                /// Not required...
                //await _unitOfWork.PersistToDatabaseAsync();

                /// Status code 200OK.
                return Ok(result);
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and send it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Replaces the exception with the StatusCode with information
                /// of what went wrong to inform the caller (client). 
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred while updating the user " +
                    "claims. Please try again.");
            }
        }

        #endregion

        #region Delete actions


        #endregion
    }
}




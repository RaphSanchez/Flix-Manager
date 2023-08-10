using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BlazorMovies.Shared.AuthZHelpers;
using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Shared.EntityDtos
{
    /// <summary>
    /// A type that represents an <see cref="ApplicationUser"/>.Id and a
    /// collection of <see cref="AuthZClaimDto"/> items to serve Http
    /// requests for the current Users stored in the database. 
    /// </summary>
    public class UserClaimsDto
    {
        /// <summary>
        /// The <see cref="ApplicationUser.Id"/>
        /// </summary>
        public string UserId { get; set; } = null!;

        /// <summary>
        /// A collection of <see cref="AuthZClaimDto"/> that the current User
        /// holds. 
        /// </summary>
        public List<AuthZClaimDto> AuthZClaimDtos { get; set; } 

        public UserClaimsDto()
        {
            AuthZClaimDtos = new List<AuthZClaimDto>();
        }

    }
}




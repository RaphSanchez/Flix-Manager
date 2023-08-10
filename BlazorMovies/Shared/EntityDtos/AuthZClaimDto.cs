using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorMovies.Shared.AuthZHelpers;

namespace BlazorMovies.Shared.EntityDtos
{
    /// <summary>
    /// A type that flattens a type <see cref="AuthZClaim"/> and adds a flag to
    /// indicate that the current claim is assigned (or not) to an application
    /// user.
    /// </summary>
    public class AuthZClaimDto
    {
        /// <summary>
        /// The type (name) of the <see cref="AuthZClaim"/> that it represents.
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// The value of the <see cref="AuthZClaim"/> that it represents.
        /// </summary>
        public string Value { get; set; } = null!;

        /// <summary>
        /// Flag to determine if the <see cref="AuthZClaim"/> that it represents
        /// is selected for a given application user. 
        /// </summary>
        public bool IsSelected { get; set; }
    }
}


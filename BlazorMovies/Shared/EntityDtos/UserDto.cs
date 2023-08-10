using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Shared.EntityDtos
{
    /// <summary>
    /// A type that flattens a type <see cref="ApplicationUser"/> to serve Http
    /// requests for the current Users stored in the database. 
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// The Id of the <seealso cref="ApplicationUser"/>.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// The Email of the <see cref="ApplicationUser"/>.
        /// </summary>
        public string Email { get; set; } = null!;
    }
}


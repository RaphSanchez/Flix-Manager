using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorMovies.Shared.EDM
{
    /// <summary>
    /// The <see cref="MovieScore"/> root entity represents a record
    /// in the database table that stores a
    /// <see cref="BlazorMovies.Shared.EDM.Movie"/> score selected by
    /// an <see cref="ApplicationUser"/>.
    /// </summary>
    public class MovieScore
    {
        /// <summary>
        /// Identity property for the MovieScore record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The score selected by the <see cref="ApplicationUser"/>.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// The date that the scoring took place.
        /// </summary>
        public DateTime ScoreDate { get; set; }

        /// <summary>
        /// Foreign key property of the reference object type
        /// <see cref="BlazorMovies.Shared.EDM.Movie"/> that this
        /// <see cref="MovieScore"/> belongs to.
        /// </summary>
        /// <remarks>
        /// The type <see cref="BlazorMovies.Shared.EDM.Movie"/> has a global
        /// query filter because it is marked as an IsAuditable entity. Either
        /// this <see cref="MovieScore"/> gets a similar global query filter
        /// or the MovieId member is optional to avoid unexpected results.
        /// </remarks>
        public int? MovieId { get; set; }

        /// <summary>
        /// Reference navigation property. It represents the reference object
        /// that this <see cref="MovieScore"/> belongs to. 
        /// </summary>
        /// <remarks>
        /// The type <see cref="BlazorMovies.Shared.EDM.Movie"/> has a global
        /// query filter because it is marked as an IsAuditable entity. Either
        /// this <see cref="MovieScore"/> gets a similar global query filter
        /// or the Movie member is optional to avoid unexpected results.
        /// </remarks>
        public Movie? Movie { get; set; }

        /// <summary>
        /// Foreign key property of the reference object type
        /// <see cref="ApplicationUser"/> that this <see cref="MovieScore"/>
        /// belongs to.
        /// </summary>
        public string? UserId { get; set; }
    }
}



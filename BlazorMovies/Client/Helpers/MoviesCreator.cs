using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// This class was left for backwards compatibility because it is consumed by the
    /// 02-Components-Part-One.razor component.
    /// </summary>
    public class MoviesCreator
    {
        public static List<Movie> GenerateMoviesList()
        {
            return new List<Movie>()
            {
                /// HTML semantic meaning for Title property is cast with MarkupString property
                /// where it is consumed (inside the for() loop of 'Movies-Colored Background'
                /// https://www.syncfusion.com/faq/blazor/general/if-the-raw-html-includes-a-property-reference-i-e-counter-is-there-any-way-of-sending-counter-as-a-parameter
                new Movie() {Title = "SpiderMan - Far From Home", ReleaseDate = new DateTime(2019, 7, 2)},
                new Movie() {Title = "Wonder Woman", ReleaseDate = new DateTime(2016, 11, 23)},
                new Movie() {Title = "Inception", ReleaseDate = new DateTime(2010, 7, 16)}
            };
        }
    }
}



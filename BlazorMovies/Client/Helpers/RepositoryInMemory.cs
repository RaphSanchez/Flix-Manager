using System;
using System.Collections.Generic;
using System.Linq;

using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// This class implements the IRepository interface which means it can be used to
    /// serve the service configured in the dependency injection system using the
    /// IRepository interface.
    /// </summary>
    public class RepositoryInMemory : IRepository
    {
        public List<Movie> GetMovies()
        {
            return new List<Movie>()
            {
                new Movie() {
                    Id = 1,
                    Title = "SpiderMan - Far From Home",
                    ReleaseDate = new DateTime(2019, 7, 2),
                    PosterPath = "Images/38-seed-data/spider-man-far.jpg"},
                new Movie() {
                    Id = 2,
                    Title = "Wonder Woman",
                    ReleaseDate = new DateTime(2016, 11, 23),
                    PosterPath = "Images/38-seed-data/wonder-woman.jpg"
                },
                new Movie() {
                    Id = 3,
                    Title = "Inception",
                    ReleaseDate = new DateTime(2010, 7, 16),
                    PosterPath = "Images/38-seed-data/inception.jpg"
                },
                new Movie()
                {
                    Id = 4,
                    Title = "Serendipity",
                    ReleaseDate = new DateTime(2005, 03, 22),
                    PosterPath = "Images/38-seed-data/serendipity.jpg"
                }
            };
        }
    }
}



using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// Abstraction layer between higher and lower level classes and/or components.
    /// Any class that implements this interface can be served as an argument to
    /// the IRepository service configured in the dependency injection system container
    /// of the Main() method on the Program class. 
    /// </summary>
    public interface IRepository
    {
        List<Movie> GetMovies();
    }
}


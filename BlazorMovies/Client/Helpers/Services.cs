using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// Used to demonstrate the singleton lifetime configuration
    /// of a service which creates a single instance of the service (class)
    /// and all components requesting the service receive the same instance.
    /// The service configuration takes place in the Main method of the Program
    /// class that belongs to the Client project.
    /// </summary>
    public class SingletonService
    {
        public int Value { get; set; }
    }

    /// <summary>
    /// Used to demonstrate the transient lifetime configuration
    /// of a service which creates a different instance of the service (class)
    /// and each component requesting the service receives a new instance. The
    /// service configuration takes place in the Main method of the Program
    /// class that belongs to the Client Project. 
    /// </summary>
    /// <remarks>
    /// Only register classes (services) as transient if they do not implement
    /// IDisposable; if they do not need to be disposed deterministically because
    /// a Blazor app lives withing a browser's tab and will not be disposed until
    /// that tab is close. Otherwise your application will leak memory. For more
    /// info refer to https://blazor-university.com/dependency-injection/dependency-lifetimes-and-scopes/transient-dependencies/
    /// </remarks>
    public class TransientService
    {
        public int Value { get; set; }
    }
}



using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebAssembly.Services;

namespace BlazorMovies.Client
{
    public partial class App
    {
        /// <summary>
        /// Allows URI navigation and manipulation.
        /// </summary>
        [Inject] private NavigationManager? _navManager { get; set; }
        
        /// <summary>
        /// Permits loading assemblies (.dll files) at runtime.
        /// </summary>
        [Inject] private LazyAssemblyLoader? _assemblyLoader { get; set; }

        /// <summary>
        /// Redirects the user to the application's Index from the <NotFound>
        /// rendered component.
        /// </summary>
        private void NavigateToIndex()
        {
            /// Must be positioned before navigating to a different URI or it
            /// will not capture the invalid URL.
            Console.WriteLine($"Invalid URI: ({_navManager?.Uri})");

            /// Navigates to Index. Its forceload parameter is set to false by
            /// default. It is defined here for illustrative purposes.
            _navManager?.NavigateTo("/", false);
        }

        /// <summary>
        /// Contains any additional assemblies; e.g., Razor Class Libraries with routable
        /// components to be passed to the AdditionalAssemblies parameter of the Router
        /// component to consider when searching for any additional routes to add to its
        /// collection. It uses System.Reflection to extract the Assembly where the routable
        /// component resides.
        /// </summary>
        /// <remarks>
        /// The TestComponent resides in the "Weather" assembly and the assembly was later
        /// configured for lazy loading. This means that it does not load during app startup
        /// and will cause an exception because the assembly is still null. Instead, it
        /// is configured inside the OnNavigateAsync() handler for the OnNavigateAsync
        /// EventCallbacck parameter of the Router component to load when the user navigates
        /// to any of its routable components; i.e., when the user navigates to FetchData or
        /// to TestComponent.
        /// </remarks>
        //private Assembly[] _additionalAssemblies = new[]
        //{
        //    (typeof(TestComponent).Assembly),
        //};

        /// <summary>
        /// Collection of assemblies to load when the currently required
        /// route matches any of the registered routes in each conditional
        /// block.
        /// </summary>
        private readonly List<Assembly> _lazyLoadedAssemblies = new();

        /// <summary>
        /// Event handler for the OnNavigateAsync event callback parameter of the
        /// Router component. Responsible for loading the correct assemblies for
        /// endpoints that a user requests.
        /// </summary>
        /// <param name="context">Captures the currently required route.</param>
        private async Task OnNavigateAsync(NavigationContext context)
        {
            try
            {
                IEnumerable<Assembly>? assemblies = null;
                if (context.Path is "fetchdata" or "test-component")
                {
                    assemblies = await _assemblyLoader?.LoadAssembliesAsync(
                        new[] { "Weather.dll" })!;

                    _lazyLoadedAssemblies.AddRange(assemblies);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        //private List<Assembly> _lazyLoadedAssemblies = new();
        //private async Task OnNavigateAsync(NavigationContext context)
        //{
        //    try
        //    {
        //        IEnumerable<Assembly> assemblies = null;
        //        switch (context.Path)
        //        {
        //            case "fetchdata":
        //                assemblies = await _assemblyLoader.LoadAssembliesAsync(
        //                    new[] { "MathNet.Numerics.dll" });
        //                break;
        //        }

        //        if (assemblies != null)
        //        {
        //            _lazyLoadedAssemblies.AddRange(assemblies);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Error: {ex.Message}");
        //    }
        //}
    }
}

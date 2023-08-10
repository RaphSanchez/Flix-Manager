using System.Timers;

using BlazorMovies.Client.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Shared
{
    public partial class DataBindCounter : IAsyncDisposable
    {
        private int _currentCount = 0;
        private static int _currentCountStatic = 0;

        /// <summary>
        /// Stores a reference to the module's external JS file. This JS object
        /// (or file) contains the JS module with the function(s) of interest; i.e.,
        /// the IJSObjectReference is the data type in Blazor that represents a
        /// JS module.
        /// </summary>
        private IJSObjectReference? module;

        /// <summary>
        /// Injecting the IJSRuntime service with a property is required because
        /// we are in back-end code. If we were in UI Code (.razor file as opposed
        /// to a .cs file) we could use an @inject IJSRuntime Blazor directive.
        /// </summary>
        [Inject] private IJSRuntime? _js { get; set; }

        /// <summary>
        /// Used to demonstrate the the Blazor framework is calling for
        /// unmanaged resource disposal when the component is removed
        /// from the UI (when the user navigates to a different routable
        /// component).
        /// </summary>
        private System.Timers.Timer? _timer;

        /// <summary>
        /// Stores a value each time the Timer.Interval is exhausted. This
        /// value is printed into the web browser's console. 
        /// </summary>
        private int _timerCounter = 0;

        private async Task IncrementCountAsync()
        {
            _currentCount++;

            /// SingletonService custom service property value
            _mySingletonService.Value++;

            /// TransientService custom service property value
            MyTransientService!.Value++;

            /// Instance of an IJSObjectReference, that stores a reference
            /// to the module's external JS file, used to invoke the specified
            /// JS function. The "my_function" JS has a formal input parameter
            /// which is satisfied with the second argument.
            if (module != null)
            {
                await module.InvokeVoidAsync("my_function", ".Net created message!");

                /// Instance of an IJSObjectReference, that stores a reference
                /// to the module's external JS file, used to invoke the specified
                /// JS function. The "my_other_function" JS has a formal input parameter
                /// which is satisfied with the second argument. 
                await module.InvokeVoidAsync("my_other_function", "Other .Net message!");
            }

            _timer = new System.Timers.Timer { Enabled = true, Interval = 1000};
            _timer.Elapsed += (o, e) => Console.WriteLine($"Timer Counter: " +
                                                          $"{_timerCounter++}");
            _timer.Start();
        }

        /// <summary>
        /// Dependency injection of a service using an explicit property.
        /// </summary>
        [Inject]
        private TransientService? MyTransientService { get; set; }

        /// This service is injected in the DataBindCounter.razor component
        /// related to this partial class. The DI approach used there is
        /// with an @inject Razor directive and it was left like that to
        /// illustrate both approaches. However, you should try to adhere
        /// to only one approach whenever possible.
        //[Inject]
        //private SingletonService MySingletonService { get; set; }

        /// Method to demonstrate how to invoke static CSharp methods from JS
        [JSInvokable]
        public static Task<int> GetCurrentCountStaticAsync()
        {
            /// returning Task.FromResult() is much better than using
            /// Task.Run() if you are not performing any calculations.
            /// DO NOT use Task.Run in the implementation of the method;
            /// instead, use Task.Run to call the method!
            /// https://blog.stephencleary.com/2013/11/taskrun-etiquette-examples-dont-use.html
            return Task.FromResult(_currentCountStatic);
        }

        #region Importing a JS Module

        public async ValueTask DisposeAsync()
        {
            if (module is not null)
            {
                await module.DisposeAsync().ConfigureAwait(false);
            }
            
            /// DON'T IMPLEMENT both IAsyncDispose and IDispose interfaces
            /// separately or one will not work appropriately.
            _timer?.Dispose();

            GC.SuppressFinalize(this);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            /// We only need to load the .js file (or module) once during
            /// initialization of the component. You could invoke (download) the
            /// JS module at a different time; e.g., inside the IncrementCountAsync()
            /// method so that it does not download unless the user raises the
            /// click event of the button element.
            /// <remarks>
            /// JavaScript (.js) files and other static assets are not generally
            /// cached on clients during development. During production in the
            /// Production Environment, JS files are usually cached by clients.
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/?view=aspnetcore-5.0#cached-javascript-files-1
            /// </remarks>
            if (firstRender)
            {
                /// Uses an instance of the IJSRuntime service (_js) to invoke a
                /// JS object that contains the JS module with JS functions for
                /// this particular Blazor component. By convention, the "import"
                /// identifier is a special identifier used specifically for
                /// importing a JS module. The "import" JS function imports the
                /// JS module specified in the path.
                if (_js != null)
                    module = await _js.InvokeAsync<IJSObjectReference>("import",
                        "./js/DataBindCounter.js");
            }
        }

        #endregion
    }
}


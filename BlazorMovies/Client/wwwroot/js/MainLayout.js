
/* Automatic User Logout. ASP.Net Core Identity: */

/// Monitors current User activity and sets the timer state accordingly.
/// Consumed by the Application/Client/Helpers/IJSRuntimeExtensions
/// InitializedInactivityTimerTask<T>(IJSRuntime, T) method overload.
export function initializeInactivityTimer(objRef) {
    
    // Timer instance
    var inactivityTimer;

    // onmousemove property with a "resetTimer" handler.
    document.onmousemove = resetTimer;

    // onkeypress property with a "resetTimer" handler.
    document.onkeypress = resetTimer;

    // resetTimer handler 
    function resetTimer() {
        // Clears a timer set with the "setTimeout()" method.
        clearTimeout(inactivityTimer);

        // Initializes the timer instance to execute a piece of code after 
        // a certain amount of time has passed. It will execute the "logout" 
        // function declared below which in turn invokes a .Net instance 
        // method named "Logout". The value is in milliseconds. 
        inactivityTimer = setTimeout(logout, 600000);
    }

    // Invokes a .Net instance method to logout current User.
    // Method is declared in the Application/Client/Shared
    // MainLayout.razor component. 
    function logout() {
        objRef.invokeMethodAsync("LogoutTask");
    }
}



/* Custom JS function concatenates a local string with a
   string message passed when consumed from CSharp code. 
    It is consumed from: 
    02-ComponentsPartThree routable component. */
function my_function(message) {
    window.alert("External custom JS function: " + message);
}



/* Custom JS function invokes a .Net static invokable method
    and captures its return type value to pass it as an argument
    to a window.alert() JS function. The .Net method has no
    formal input parameters. The JS function is consumed from: 
    DotNetFromJSDemo component. */
function dotNetStaticInvocation() {
    DotNet.invokeMethodAsync("BlazorMovies.Client", "ReturnIntArrayStaticAsync")
        .then(result => {
            window.alert(".Net Static Method Return Value: " + result);
        });
}

/* Custom JS function invokes a .Net instance invokable method
    and captures its return value to pass it as an argument
    to a window.alert() JS function. The .Net method has no
    formal input parameters. The JS function is consumed from:
    DotNetFromJSDemo component. */
function dotNetInstanceInvocation(objRef) {
    objRef.invokeMethodAsync("ReturnIntArrayInstanceAsync")
        .then(result => {
            window.alert(".Net Instance Method Return Value: " + result);
        });

    //objRef.dispose();
}

/* Custom JS function invokes a .Net instance invokable method
    and captures its return type value. It sends back (return) 
    the captured value to the method that is invoking the JS 
    function. The .Net method has no formal input parameters. The
    JS function is consumed from: DotNetFromJSDemo component. */
function dotNetInstanceInvocationReturn(objRef) {
    return objRef.invokeMethodAsync("ReturnIntArrayInstanceAsync");
}

/* Custom JS function has an extra "inputElementItems" parameter
    that is satisfied from .Net code by passing the string data
    supplied, by the user, to an HTML input element. The JS-invokable
    .Net instance method also uses the string data, it extracts any
    number items and returns an int[]. The second window.alert
    function displays only the numbers extracted from the string
    data. The JS function is consumed from: DotNetFromJSDemo component. */
function dotNetInstanceInvocationWithParameter(objRef, inputElementItems) {
    objRef.invokeMethodAsync("ReturnIntArrayInstanceParameterAsync")
        .then(result => {
            // Consumes the original input data passed as argument 
            // when invoking the dotNetInstanceInvocationWithParameter
            // JS function. 
            window.alert("Number of items in the array: " +
                inputElementItems.length);

            // Consumes the result of the JS-invokable instance method
            window.alert("Number of items successfully processed to type int: " +
                result.length);
        });

    //objRef.Dispose();
}

/* Automatic User Logout. ASP.Net Core Identity: */

// Monitors current User activity and sets the timer state accordingly.
// For illustrative purposes only because the Application/Client/Shared
// MainLayout component implements JS Isolation.
function initializeInactivityTimer(objRef) {

    // Timer instance
    var inactivityTimer;

    // onmousemove event with a "resetTimer" handler.
    document.onmousemove = resetTimer;

    // onkeypress event with a "resetTimer" handler.
    document.onkeypress = resetTimer;

    // resetTimer handler 
    function resetTimer() {
        // Clears a timer set with the "setTimeout()" method.
        clearTimeout(inactivityTimer);

        // Initializes the timer instance to execute a piece of code after 
        // a certain amount of time has passed. It will execute the "logout" 
        // function declared below which in turn invokes a .Net instance 
        // method named "Logout".
        inactivityTimer = setTimeout(logout, 3000);
    }

    // Invokes a .Net instance method to logout current User.
    // Method is declared in the Application/Client/Shared
    // MainLayout.razor component. 
    function logout() {
        objRef.invokeMethodAsync("LogoutTask");
    }
}




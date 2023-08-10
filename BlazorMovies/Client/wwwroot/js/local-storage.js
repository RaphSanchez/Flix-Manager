/* JS Isolation file with modules to export JS function to .Net where the
 * CultureSelector component and the LocalizationService use them. 
 * ***************************************************************************/

/// Window.localStorage
/// https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage
/// https://developer.mozilla.org/en-US/docs/Web/API/Web_Storage_API/Using_the_Web_Storage_API
///
/// Episode "138. Manually Changing the App's Language" of Udemy course 
/// "Programming in Blazor - ASP.Net Core 5" by Felipe Gavilán
/// https://www.udemy.com/share/102l0i3@DhvnVDpIfM5CidiZG_MZKhBe6brf9r3Ilb6P5v2y9nbI61xsVlv3eSEfINBYL2OV/
/// 
/// Dynamically set the culture by user preference
/// https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-6.0&pivots=webassembly#dynamically-set-the-culture-by-user-preference
///
/// LocalStorage.js module implements JS Isolation to get/set the current user's
/// culture preference for localization purposes. 


// Sets a storage object for the current Document's origin local space that 
// is saved across browser sessions. 
export function setInLocalStorage(key, value) {
    localStorage[key] = value;
}

// Gets a storage object from the current Document's original local space
// that is saved across browser sessions.
export function getFromLocalStorage(key) {
    return localStorage[key];
}
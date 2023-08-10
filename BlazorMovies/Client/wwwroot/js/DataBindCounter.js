
/* Custom JS function concatenates a local string with a
   string message passed when consumed from CSharp code. 
   The export statement allows you to export this feature
   (or JS function) of the current module. It is consumed
   from: DataBindCounter.razor component. 
*/
export function my_function(message) {
    window.alert(`JS module: ${message}`);
}


/*  Another custom JS function to demonstrate that a single
    JS module can host multiple JS functions that can be
    handpicked to be exported into .Net.
*/
export function my_other_function(message) {
    window.alert(`Other JS function same module: ${message}`);
}



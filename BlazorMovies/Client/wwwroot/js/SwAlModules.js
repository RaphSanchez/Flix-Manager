
/// Contains the JS modules required to invoke SweetAlert functions
/// to display dialog boxes that provide and receive feedback
/// related to a given action.

/// Creates a confirmation modal with title, message, and icon type.
/// It uses a JS Promise that represents the eventual completion (or
/// failure) of an asynchronous operation and its returning value. It
/// allows to associate handlers with an asynchronous action's
/// eventual success value or failure reason.
export function SwAlConfirm(title, message, iconType) {
    return new Promise((resolve) => {
        Swal.fire({
            title: title,
            text: message,
            icon: iconType,
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Confirm Operation'
        }).then((result) => {  // executed after the user clicks a button.
            if (result.value) { // means user clicked on confirm.
                resolve(true); // return true.
            } else {
                resolve(false); // return false. 
            }
        })
    });
}


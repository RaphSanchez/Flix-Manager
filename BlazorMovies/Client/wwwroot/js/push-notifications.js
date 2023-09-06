
/* JS functions to get the notification permission status, subscribe, or
 * unsubscribe the current user from the user agent's push service to receive
 * web push notifications.
 * 
 * Episode 155. Push API - Frontend of Udemy course Programando en Blazor
 * - ASP.Net Core 7 by Felipe Gavilán:
 * https://www.udemy.com/share/101ZK23@SISffxUyMuU19GXVJv5hXV6O951dBnUmqoh18WH91Pbb3jmO9hNJYr7K2pc1Mt-P/
 * Re-engage users with badges, notifications, and push messages:
 * https://learn.microsoft.com/en-us/microsoft-edge/progressive-web-apps-chromium/how-to/notifications-badges
 * Episode 30. Send and receive push messages - Progressive Web App Training
 * from YouTube course Progressive Web App Training by Google Chrome Developers
 * https://youtu.be/N9zpRvFRmj8
 * ***************************************************************************/

// Uses the Notification API interface to determine the status of the 
// permission, if any, granted (or denied) by the end user to receive web push 
// notifications from the web application (Flix Manager). 
async function getStatusNotificationPermission() {
    // Detects whether the current browser supports Push Notifications
    // https://codelabs.developers.google.com/codelabs/push-notifications/#2
    if ('PushManager' in window) {
        /*console.log('Web browser supports web push notifications!');*/

        // Permission status.
        const permissionStatus = Notification.permission;

        // If permission has been previously denied, it aborts any further actions.
        if (permissionStatus === 'denied') {
            return permissionStatus;
        }

        // Verifies if there is a registered service worker that can be used to
        // manage web push notifications sent by the push service of the user agent
        // (a computer program representing a person; e.g., a browser). 
        const worker = await navigator.serviceWorker.getRegistration();

        // Employs the registered service worker to attempt to retrieve a current
        // web push notifications subscription. 
        // https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps/Tutorials/js13kGames/Re-engageable_Notifications_Push#push
        const existingSubscription = await worker.pushManager.getSubscription();

        // If existingSubscription, return 'granted'. Else, return 'default' (end
        // user has not provided a response for the permission; neither granted nor
        // denied).
        if (existingSubscription) {
            return 'granted';
        }
        else { return 'default'; }
    } else {
        console.log('Web browser does NOT support web push notifications!');
        return 'not-supported';
    }
}

// Subscribes the current user to the push service of the user agent (a 
// computer program representing a person; e.g., a browser) and returns the
// subscription details in a string with Base64 format to persist into the
// Application/Shared/EDM PushSubscriptionsDetails database table responsible
// for persisting the required data to target a specific end user to send a
// push notification. 
async function subscribeUserToPushNotifications() {
    // Voluntary Application Server Identity (VAPID) is a way to send and
    // receive website push notifications. The Application/Server-Api 
    // voluntarily identifies itself to the push service using VAPID.
    //
    // Fetches an Http request to get the VAPID public key. 
    // Route template complement obtained from the Application/Server-Api
    // Controllers PushSubscriptions controller.
    // 
    // The 'get-public-key' route segment must be registered in the 
    // getResponseAndUpdateCache function of the service-worker.js files to be
    // excluded from our custom 'dynamic-cache' to ensure that the Http request
    // is always fetched to the server.
    const vapidPublicKeyResponse =
        await fetch('/api/pushsubscriptions/get-public-key');

    // Public key allocated into a local variable.
    //
    // If application is offline, the ".text" method throws a JSException with 
    // a "failed to fetch" error and stops execution of the function. We can 
    // handle it from the PushNotification component by informing the user that
    // an online connection is required to consume the push notification 
    // service. 
    //
    // If the Http request and its .text method are not at the beginning of the
    // subscribeUserToPushNotifications() function and the application is 
    // offline, the function subscribes the user to the push service of the 
    // user agent (web browser) even if the application is offline but the 
    // operation is not registered in the CSharp world because there is no 
    // connection to the web server; i.e., push subscription details for the 
    // specific user are not sent to the Application/Server-Api to be persisted
    // into the database table.
    const vapidPublicKey = await vapidPublicKeyResponse.text();

    // Web browser requests permission to the current user.
    // https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps/Tutorials/js13kGames/Re-engageable_Notifications_Push#request_permission
    var notificationPermission = await Notification.requestPermission();

    // If permission not granted, return null.
    if (notificationPermission !== 'granted') {
        return null;
    }

    // Verifies if there is a registered service worker that can be used to
    // manage web push notifications sent by the push service of the user agent
    // (a computer program representing a person; e.g., a browser). 
    const workerRegistration = await navigator.serviceWorker.getRegistration();

    // Employs the registered service worker to attempt to retrieve a current
    // web push notifications subscription. 
    const existingSubscription =
        await workerRegistration.pushManager.getSubscription();

    // Subscribes the current user to the push notifications service.
    if (!existingSubscription) {
        // Subscribes the current user to the push service of the user agent
        // (a computer program representing a person; e.g., a browser).
        const newSubscription = await workerRegistration.pushManager.subscribe({
            // Assurance that a notification is shown every time a push message
            // is received. This value is required and must be true.
            // https://codelabs.developers.google.com/codelabs/push-notifications#4
            // https://developer.mozilla.org/en-US/docs/Web/Progressive_web_apps/Tutorials/js13kGames/Re-engageable_Notifications_Push#push_example
            userVisibleOnly: true,
            applicationServerKey: vapidPublicKey
        });

        // Returns the extracted values from the new subscription as a string
        // in Base64 format to pass to the CSharp world and persist them as a
        // record in the Application/Shared/EDM PushSubscriptionsDetails
        // database table responsible for persisting the required data to 
        // target a specific end user to send a push notification. 
        return getSubscriptionDetails(newSubscription);
    }
    // Returns the extracted values from an existing notification subscription
    // as a string in Base64 format to pass to the CSharp world for persisting
    // them to the Application/Shared/EDM PushSubscriptionsDetails database
    // table.
    // 
    // It is important to always pass the latest values to persist them as the
    // latest version of the subscription in case the push service of the user
    // agent makes any modifications.
    else {
        return getSubscriptionDetails(existingSubscription);
    }
}

// Unsubscribes the current user from the push service of the user agent 
// (a computer program representing a person; e.g., a browser) and returns the
// subscription details in a string with Base64 format to remove from the
// Application/Shared/EDM PushSubscriptionsDetails database table responsible
// for persisting the required data to target a specific end user to send a
// push notification.
async function unsubscribeUserFromPushNotifications() {

    // Fetches an Http request to get the VAPID public key. 
    // Route template complement obtained from the Application/Server-Api
    // Controllers PushSubscriptions controller.
    //
    // Its unique purpose is to send an Http request to test if a connection
    // to the web server is available. 
    // 
    // The 'get-public-key' route segment is registered in the 
    // getResponseAndUpdateCache function of the service-worker.js files to be
    // excluded from our custom 'dynamic-cache' to ensure that the Http request
    // is always fetched to the server.
    const vapidPublicKeyResponse =
        await fetch('/api/pushsubscriptions/get-public-key');

    // Public key allocated into a local variable.
    //
    // If application is offline, the ".text" method throws a JSException with 
    // a "failed to fetch" error and stops execution of the function. We can 
    // handle it from the PushNotification component by informing the user that
    // an online connection is required to consume the push notification 
    // service. 
    //
    // If the Http request and its .text method are not at the beginning of the
    // unsubscribeUserToPushNotifications() function and the application is 
    // offline, the function unsubscribes the user from the push service of the 
    // user agent (web browser) even if the application is offline but the 
    // operation is not registered in the CSharp world because there is no 
    // connection to the web server; i.e., push subscription details for the 
    // specific user are not sent to the Application/Server-Api to be removed
    // from the database table.
    const vapidPublicKey = await vapidPublicKeyResponse.text();

    // Verifies if there is a registered service worker that can be used to
    // manage web push notifications sent by the push service of the user agent
    // (a computer program representing a person; e.g., a browser). 
    const worker = await navigator.serviceWorker.getRegistration();

    // Employs the registered service worker to attempt to retrieve a current
    // web push notifications subscription. 
    const existingSubscription = await worker.pushManager.getSubscription();

    if (existingSubscription) {

        // Unsubscribes the current user from the push service of the user
        // agent.
        existingSubscription.unsubscribe();

        // Returns the extracted values from an existing notification subscription
        // as a string in Base64 format to pass to the CSharp world for removing
        // them from the Application/Shared/EDM PushSubscriptionsMetadata database
        // table.
        return getSubscriptionDetails(existingSubscription);
    }
}

// Extracts the push subscription values provided by the push service of the
// user agent. These values are returned as a string in Base64 format to be
// mapped to an instance of the Application/Shared/EDM PushSubscriptionDetails
// class which represents a record in the PushSubscriptionsDetails database
// table responsible for persisting the required data to target a specific end 
// user to send a push notification.
//
// Ensure that the property names exactly match the property names of the
// Application/Shared/EDM PushSubscriptionDetails class. Otherwise, JSInterop
// will throw an exception because model binding will fail.
function getSubscriptionDetails(subscription) {
    return {
        pushEndpointUrl: subscription.endpoint,
        p256dh: arrayBufferToBase64(subscription.getKey('p256dh')),
        auth: arrayBufferToBase64(subscription.getKey('auth'))
    }
}

// Converts a byte array buffer into a string in Base64 format.
// https://stackoverflow.com/a/9458996
// https://w3c.github.io/push-api/#push-service-use
function arrayBufferToBase64(buffer) {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }

    return window.btoa(binary);
}


using Firebase.Messaging;
using System.Collections;
using TMPro;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.PushNotifications;
using UnityEngine;
using UnityEngine.UI;

/*
    The Firebase Cloud Message library will be initialized when adding handlers for either the 
    TokenReceived or MessageReceived events.

    Upon initialization, a registration token is requested for the client app instance. The app
    will receive the token with the OnTokenReceived event, which should be cached for later use. 
    You'll need this token if you want to target this specific device for messages.

    In addition, you will need to register for the OnMessageReceived event if you want to be able
    to receive incoming messages.
*/
public class NotificationManager : MonoBehaviour
{
    const string DataKey = "msgString";
    const string RecipientKey = "recipient";
    
    //Public Text variables used to help debugging
    public TMP_Text deviceTokenText;
    public TMP_Text dataText;
    public Button SendNotificationBtn;

    //This is the URL or IP address of your web server.
    [SerializeField] private string webServer = "10.0.0.01";
    [SerializeField] private string topic = "all";

    // Used to store the device token of the device this application is running on.
    private string myDeviceToken; // This token is unique to BOTH the firebase project and the device.

    public async void Start()
    {
        SendNotificationBtn.onClick.AddListener(() => OnButtonSendDelayedMessage());

        // Push Notification Package
        await UnityServices.InitializeAsync();
        // Note: This is the minimum required in Analytics version 3.0.0 and above to ensure the events with the push notification data are sent correctly.
        // In a real game you would need to handle privacy consent states here, see the Analytics documentation for more details.
        await AnalyticsService.Instance.CheckForRequiredConsents();

        try
        {
            string pushToken = await PushNotificationsService.Instance.RegisterForPushNotificationsAsync();
            PushNotificationsService.Instance.OnNotificationReceived += notificationData =>
            { Debug.Log("Received a notification!"); };
        }
        catch
        {
            Debug.Log("Failed to retrieve a push notification token.");
        }


        // FireBase SDK - Adds the appropriate functions to the specified events.
        FirebaseMessaging.TokenReceived += OnTokenReceived;
        FirebaseMessaging.MessageReceived += OnMessageReceived;

#if UNITY_ANDROID
        Screen.fullScreen = false;
#endif
    }
    private void OnDestroy()
    {
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
    }

    //Called when the TokenReceived event occurs - meaning the device has recieved a token from Firebase.
    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        Debug.Log("Received Registration Token: " + myDeviceToken);

        //Sets the myDeviceToken variable to the device's device token
        myDeviceToken = token.Token;

        //Displays the Device's token to the screen.
        //If "No Device Token!" is shown, you have done something wrong with the setup of your Firebase Project.
        deviceTokenText.text = "token: " + myDeviceToken;

        //Subscribes the device to the topic 'all'
        FirebaseMessaging.SubscribeAsync("/topics/"+topic);

        //If using Firebase SDK <5.2.0 use the below fucntion to subscribe to a topic
        //Firebase.Messaging.FirebaseMessaging.Subscribe("/topics/all");
    }

    //Called when the MessageRecieved event occurs - meaning the device has received a message from Firebase.
    public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Received a new message from: " + e.Message.From);

        /* Changes the device token text on screen to the notification's body sent to the device
         * This is to demonstrate how notification components can be used if the application is in the foreground.
         * It is prefered to use the data component for this type of behavior, but I wanted to show that you can do it.
         */
        deviceTokenText.text = "token: " + e.Message.Notification.Title + "\n" +  e.Message.Notification.Body;

        string dataTextStr = "No Data Text!";

        /* This function attempts to update the dataTextStr variable to the value assigned to the key: "dataString"
         * This is the prefered method of sending bits of data to your application as it is not seen by the end user,
         * unless you choose to do so as we do here.
         */
        e.Message.Data.TryGetValue(DataKey, out dataTextStr);
        dataText.text = "msg received: " + dataTextStr;
    }

    //Called when the "Send With Delay" button is pressed on screen.
    public void OnButtonSendDelayedMessage()
    {
        Debug.Log("Sending Message...");
        StartCoroutine(SendDelayedMessage());
    }

    //Coroutine is started after "Send With Delay" button is pressed.
    //This will handle the call to a PHP script on our web server.
    private IEnumerator SendDelayedMessage()
    {
        /* Here we will use a WWWForm to send data from Unity to our web server.
         */
        WWWForm webForm = new WWWForm();

        /* With this WWWForm, we will set the recipient key to the 'all' topic.
         * Using this recipient in out PHP script will send a notification to all devices subscribed to the 'all' topic.
         */

        webForm.AddField(RecipientKey, "/topics/"+topic);

        /* Additionally, the form can be configured to send the notification to a specific device.
         * Uncommenting the line below, and commenting out the line above sends the message to your device only.
         */

        //webForm.AddField(Recipient, myDeviceToken);

        WWW serverCall = new WWW(webServer + "/PushNotificationsProject/SendNotificationWithDelay.php", webForm);

        yield return serverCall;

        if (serverCall.error == null)
        {
            //Message sent successfully. 
            //serverCall.text will contain any text returned from the PHP script using the echo function.
            Debug.Log("Successfully Sent Message With Delay!\nFrom web server: " + serverCall.text);
        }
        else
        {
            //Message NOT sent successfully. 
            //serverCall.error will contain any error messages returned from the PHP script.
            Debug.LogError("Error: Failed to Send Message.\nFrom web server: " + serverCall.error);
        }
    }
}

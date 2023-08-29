Tutorial Link: https://youtube.com/playlist?list=PLgYNYeZLALseW_0VapXEXNx6jpQk4Juiv&si=bU57E9mUU2kp6krh

How to Configure Test Project to Receive Push Notifications:

1. Go to Edit->Project Settings->Player and navigate to the Android tab in the inspector. Copy the package name (or change it to whatever you like then copy it).

2. Import the latest version of the "FirebaseMessaging.unitypackage" from the Firebase Unity SDK.

3. Login to the Firebase Console and create a new project or open an existing one.

4. Create a new Android application using that package name you copied in step 1

5. Download the google-services.json file and put it in the Assets folder of the Unity project.

6. Create a build of the game and install the .apk file on your Android device.

7. When you open the app on your Android device, you should see the device's token printed to the screen in plain text. If it says "No Device Token!" your device is not receiving an FCM device token and likely there is an issue with one of the steps above or Google Play Services is not up to date on the device.

8. Back in the Firebase console, you should now see an active user for your application and can send a test message to your device. Note that you will not receive the notification if the application is open on screen. Close the app on your Android device, send the test message and you should get the notification almost instantly.

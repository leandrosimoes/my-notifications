# My Notifications #

I'm studing SignalR and because of this I made this little WPF lib to show notifications on desktop with feedback from the users.
I put two sample projects in the solution to test two scenarios:

### 1 - Windows Forms Only

One is using with a simple windows form application, so, you can run the `MyNotifications.WinForm` project and click in the buttons to show the different types of notifications and see how it works.

### 2 - An MVC Project Sending Notifications To The Windows Form Application And Receiving The Users Feedback

In this case, you can run the `MyNotifications.MVC` project, than you can open the windows form application from `YOUR_PROJECTS_PATH\MyNotifications\MyNotifications.WinForm\bin\Debug\MyNotifications.WinForm.exe`. After run both projects, just send the notifications from the MVC project and see the magic happens. Then you can answer the notifications from the WinForm project and see the magic happens again.

# How To Use In My WinForms Projects

Just compile the `MyNotifications.Lib` project and reference the `MyNotifications.Lib.dll` generated to your WinForm project. After this, you can do something like:

```csharp
using MyNotifications.Lib;

var notificationController = new NotificationController();

// Here is the sample of a function to handle the users answers
private void MyHadleFunction(object sender, OnCloseNotificationsArgs e) {
    Guid id = e.id; // Notification ID
    string title = e.title; // Notification title
    string message = e.message; // Notification message
    NotificationType type = e.type; // Notification type
    string answer = e.answer; // Notification answer

    // Do whatever you want with this information
}

// Here you set your function to handle when the user answers to the notifications
notificationController.OnCloseNotifications += MyHadleFunction

// Here you see how to show a notification
// id: Unique Guid ID for the notification
// title: String to show in title of the notification
// message: String to show in body of the notification
// type: Enum NotificationType to determine the type of the notification. The enum has this types:
//      NotificationType.Simple -> Shows just the title and message with an OK button. 
//                                 Do not return anything in Answer property
//      NotificationType.YesNo ->  Shows the title, message, YES button and NO button. 
//                                 Return YES or NO in the Answer property
//      NotificationType.Answer -> Shows the title, message, a textbox to the user type the answer and a SEND button. 
//                                 Return the answer that the user typed insinde the textbox in the Answer property.
private void ShowANewNotification(Guid id, string title, string message, NotificationType type) {
    // Finally show the notification
    notificationController.ShowNotification(id, title, message, type);
}
```

### Contribute

Any sugestion or bug report, just open a PR on develop branch or mail me at [leandro.simoes@outlook.com](mailto:leandro.simoes@outlook.com)
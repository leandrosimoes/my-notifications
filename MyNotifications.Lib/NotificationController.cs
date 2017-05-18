using MyNotifications.WPF.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.Integration;

namespace MyNotifications.Lib
{
    public class OnCloseNotificationsArgs
    {
        public Guid Id { get; internal set; }
        public string Title { get; internal set; }
        public string Message { get; internal set; }
        public NotificationType Type { get; internal set; }
        public string Answer { get; internal set; }
    }

    public class NotificationController
    {
        private List<Notification> notifications;
        private Notification notification;

        public event EventHandler<OnCloseNotificationsArgs> OnCloseNotifications;

        public NotificationController()
        {
            if (notifications == null)
            {
                notifications = new List<Notification>();
            }
        }

        private void SetUpNotificationLocation(ref Notification notif)
        {
            var workArea = System.Windows.SystemParameters.WorkArea;
            var qtdNotifications = notifications.Count;

            notif.Left = workArea.Right - notif.Width - 10;
            notif.Top = workArea.Bottom - ((notif.Width - 10) * qtdNotifications + 1);
        }


        public void ShowNotification(Guid id, string title, string message, NotificationType? type = NotificationType.Simple)
        {
            var notification = new Notification(title, message, id, notifications.Count + 1, type);

            var qtdNotifications = notifications.Count;
            var workArea = System.Windows.SystemParameters.WorkArea;

            notification.Left = workArea.Right - notification.Width - 10;
            notification.Top = workArea.Bottom - ((notification.Height + 10) * (qtdNotifications + 1));

            notification.OnClose += OnCloseNotification;

            notifications.Add(notification);

            //https://social.msdn.microsoft.com/Forums/vstudio/en-US/c68d5f3c-c8cc-427d-82e3-6135d075a304/bug-in-nonwpfwpf-mixapplications?forum=wpf
            ElementHost.EnableModelessKeyboardInterop(notification);

            notification.Show();
        }

        private void OnCloseNotification(object sender, OnCloseEventArgs e)
        {
            if (e.notification == null) return;

            if (e.notification.Id != Guid.Empty && e.notification.Index > 0)
            {
                var notificationToClose = notifications.FirstOrDefault(n => n.Id == e.notification.Id);
                notifications.Remove(notificationToClose);

                e.notification.Close();

                var handler = OnCloseNotifications;
                var args = new OnCloseNotificationsArgs
                {
                    Id = e.notification.Id,
                    Title = e.notification.Title,
                    Message = e.notification.Message,
                    Type = e.notification.Type,
                    Answer = e.notification.Type == NotificationType.Answer 
                             ? e.notification.Answer
                             : e.notification.Type == NotificationType.YesNo 
                                    ? e.notification.Yes 
                                        ? "Yes" 
                                        : "No"
                                    : ""
                };
                handler(sender, args);

                var qtdNotifications = notifications.Count;
                var workArea = System.Windows.SystemParameters.WorkArea;
                foreach (var notif in notifications.OrderByDescending(n => n.Index))
                {
                    if (notif.Index > e.notification.Index)
                    {
                        notif.Index--;
                        notif.SlideMeDown(workArea.Bottom - ((notif.Height + 10) * notif.Index), 100 * notif.Index);
                    }
                }
            }
        }
    }
}

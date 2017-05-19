using MyNotifications.WPF.Enums;
using System;

namespace MyNotifications.MVC.Interfaces
{
    public interface INotificationHub
    {
        void newClientOnline(string connectionId);
        void disconnectUser(string connectionId);
        void notificationRead(Guid id, string title, string message, string answer);
        void notificationReceived(Guid id, string title, string message, NotificationType type);
        void sendNotification(string title, string message, Guid user, NotificationType type);
        void notificationSend(Guid id, string title, string message, NotificationType type);
    }
}

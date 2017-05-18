using System;

namespace MyNotifications.MVC.Interfaces
{
    public interface INotificationHub
    {
        void newClientOnline(string connectionId);
        void disconnectUser(string connectionId);
        void notificationRead(Guid id, string title, string message, string answer);
    }
}

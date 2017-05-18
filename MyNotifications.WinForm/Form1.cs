using Microsoft.AspNet.SignalR.Client;
using MyNotifications.Lib;
using MyNotifications.WPF.Enums;
using System;
using System.Windows.Forms;

namespace MyNotifications.WinForm
{
    public partial class Form1 : Form
    {
        private readonly HubConnection _conn;
        private readonly IHubProxy _proxy;
        private readonly NotificationController _controller;

        public Form1()
        {
            InitializeComponent();

            _controller = new NotificationController();

            _controller.OnCloseNotifications += OnCloseNotification;

            try
            {
                _conn = new HubConnection(@"http://localhost:51186/signalr/hubs", "fromDesktop=true");
                _proxy = _conn.CreateHubProxy("NotificationsHub");
                _proxy.On<Guid, string, string, NotificationType>("notificationReceived", (id, title, message, type) => OnMessageReceived(id, title, message, type));
                _conn.Start().Wait();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnCloseNotification(object sender, OnCloseNotificationsArgs e)
        {
            _proxy.Invoke("NotificationRead", e.Id, e.Title, e.Message, e.Answer).Wait();
        }

        private void OnMessageReceived(Guid id, string title, string message, NotificationType type)
        {
            if (InvokeRequired)
            {
                Invoke((Action)(() =>
                {
                    _controller.ShowNotification(id, title, message, type);
                }));
            }
            else
            {
                _controller.ShowNotification(id, title, message, type);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _conn.Stop();
        }

        private void btnSimple_Click(object sender, EventArgs e)
        {
            _controller.ShowNotification(Guid.NewGuid(), "Simple Notification", "Simple message for a simple notification", NotificationType.Simple);
        }

        private void btnYesNo_Click(object sender, EventArgs e)
        {
            _controller.ShowNotification(Guid.NewGuid(), "Yes or No Notification", "Do you like this notifications system?", NotificationType.YesNo);
        }

        private void btnAnswer_Click(object sender, EventArgs e)
        {
            _controller.ShowNotification(Guid.NewGuid(), "Answer Notification", "Tell us more about what do you think about this notifications", NotificationType.Answer);
        }
    }
}

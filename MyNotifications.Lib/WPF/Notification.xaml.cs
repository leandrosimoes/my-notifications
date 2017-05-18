using MyNotifications.WPF.Enums;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MyNotifications.Lib
{
    internal class OnCloseEventArgs : EventArgs
    {
        public Notification notification { get; internal set; }
    }

    /// <summary>
    /// Interaction logic for Notification.xaml
    /// </summary>
    internal partial class Notification : Window
    {
        private System.Windows.Threading.DispatcherTimer _timer;

        public Guid Id { get; }
        public string Title { get; internal set; }
        public string Message { get; internal set; }
        public int Index { get; set; }
        public bool Yes { get; internal set; }
        public string Answer { get; internal set; }
        public NotificationType Type { get; internal set; }

        public Notification(string title, string message, Guid id, int index, NotificationType? type = NotificationType.Simple)
        {
            InitializeComponent();

            lblTitle.Content = title;
            lblMessage.Text = message;
            Id = id;
            Title = title;
            Message = message;
            Index = index;
            Type = type.Value;

            if (type == NotificationType.YesNo)
            {
                btnYes.Visibility = Visibility.Visible;
                btnNo.Visibility = Visibility.Visible;
                btnClose.Visibility = Visibility.Hidden;
            }

            if (type == NotificationType.Answer)
            {
                btnSend.Visibility = Visibility.Visible;
                txtAnswer.Visibility = Visibility.Visible;
                btnClose.Visibility = Visibility.Hidden;
            }
        }

        public event EventHandler<OnCloseEventArgs> OnClose;

        private void SlideOut()
        {
            var da = new DoubleAnimation
            {
                From = Left,
                To = Left + Width + 20,
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };
            da.Completed += SlideOutCompleted;

            this.BeginAnimation(LeftProperty, da);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            SlideOut();
        }

        private void SlideOutCompleted(object sender, EventArgs e)
        {
            var handler = OnClose;
            var args = new OnCloseEventArgs
            {
                notification = this
            };

            handler(sender, args);
        }

        public void SlideMeDown(double top, int delay)
        {
            if (_timer == null)
            {
                _timer = new System.Windows.Threading.DispatcherTimer();
            }

            _timer.Tick += new EventHandler((sender, e) => DoIt(sender, e, top));
            _timer.Interval = TimeSpan.FromMilliseconds(delay > 0 ? delay : 1);
            _timer.Start();
        }

        private void DoIt(object sender, EventArgs e, double top)
        {
            _timer.Stop();

            var da = new DoubleAnimation
            {
                From = Top,
                To = top,
                Duration = new Duration(TimeSpan.FromMilliseconds(300))
            };

            this.BeginAnimation(TopProperty, da);
        }

        private void btnYes_Click(object sender, RoutedEventArgs e)
        {
            Yes = true;

            SlideOut();
        }

        private void btnNo_Click(object sender, RoutedEventArgs e)
        {
            Yes = false;

            SlideOut();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            Yes = false;
            Answer = txtAnswer.Text;

            if (string.IsNullOrEmpty(Answer) || Answer.Contains("TYPE YOUR ANSWER HERE"))
            {
                txtAnswer.Focus();
                return;
            };

            SlideOut();
        }

        private void txtAnswer_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtAnswer.Text.Contains("TYPE YOUR ANSWER HERE"))
            {
                txtAnswer.Text = "";
            }
        }

        private void txtAnswer_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtAnswer.Text))
            {
                txtAnswer.Text = "TYPE YOUR ANSWER HERE";
            }
        }
    }
}

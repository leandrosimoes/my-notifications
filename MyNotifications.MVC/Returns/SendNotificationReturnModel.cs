using MyNotifications.WPF.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyNotifications.MVC.Returns
{
    public class SendNotificationReturnModel
    {
        public bool Success { get
            {
                return string.IsNullOrEmpty(ErrorMessage);
            }
        }
        public string ErrorMessage { get; set; }

        public Guid id { get; set; }
        public Guid idUser { get; set; }
        public string title { get; set; }
        public string message { get; set; }
        public NotificationType type { get; set; }
    }
}

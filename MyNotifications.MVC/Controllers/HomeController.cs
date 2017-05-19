using Microsoft.AspNet.SignalR;
using MyNotifications.MVC.Handlers;
using MyNotifications.MVC.Returns;
using MyNotifications.WPF.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyNotifications.MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext _context;

        public HomeController()
        {
            _context = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendNotification(string title, string message, Guid user, NotificationType type)
        {
            var returnModel = new SendNotificationReturnModel
            {
                id = Guid.NewGuid(),
                idUser = user,
                title = title,
                message = message,
                type = type
            };

            if (UserHandler.ConnectedIds.Contains(returnModel.idUser.ToString()))
            {
                _context.Clients.Client(returnModel.idUser.ToString())
                    .notificationReceived(
                        returnModel.id,
                        returnModel.title,
                        returnModel.message,
                        returnModel.type);
            }
            else
            {
                returnModel.ErrorMessage = "User is not connected anymore";
            }

            return new JsonResult
            {
                Data = returnModel
            };
        }
    }
}
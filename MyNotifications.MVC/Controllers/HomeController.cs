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
        private IHubContext _context;

        // GET: Home
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

            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();

            if (UserHandler.ConnectedIds.Contains(returnModel.idUser.ToString()))
            {
                context.Clients.Client(returnModel.idUser.ToString())
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

        [HttpPost]
        public ActionResult SendChatMessage(string message, Guid user)
        {
            var returnModel = new SendChatMessageReturnModel
            {
                idUser = user,
                message = message,
                isAnswer = false
            };

            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationsHub>();

            if (UserHandler.ConnectedIds.Contains(returnModel.idUser.ToString()))
            {
                context.Clients.Client(returnModel.idUser.ToString())
                    .chatMessageReceived(
                        returnModel.idUser,
                        returnModel.message,
                        returnModel.isAnswer);
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
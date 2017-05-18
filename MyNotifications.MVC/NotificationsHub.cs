using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using MyNotifications.MVC.Handlers;
using MyNotifications.MVC.Interfaces;

namespace MyNotifications.MVC
{
    /// <summary>
    /// Algumas observações a serem passadas:
    ///     
    ///     ### QUANDO USAR METODOS ASINCRONOS:
    ///     Caso o metodo seja um metodo que terá uma espera, como por exemplo, requisição em um banco de dados e etc,
    ///     recomenda-se usar um método asincrono retornando uma Task (para void) ou Task<T> onde T pode ser qualquer tipo de dado.
    ///     Neste caso, no javascript, deve-se trabalhar com promises, desta forma:
    ///         seuProxy.server.suaFuncaoAsincrona().done(function(retorno) { console.log(retorno) });
    ///         
    ///     ### OVERLOADS
    ///     Você pode ter overloads de métodos no Hub, porém diferente do .NET, o SignalR só aceita overloads com QUANTIDADES de parametros
    ///     diferentes, e não somente pela TIPAGEM dos parametros. Ex:
    ///     
    ///         MeuMetodo(int a, int b);
    ///         MeuMetodo(int a, string b); ERRO
    ///         MeuMetodo(int a, int b, string c); CORRETO
    /// </summary>

    //[HubName("UserDisconnect")] <= Pode-se usar este atributo para definir como irá ficar o nome do hub no client
    public class NotificationsHub : Hub<INotificationHub>
    {
        public override Task OnConnected()
        {
            if (Context.QueryString.Get("fromDesktop") != null && Context.QueryString.Get("fromDesktop") == "true")
            {
                UserHandler.ConnectedIds.Add(Context.ConnectionId);
                Clients.All.newClientOnline(Context.ConnectionId);
            }
            else
            {
                foreach (var id in UserHandler.ConnectedIds)    
                {
                    Clients.All.newClientOnline(id);
                }
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            Clients.All.disconnectUser(Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        // [HubMethodName("NotificationRead")] <= Pode-se usar este atributo para definir como irá ficar o nome do método no client, 
        // desta forma, o metodo ficará igual está aqui, e não mudará sozinho para pascal case
        public void NotificationRead(Guid id, string title, string message, string answer)
        {
            Clients.All.notificationRead(id, title, message, answer);
        }
    }
}
using Microsoft.AspNet.SignalR;
using NeshHouse.Stats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Security.Principal;
using System.Security.Claims;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Owin;
using System.Threading.Tasks;
using NeshHouse.Stats.Web.SignalRHelpers;

namespace NeshHouse.Stats.Web
{
    [Authorize]
    public class BeerpongHub : Hub
    {

        public override Task OnConnected()
        {
            var name = Context.User.Identity.Name;

            using (var db = new HubContext())
            {
                var user = db.Users
                    .Include(u => u.Connections)
                    .SingleOrDefault(u => u.UserName == name);

                if (user == null)
                {
                    user = new User
                    {
                        UserName = name,
                        Connections = new List<Connection>()
                    };
                    db.Users.Add(user);
                }

                user.Connections.Add(new Connection
                {
                    ConnectionID = Context.ConnectionId,
                    UserAgent = Context.Request.Headers["User-Agent"],
                    Connected = true
                });
                db.SaveChanges();
            }
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            using (var db = new HubContext())
            {
                var connection = db.Connections.Find(Context.ConnectionId);
                connection.Connected = false;
                db.SaveChanges();
            }
            return base.OnDisconnected(stopCalled);
        }

        public void AddToRoom(string roomName)
        {
            using (var db = new HubContext())
            {
                var user = db.Users.Include(x => x.Connections).SingleOrDefault(x => x.UserName == Context.User.Identity.Name);

                if (user != null)
                {
                    Clients.All.userChanged(user.UserName);
                }

                
            }

            Clients.All.msg(roomName);
        }

        public void RemoveFromRoom(string roomName)
        {
            using (var db = new HubContext())
            {
                var user = db.Users.Include(x => x.Connections).SingleOrDefault(x => x.UserName == Context.User.Identity.Name);

                if (user != null)
                {
                    Clients.All.userChanged(user.UserName);
                }
            }
        }
    }
}
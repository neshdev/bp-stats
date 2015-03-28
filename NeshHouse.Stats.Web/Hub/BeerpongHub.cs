using Microsoft.AspNet.SignalR;
using NeshHouse.Stats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Security.Principal;
using System.Security.Claims;

namespace NeshHouse.Stats.Web
{
    [Authorize]
    public class BeerpongHub : Hub
    {
        private string GetCurrentUserName()
        {
            if (Context.User.GetType() == typeof(GenericPrincipal) || Context.User.GetType() == typeof(WindowsPrincipal))
            {
                var principle = Context.Request.Environment["server.User"] as ClaimsPrincipal;
                if (principle != null)
                {
                    return principle.Identity.Name;
                }

                var token = Context.Request.QueryString.Get("Bearer");
                var authenticationTicket = Startup.AuthServerOptions.AccessTokenFormat.Unprotect(token);
                return authenticationTicket.Identity.Name;
            }
            else
            {
                var principle = Context.User as ClaimsPrincipal;
                var results = principle.Claims.First(x => x.Type == "sub");
                return principle != null ? principle.Identity.Name : string.Empty;
            }
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            //var username = GetCurrentUserName();
            var username = Context.User.Identity.Name;

            using (var db = new HubContext())
            {
                var user = db.Users.Include(x => x.Rooms).SingleOrDefault(x => x.UserName == username);
                if (user == null)
                {
                    user = new User()
                    {
                        UserName = username
                    };
                    db.Users.Add(user);
                    db.SaveChanges();
                }
                else
                {
                    foreach (var item in user.Rooms)
                    {
                        Groups.Add(Context.ConnectionId, item.RoomName);
                    }
                }
            }
            return base.OnConnected();
        }

        public void AddToRoom(string roomName)
        {
            using (var db = new HubContext())
            {
                // Retrieve room.
                var room = db.Rooms.Find(roomName);

                if (room != null)
                {
                    var user = new User() { UserName = Context.User.Identity.Name };
                    db.Users.Attach(user);

                    room.Users.Add(user);
                    db.SaveChanges();
                    Groups.Add(Context.ConnectionId, roomName);

                    Clients.All.userChanged(user);
                }
            }

            Clients.All.msg(roomName);

            
        }

        public void RemoveFromRoom(string roomName)
        {
            using (var db = new HubContext())
            {
                // Retrieve room.
                var room = db.Rooms.Find(roomName);
                if (room != null)
                {
                    var user = new User() { UserName = Context.User.Identity.Name };
                    db.Users.Attach(user);

                    room.Users.Remove(user);
                    db.SaveChanges();

                    Groups.Remove(Context.ConnectionId, roomName);

                    Clients.All.userChanged(user);
                }
            }
        }
    }
}
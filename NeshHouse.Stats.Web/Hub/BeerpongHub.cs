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

                var otherConnections = user.Connections.Where(x => x.ConnectionID != Context.ConnectionId).ToList();
                if (otherConnections != null)
                {
                    foreach (var item in otherConnections)
	                {
                        db.Connections.Remove(item);
                        Clients.Client(item.ConnectionID).disconnect();
	                }                    
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
                if (connection != null)
                {
                    db.Connections.Remove(connection);
                    //connection.Connected = false;
                    db.SaveChanges();
                }
            }
            return base.OnDisconnected(stopCalled);
        }

        public void JoinLobby(string roomName)
        {
            using (var context = new HubContext())
            {
                var connection = context.Connections.Single(x => x.ConnectionID == Context.ConnectionId);
                var user = context.Users.Single(x => x.Connections.Any(y => y.ConnectionID == connection.ConnectionID));


                var room = context.Rooms.FirstOrDefault(x => x.RoomName == roomName);
                if (room == null)
                {
                    room = new ConversationRoom()
                    {
                        RoomName = roomName,
                        Users = new List<User>() { },
                    };
                }

                var userExists = room.Users.Any(x => x.UserName == user.UserName);

                if (!userExists)
                {
                    room.Users.Add(user);
                }

                Groups.Add(Context.ConnectionId, roomName);

                Clients.OthersInGroup(roomName).joinedLobby(user);

                context.SaveChanges();
            }
        }

        public void UnjoinLobby(string roomName)
        {
            using (var context = new HubContext())
            {
                var connection = context.Connections.Single(x => x.ConnectionID == Context.ConnectionId);
                var user = context.Users.Single(x => x.Connections.Any(y => y.ConnectionID == connection.ConnectionID));

                var room = context.Rooms.FirstOrDefault(x => x.RoomName == roomName);
                if (room == null)
                {
                    throw new Exception("Room does not exists");
                }

                var userExists = room.Users.Any(x => x.UserName == user.UserName);

                if (!userExists)
                {
                    throw new Exception("User already not in room");
                }

                Groups.Remove(Context.ConnectionId, roomName);

                Clients.OthersInGroup(roomName).unjoinedLobby(user);

                context.SaveChanges();
            }
        }
    }
}
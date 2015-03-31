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
                    .SingleOrDefault(u => u.Name == name);

                if (user == null)
                {
                    user = new User
                    {
                        Name = name,
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

        public IEnumerable<UserGroup> JoinLobby(string group, string team)
        {
            using (var context = new HubContext())
            {
                var connection = context.Connections.Single(x => x.ConnectionID == Context.ConnectionId);
                var user = context.Users.Single(x => x.Connections.Any(y => y.ConnectionID == connection.ConnectionID));


                var groupRef = context.Groups.Include(x=> x.UserGroups).FirstOrDefault(x => x.Name == group);
                if (groupRef == null)
                {
                    groupRef = new Group()
                    {
                        Name = group,
                        UserGroups = new List<UserGroup>(),
                    };

                    context.Groups.Add(groupRef);
                }


                var refUseGroup = groupRef.UserGroups.FirstOrDefault(x => x.UserName == user.Name);

                if (refUseGroup == null)
                {
                    refUseGroup = new UserGroup()
                    {
                        Group = groupRef,
                        Team = team,
                        User = user,
                        CreateDate = DateTime.Now,
                        LastUpdatedDate = DateTime.Now,
                        IsConfirmed = false,
                    };

                    groupRef.UserGroups.Add(refUseGroup);
                }
                else
                {
                    refUseGroup.Team = team;
                    refUseGroup.LastUpdatedDate = DateTime.Now;
                }

                Groups.Add(Context.ConnectionId, group);

                Clients.Group(group).joinedLobby(refUseGroup);

                context.SaveChanges();

                return groupRef.UserGroups;
            }
        }

        public void UnjoinLobby(string roomName)
        {
            //using (var context = new HubContext())
            //{
            //    var connection = context.Connections.Single(x => x.ConnectionID == Context.ConnectionId);
            //    var user = context.Users.Single(x => x.Connections.Any(y => y.ConnectionID == connection.ConnectionID));

            //    var room = context.Groups.FirstOrDefault(x => x.RoomName == roomName);
            //    if (room == null)
            //    {
            //        throw new Exception("Room does not exists");
            //    }

            //    var userExists = room.Users.Any(x => x.UserName == user.Name);

            //    if (!userExists)
            //    {
            //        throw new Exception("User already not in room");
            //    }

            //    Groups.Remove(Context.ConnectionId, roomName);

            //    Clients.OthersInGroup(roomName).unjoinedLobby(new { name = user.Name });

            //    context.SaveChanges();
            //}
        }
    }
}
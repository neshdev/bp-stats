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


        private void CleanLobby(string groupName, HubContext context)
        {
            var group = context.Groups
                               .Include("UserGroups")
                               .Include("UserGroups.Game")
                               .Include("UserGroups.Game.GameResults")
                               .SingleOrDefault(x => x.Name == groupName);
            if (group != null)
            {
                var userGroupsInPending = group.UserGroups.Where(x => x.GameId != null);
                if ( userGroupsInPending.Count() > 0  ){
                    var ug = userGroupsInPending.First();
                    var totalConfirmed = ug.Game.GameResults.Count(x => x.IsConfirmed == true);
                    if (totalConfirmed == ug.Game.GameResults.Count)
                    {
                        ug.Game.Status = GameStatus.Closed;
                        context.UserGroups.RemoveRange(userGroupsInPending);
                        context.Groups.Remove(group);
                        context.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Lobby currently in use.");
                    }
                }
            }
        }

        public IEnumerable<UserGroup> JoinLobby(string group, string team)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new Exception("Group cannot be empty");
            }

            if (string.IsNullOrEmpty(team)){
                throw new Exception("Group cannot be empty");
            }

            using (var context = new HubContext())
            {
                CleanLobby(group, context);

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
                else
                {
                    //todo: only users who are part of the lobby can enter the lobby
                    //if (groupRef.UserGroups.Any(x=> x.GameId.HasValue))
                    //{
                    //    throw new Exception("Lobby name is taken. Lobby is pending for results.");
                    //}

                    if (groupRef.UserGroups.Count > 4)
                    {
                        throw new Exception(string.Format("This is in only designed to handle 1v1 or 2v2. Only 4 players max allowed in lobby.", groupRef.UserGroups.Count));
                    }
                }

                var refUseGroup = groupRef.UserGroups.FirstOrDefault(x => x.UserName == user.Name);

                if (refUseGroup == null)
                {
                    refUseGroup = new UserGroup()
                    {
                        GroupName = groupRef.Name,
                        Group = groupRef,
                        Team = team,
                        User = user,
                        UserName = user.Name,
                    };

                    groupRef.UserGroups.Add(refUseGroup);
                }
                else
                {
                    refUseGroup.Team = team;
                    refUseGroup.LastUpdatedDate = DateTimeOffset.Now;
                }

                Groups.Add(Context.ConnectionId, group);

                Clients.Group(group).joinedLobby(refUseGroup);

                context.SaveChanges();

                return groupRef.UserGroups;
            }
        }

        public void UnjoinLobby(string group)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new Exception("Group cannot be empty");
            }

            using (var context = new HubContext())
            {
                var connection = context.Connections.Single(x => x.ConnectionID == Context.ConnectionId);
                var user = context.Users.Single(x => x.Connections.Any(y => y.ConnectionID == connection.ConnectionID));

                var groupRef = context.Groups.Include(x => x.UserGroups).FirstOrDefault(x => x.Name == group);
                if (groupRef == null)
                {
                    throw new Exception("Group does not exists");
                }

                var refUseGroup = groupRef.UserGroups.FirstOrDefault(x => x.UserName == user.Name);

                if (refUseGroup == null)
                {
                    throw new Exception("User is already not in group");
                }
                else
                {
                    context.UserGroups.Remove(refUseGroup);
                }

                Groups.Remove(Context.ConnectionId, group);

                Clients.Group(group).unjoinedLobby(refUseGroup);

                context.SaveChanges();
            }
        }

        public void ReportWin(string group, string winningTeam)
        {
            if (string.IsNullOrEmpty(group))
            {
                throw new Exception("Group cannot be empty");
            }

            if (string.IsNullOrEmpty(winningTeam))
            {
                throw new Exception("winningTeam cannot be empty");
            }

            //block clients from reporting?

            using (var context = new HubContext())
            {
                var connection = context.Connections.Single(x => x.ConnectionID == Context.ConnectionId);
                var user = context.Users.Single(x => x.Connections.Any(y => y.ConnectionID == connection.ConnectionID));

                var groupRef = context.Groups
                                      .Include(x => x.UserGroups)
                                      .Include("UserGroups.User")
                                      .Include("UserGroups.User.Connections")
                                      .FirstOrDefault(x => x.Name == group);
                if (groupRef == null)
                {
                    throw new Exception("Group does not exists");
                }
                else
                {
                    if (groupRef.UserGroups.Any(x=> x.GameId.HasValue))
                    {
                        throw new Exception("Game already reported.");
                    }

                    if (!(groupRef.UserGroups.Count == 2 || groupRef.UserGroups.Count == 4))
                    {
                        throw new Exception(string.Format("This is in only designed to handle 1v1 or 2v2. Currently only {0} players in lobby", groupRef.UserGroups.Count));
                    }

                    //todo: also check if teams are unique
                }

                var refUserGroup = groupRef.UserGroups.FirstOrDefault(x => x.UserName == user.Name);

                if (refUserGroup == null)
                {
                    throw new Exception("User is not in group");
                }
                var matchUp = groupRef.UserGroups.Count == 4 ? Matchup.TwoOnTwo : Matchup.OneOnOne;

                var game = new Game()
                {
                    GameResults = new List<GameResult>(),
                    ReportDate = DateTimeOffset.Now,
                    Status = GameStatus.PendingConfirmation,
                    Matchup = matchUp,
                };

                

                foreach (var item in groupRef.UserGroups)
                {
                    var gameResult = new GameResult
                                  {
                                      UserName = item.UserName,
                                      User = item.User,
                                      Outcome = item.Team == winningTeam ? GameOutcome.Win : GameOutcome.Loss,
                                      IsConfirmed = false,
                                  };

                    game.GameResults.Add(gameResult);
                    item.Game = game;
                }

                var currentUserGameResults = game.GameResults.Where(x => x.UserName == user.Name).First();
                currentUserGameResults.IsConfirmed = true;

                var refUserGroupOthers = groupRef.UserGroups.Where(x => x.UserName != user.Name);

                foreach (var item in refUserGroupOthers)
                {
                    var activeConnectons = item.User.Connections.Where(x => x.Connected);
                    foreach (var c in activeConnectons)
                    {
                        var gameResult = game.GameResults.FirstOrDefault(x => x.UserName == item.UserName);

                        Clients.Client(c.ConnectionID).confirmResults(gameResult);
                    }
                }

                context.SaveChanges();
            }
        }
    }
}
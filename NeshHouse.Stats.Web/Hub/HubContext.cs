using Moserware.Skills;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace NeshHouse.Stats.Web.Models
{
    public class HubContext : DbContext
    {

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserGroup>().HasKey(ug => new { ug.UserName, ug.GroupName });

            
        }

        public HubContext()
            : base("name=HubContext")
        {
            //Database.SetInitializer<HubContext>(new MigrateDatabaseToLatestVersion<HubContext, Configuration>());
            //Database.SetInitializer<HubContext>(new DropCreateDatabaseAlways<HubContext>());

        }

        public override int SaveChanges()
        {
            foreach (var auditableEntity in ChangeTracker.Entries<Auditable>())
            {
                if (auditableEntity.State == EntityState.Added || auditableEntity.State == EntityState.Modified)
                {
                    // implementation may change based on the useage scenario, this
                    // sample is for forma authentication.

                    // modify updated date and updated by column for 
                    // adds of updates.
                    auditableEntity.Entity.LastUpdatedDate = DateTimeOffset.Now;

                    // pupulate created date and created by columns for
                    // newly added record.
                    if (auditableEntity.State == EntityState.Added)
                    {
                        auditableEntity.Entity.CreateDate = DateTimeOffset.Now;
                    }
                    else
                    {
                        // we also want to make sure that code is not inadvertly
                        // modifying created date and created by columns 
                        auditableEntity.Property(p => p.CreateDate).IsModified = false;
                    }
                }
            }

            return base.SaveChanges();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }

        public DbSet<Game> Games { get; set; }
        public DbSet<GameResult> GameResults { get; set; }
        public DbSet<GameTeam> GameTeams { get; set; }

        public DbSet<Team> Teams { get; set; }
        public DbSet<UserTeam> UserTeams { get; set; }
    }


    public class User
    {
        [Key]
        public string Name { get; set; }

        [JsonIgnore]
        public virtual ICollection<Connection> Connections { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserGroup> UserGroups { get; set; }
    }

    public class Connection
    {
        public string ConnectionID { get; set; }
        public string UserAgent { get; set; }
        public bool Connected { get; set; }
    }

    public class Group
    {
        [Key]
        public string Name { get; set; }

        public virtual ICollection<UserGroup> UserGroups { get; set; }

    }

    public class UserGroup : Auditable
    {
        public string GroupName { get; set; }
        public string UserName { get; set; }

        [JsonIgnore]
        [ForeignKey("GroupName")]
        public Group Group { get; set; }

        [JsonIgnore]
        [ForeignKey("UserName")]
        public User User { get; set; }

        public string Team { get; set; }

        [JsonIgnore]
        public int? GameId { get; set; }

        [ForeignKey("GameId")]
        public Game Game { get; set; }
    }

    public class Game : Auditable
    {
        public int Id { get; set; }
        public DateTimeOffset ReportDate { get; set; }
        public GameStatus Status { get; set; }

        public Matchup Matchup { get; set; }

        public double MatchQuality { get; set; }

        public virtual ICollection<GameResult> GameResults { get; set; }

        public virtual ICollection<GameTeam> GameTeams { get; set; }
    }

    public enum GameStatus
    {
        PendingConfirmation,
        Rejected,
        Review,
        Closed,
    }

    public enum Matchup
    {
        OneOnOne = 1,
        TwoOnTwo = 2,
    }

    public class GameResult : Auditable
    {
        public int Id { get; set; }

        public int GameId { get; set; }

        public string UserName { get; set; }

        public bool IsConfirmed { get; set; }

        [JsonIgnore]
        [ForeignKey("GameId")]
        public Game Game { get; set; }

        [JsonIgnore]
        [ForeignKey("UserName")]
        public User User { get; set; }

        public GameOutcome Outcome { get; set; }
    }

    public class GameTeam
    {
        public int Id { get; set; }
        public int GameId { get; set; }

        [JsonIgnore]
        [ForeignKey("GameId")]
        public Game Game { get; set; }

        public int TeamId { get; set; }

        
        [JsonIgnore]
        [ForeignKey("TeamId")]
        public Team Team { get; set; }
    }

    public enum GameOutcome
    {
        Loss,
        Win,
    }

    public abstract class Auditable
    {
        [JsonIgnore]
        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset? CreateDate { get; set; }

        [JsonIgnore]
        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset? LastUpdatedDate { get; set; }
    }

    public class EnumExtensions
    {
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
    }

    public static class UserRatingExtensions
    {
        private static Team CreateTeam(IEnumerable<User> users)
        {
            GameInfo gi = GameInfo.DefaultGameInfo;
            Team team = new Team()
            {
                Mean = gi.DefaultRating.Mean,
                StandardDeviation = gi.DefaultRating.StandardDeviation,
                ConservativeRating = gi.DefaultRating.ConservativeRating,
                UserTeamRatings = users.Select(x => new UserTeam
                {
                    User = x,
                    UserName = x.Name,
                }).ToList(),
                Count = users.Count(),
            };
            return team;
        }

        public static Team FirstTeamOrDefault(this HubContext context, IEnumerable<User> users)
        {
            Team team = null;
            var userNames = users.Select(x => x.Name);
            var count = userNames.Count();
            //group by losing include
            var utrByTeam = context
                                    .UserTeams
                                    .Include(x => x.Team)
                                    .GroupBy(x => x.TeamId)
                                    .Where(x => x.Count() == count)
                                    .Select(x => new
                                    {
                                        key = x.Key,
                                        items = x
                                    }).ToList();

            foreach (var item in utrByTeam)
            {
                if (item.items.Select(x=> x.UserName).Intersect(userNames).Count() == userNames.Count())
                {
                    team = context.Teams.First(x => x.Id == item.key);
                    break;
                }
            }
            
            if ( team == null)
            {
                GameInfo gi = GameInfo.DefaultGameInfo;
                team = new Team()
                {
                    Name = string.Join(" and ", userNames),
                    Mean = gi.DefaultRating.Mean,
                    StandardDeviation = gi.DefaultRating.StandardDeviation,
                    ConservativeRating = gi.DefaultRating.ConservativeRating,
                    UserTeamRatings = users.Select(x => new UserTeam
                    {
                        User = x,
                        UserName = x.Name,
                    }).ToList(),
                    Count = users.Count(),
                };
                context.Teams.Add(team);
            }
            return team;
        }

        private static void AssociateTeamToGame(HubContext context, Game game, Team team)
        {
            var gt = new GameTeam
            {
                Team = team,
                Game = game,
            };

            game.GameTeams.Add(gt);
        }


        public static void EvaluateRating(HubContext context, Game game, IEnumerable<User> winners, IEnumerable<User> losers)
        {
            var winningTeamRating = context.FirstTeamOrDefault(winners);
            var losingTeamRating = context.FirstTeamOrDefault(losers);

            var winningPlayer = new Player<Team>(winningTeamRating);
            var losingPlayer = new Player<Team>(losingTeamRating);

            var winningTeam = new Team<Player<Team>>(winningPlayer, new Rating(winningTeamRating.Mean, winningTeamRating.StandardDeviation, winningTeamRating.ConservativeRating));
            var losingTeam = new Team<Player<Team>>(losingPlayer, new Rating(losingTeamRating.Mean, losingTeamRating.StandardDeviation, losingTeamRating.ConservativeRating));

            var teams = Teams.Concat(winningTeam, losingTeam);

            var gameInfo = GameInfo.DefaultGameInfo;

            double matchQuality = TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams);

            game.MatchQuality = matchQuality;

            var newRatings = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);

            foreach (var item in newRatings)
            {
                var teamRating = item.Key.Id;
                var rating = item.Value;
                teamRating.ConservativeRating = rating.ConservativeRating;
                teamRating.Mean = rating.Mean;
                teamRating.StandardDeviation = rating.StandardDeviation;
            }

            AssociateTeamToGame(context, game, winningTeamRating);
            AssociateTeamToGame(context, game, losingTeamRating);
        }

    }

    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int Count { get; set; }

        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public double ConservativeRating { get; set; }

        public ICollection<UserTeam> UserTeamRatings { get; set; }
    }

    public class UserTeam
    {
        public int id { get; set; }

        public string UserName { get; set; }

        public int TeamId { get; set; }

        [JsonIgnore]
        [ForeignKey("TeamId")]
        public Team Team { get; set; }

        [JsonIgnore]
        [ForeignKey("UserName")]
        public User User { get; set; }
    }

    public class Ranking
    {
        public int Rank { get; set; }
        public int DenseRank { get; set; }
        public string UserName { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Total { get; set; }
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public double ConservativeRating { get; set; }
    }
}

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

        public DbSet<TeamRating> TeamRatings { get; set; }
        public DbSet<UserTeamRating> UserTeamRatings { get; set; }
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
        private static TeamRating CreateTeam(IEnumerable<User> users)
        {
            GameInfo gi = GameInfo.DefaultGameInfo;
            TeamRating team = new TeamRating()
            {
                Mean = gi.DefaultRating.Mean,
                StandardDeviation = gi.DefaultRating.StandardDeviation,
                ConservativeRating = gi.DefaultRating.ConservativeRating,
                UserTeamRatings = users.Select(x => new UserTeamRating
                {
                    User = x,
                    UserName = x.Name,
                }).ToList(),
                Count = users.Count(),
            };
            return team;
        }

        public static TeamRating FirstTeamOrDefault(this HubContext context, IEnumerable<User> users)
        {
            TeamRating team = null;
            var userNames = users.Select(x => x.Name);

            var list = context
                                    .UserTeamRatings
                                    .Include(x => x.TeamRating)
                                    .Where(x => userNames.Contains(x.UserName))
                                    .GroupBy(x => x.TeamRatingId);

            //group by losing include
            var utrByTeam = context
                                    .UserTeamRatings
                                    .Include(x => x.TeamRating)
                                    .Where(x => userNames.Contains(x.UserName))
                                    .GroupBy(x => x.TeamRatingId)
                                    .AsEnumerable()
                                    .FirstOrDefault(x => x.Count() == users.Count());

            if (utrByTeam != null)
            {
                var id = utrByTeam.First().TeamRatingId;
                team = context.TeamRatings.First(x => x.Id == id);
            }
            else
            {
                GameInfo gi = GameInfo.DefaultGameInfo;
                team = new TeamRating()
                {
                    Name = string.Join(" and ", userNames),
                    Mean = gi.DefaultRating.Mean,
                    StandardDeviation = gi.DefaultRating.StandardDeviation,
                    ConservativeRating = gi.DefaultRating.ConservativeRating,
                    UserTeamRatings = users.Select(x => new UserTeamRating
                    {
                        User = x,
                        UserName = x.Name,
                    }).ToList(),
                    Count = users.Count(),
                };
                context.TeamRatings.Add(team);
            }

            return team;

        }


        public static void EvaluateRating(HubContext context, Game game, IEnumerable<User> winners, IEnumerable<User> losers)
        {
            var winningTeamRating = context.FirstTeamOrDefault(winners);
            var losingTeamRating = context.FirstTeamOrDefault(losers);

            var winningPlayer = new Player<TeamRating>(winningTeamRating);
            var losingPlayer = new Player<TeamRating>(losingTeamRating);

            var winningTeam = new Team<Player<TeamRating>>(winningPlayer, new Rating(winningTeamRating.Mean, winningTeamRating.StandardDeviation, winningTeamRating.ConservativeRating));
            var losingTeam = new Team<Player<TeamRating>>(losingPlayer, new Rating(losingTeamRating.Mean, losingTeamRating.StandardDeviation, losingTeamRating.ConservativeRating));

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
        }

    }

    public class TeamRating
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int Count { get; set; }

        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public double ConservativeRating { get; set; }

        public ICollection<UserTeamRating> UserTeamRatings { get; set; }
    }

    public class UserTeamRating
    {
        public int id { get; set; }

        public string UserName { get; set; }

        public int TeamRatingId { get; set; }

        [JsonIgnore]
        [ForeignKey("TeamRatingId")]
        public TeamRating TeamRating { get; set; }

        [JsonIgnore]
        [ForeignKey("UserName")]
        public User User { get; set; }
    }

    public class Ranking
    {
        public string UserName { get; set; }
        public int Wins { get; set; }
        public int Total { get; set; }
        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public double ConservativeRating { get; set; }
    }
}

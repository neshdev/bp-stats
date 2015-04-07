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

        public DbSet<UserRating> UserRatings { get; set; }
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
        public static UserRating FindAddUserRatingifNotExists(this HubContext context, User user)
        {
            UserRating rating = context.UserRatings.FirstOrDefault(x => x.Name == user.Name);
            if (rating == null)
            {
                var gameInfo = GameInfo.DefaultGameInfo;
                
                rating = new UserRating
                {
                    User = user,
                    Name = user.Name,
                    ConservativeRating = gameInfo.DefaultRating.ConservativeRating,
                    Mean = gameInfo.DefaultRating.Mean,
                    StandardDeviation = gameInfo.DefaultRating.StandardDeviation,
                };
                context.UserRatings.Add(rating);
            }
            return rating;
        }

        public static void EvaluateRating(HubContext context, Game game, IEnumerable<User> winners, IEnumerable<User> losers)
        {
            var winningTeam = new Team<Player<UserRating>>();
            foreach (var item in winners)
            {
                var userRating = context.FindAddUserRatingifNotExists(item);
                var player = new Player<UserRating>(userRating);
                winningTeam.AddPlayer(player, new Rating(userRating.Mean, userRating.StandardDeviation, userRating.ConservativeRating));
            }

            var losingTeam = new Team<Player<UserRating>>();
            foreach (var item in losers)
            {
                var userRating = context.FindAddUserRatingifNotExists(item);
                var player = new Player<UserRating>(userRating);
                losingTeam.AddPlayer(player, new Rating(userRating.Mean, userRating.StandardDeviation, userRating.ConservativeRating));
            }
            
            var teams = Teams.Concat(winningTeam, losingTeam);

            var gameInfo = GameInfo.DefaultGameInfo;

            double matchQuality = TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams);

            game.MatchQuality = matchQuality;

            var newRatings = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);

            foreach (var item in newRatings)
            {
                var userRating = item.Key.Id;
                var rating = item.Value;
                userRating.ConservativeRating = rating.ConservativeRating;
                userRating.Mean = rating.Mean;
                userRating.StandardDeviation = rating.StandardDeviation;
            }
        } 
    }

    public class UserRating
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [ForeignKey("Name")]
        public User User { get; set; }

        public double Mean { get; set; }
        public double StandardDeviation { get; set; }
        public double ConservativeRating { get; set; }
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

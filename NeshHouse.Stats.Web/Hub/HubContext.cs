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
                    auditableEntity.Entity.LastUpdatedDate = DateTime.Now;

                    // pupulate created date and created by columns for
                    // newly added record.
                    if (auditableEntity.State == EntityState.Added)
                    {
                        auditableEntity.Entity.CreateDate = DateTime.Now;
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
        public DateTime ReportDate { get; set; }
        public GameStatus Status { get; set; }
        public virtual ICollection<GameResult> GameResults { get; set; }
    }

    public enum GameStatus
    {
        PendingConfirmation,
        Rejected,
        Review,
        Closed,
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
        Win,
        Loss,
    }

    public abstract class Auditable
    {
        [JsonIgnore]
        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? CreateDate { get; set; }

        [JsonIgnore]
        //[Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? LastUpdatedDate { get; set; }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
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
            Database.SetInitializer<HubContext>(new CreateDatabaseIfNotExists<HubContext>());
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
    
    }


    public class User
    {
        [Key]
        public string Name { get; set; }

        [JsonIgnore] 
        public ICollection<Connection> Connections { get; set; }

        [JsonIgnore]
        public ICollection<UserGroup> UserGroups { get; set; }
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

        public ICollection<UserGroup> UserGroups { get; set; }

    }

    public class UserGroup
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
        public DateTime CreateDate { get; set; }

        [JsonIgnore]
        public DateTime LastUpdatedDate { get; set; }

        [JsonIgnore]
        public bool IsConfirmed { get; set; }
    }
}

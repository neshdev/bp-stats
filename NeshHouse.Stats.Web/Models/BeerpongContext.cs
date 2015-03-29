using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace NeshHouse.Stats.Web.Models
{
    public class BeerpongContext : DbContext
    {
        public BeerpongContext()
            : base("BeerPongContext")
        {

        }

        public DbSet<Stats> Stats { get; set; }
    }
}
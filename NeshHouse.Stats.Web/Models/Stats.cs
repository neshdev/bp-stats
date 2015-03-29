using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NeshHouse.Stats.Web.Models
{
    public class Stats
    {
        [Key]
        public int Id { get; set; }
        public string WinningPlayer { get; set; }
        public string LosingPlayer { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
    }
}
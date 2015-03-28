using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeshHouse.Stats.Web.Models
{
    public class Game
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public GameStatus Status { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeshHouse.Stats.Web.Models
{
    public class GamePlayer
    {
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public GameResults Results { get; set; } 
    }
}
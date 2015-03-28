using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NeshHouse.Stats.Web.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Alias { get; set; }
        public TeamType TeamType { get; set; }
    }

    public enum TeamType
    {
        RED,
        BLUE,
    }
}
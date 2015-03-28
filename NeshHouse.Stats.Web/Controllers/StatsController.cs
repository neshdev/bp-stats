using NeshHouse.Stats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NeshHouse.Stats.Web.Controllers
{
    [RoutePrefix("api/Stats")]
    public class StatsController : ApiController
    {

        [Authorize]
        [Route]
        public IHttpActionResult Get()
        {
            return 
                Ok(
                    new 
                    {
                        UserName = "test",
                        Win = 20,
                        Loss = 1,
                    }
            );
        }

        [AllowAnonymous]
        [Route("Rankings")]
        public IHttpActionResult GetRankings()
        {
            var rankinsgs = new List<Ranking>
            {
                new Ranking {
                    Position = 1,
                    Alias = "nesh",
                    Wins = 10,
                    Loss = 10,
                },
                new Ranking {
                    Position = 2,
                    Alias = "vic",
                    Wins = 10,
                    Loss = 10,
                },
            };

            return Ok(rankinsgs);
        }
            
    }

    public class Ranking
    {
        public int Position { get; set; }
        public string Alias { get; set; }
        public decimal Wins { get; set; }
        public decimal Loss { get; set; }
    }
}

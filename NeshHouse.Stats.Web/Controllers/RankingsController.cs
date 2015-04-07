using NeshHouse.Stats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NeshHouse.Stats.Web.Controllers
{
    public class RankingsController : ApiController
    {
        private HubContext db = new HubContext();

        // GET: api/Rankings
        public IQueryable<Ranking> GetRankings()
        {
            var rankings = from ur in db.UserRatings
                           join gr in db.GameResults on ur.Name equals gr.UserName
                           group gr by new { ur.Name, ur.StandardDeviation, ur.Mean, ur.ConservativeRating } into grp
                           orderby grp.Key.Mean descending, grp.Key.StandardDeviation descending, grp.Key.ConservativeRating descending
                           select new Ranking
                           {
                               UserName = grp.Key.Name,
                               Mean = grp.Key.Mean,
                               StandardDeviation = grp.Key.StandardDeviation,
                               ConservativeRating = grp.Key.ConservativeRating,
                               Total = grp.Count(),
                               Wins = grp.Sum(x=> x.Outcome == GameOutcome.Win ? 1 : 0)
                           };

            return rankings;
        }

    }
}

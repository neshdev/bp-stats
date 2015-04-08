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
            var count = 1;

            var rankings = from tr in db.TeamRatings
                           join utr in db.UserTeamRatings on tr.Id equals utr.TeamRatingId
                           join gr in db.GameResults on utr.UserName equals gr.UserName
                           where tr.Count == count
                           group gr by new { utr.UserName, tr.StandardDeviation, tr.Mean, tr.ConservativeRating } into grp
                           orderby grp.Key.Mean descending, grp.Key.StandardDeviation descending, grp.Key.ConservativeRating descending
                           select new Ranking
                           {
                               UserName = grp.Key.UserName,
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

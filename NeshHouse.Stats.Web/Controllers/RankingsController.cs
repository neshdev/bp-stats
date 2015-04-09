using NeshHouse.Stats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.Data.SqlClient;

namespace NeshHouse.Stats.Web.Controllers
{
    public class RankingsController : ApiController
    {
        private HubContext db = new HubContext();

       //  GET: api/Rankings
        public IEnumerable<Ranking> GetRankings()
        {
            Dictionary<string, string> paramDict = this.Request.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

            int matchup = 1;

            if (paramDict.ContainsKey("matchup"))
            {
                var matchupString = paramDict["matchup"];
                var tryMatchup = 0;
                matchup = int.TryParse(matchupString, out tryMatchup) ? tryMatchup : matchup;
            }
            
            var results = RetrieveRankings(matchup);

            # region inefficient query in linqpad using sp instead
            //
            //var results = from s in 
            //                  (from g in db.Games
            //                  where g.Matchup == matchUp
            //                  join gt in db.GameTeams on g.Id equals gt.GameId
            //                  join t in db.Teams on gt.TeamId equals t.Id
            //                  join ut in db.UserTeams on t.Id equals ut.TeamId
            //                  join gr in db.GameResults on new { g.Id, ut.UserName } equals new { Id = gr.GameId, UserName = gr.UserName }
            //                  group gr by new { t.Name, t.Mean, t.StandardDeviation, t.ConservativeRating, gr.Outcome, g.Id })
            //              group s by new { s.Key.Name, s.Key.Mean, s.Key.StandardDeviation, s.Key.ConservativeRating } into rankings
            //              orderby rankings.Key.Mean descending
            //                     ,rankings.Key.ConservativeRating descending
            //                     , rankings.Key.StandardDeviation descending
            //              select new Ranking
            //              {
            //                  UserName = rankings.Key.Name,
            //                  Total = rankings.Select(x=> x.Key.Id).Count(),
            //                  Wins = rankings.Where(x=> x.Key.Outcome == GameOutcome.Win).Count(),
            //                  Losses = rankings.Where(x => x.Key.Outcome == GameOutcome.Loss).Count(),
            //                  Mean = rankings.Key.Mean,
            //                  StandardDeviation = rankings.Key.StandardDeviation,
            //                  ConservativeRating = rankings.Key.ConservativeRating,
            //              };
            # endregion

            return results;
        }
        
        private List<Ranking> RetrieveRankings(int matchup)
        {
            var results = db.Database.SqlQuery<Ranking>("exec [dbo].[stats_RetrieveRankings] {0}", matchup).ToList();
            return results;
        }

    }
}

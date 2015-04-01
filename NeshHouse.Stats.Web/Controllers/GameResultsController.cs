using NeshHouse.Stats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.OData;

namespace NeshHouse.Stats.Web.Controllers
{
    //todo: [Authorize]
    public class GameResultsController : ODataController
    {
        HubContext db = new HubContext();

        [EnableQuery]
        public IQueryable<GameResult> Get()
        {
            return db.GameResults;
        }

        [EnableQuery]
        public SingleResult<GameResult> Get([FromODataUri] int key)
        {
            IQueryable<GameResult> result = db.GameResults.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

    }
}
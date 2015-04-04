using NeshHouse.Stats.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

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

        private bool GameResultExists(int key)
        {
            return db.Games.Any(p => p.Id == key);
        }

        [EnableQuery]
        public SingleResult<GameResult> Get([FromODataUri] int key)
        {
            IQueryable<GameResult> result = db.GameResults.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }

        public async Task<IHttpActionResult> Put([FromODataUri] int key, GameResult update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != update.Id)
            {
                return BadRequest();
            }
            db.Entry(update).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameResultExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Updated(update);
        }

    }
}
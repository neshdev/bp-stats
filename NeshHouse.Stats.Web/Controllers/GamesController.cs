using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using NeshHouse.Stats.Web.Models;

namespace NeshHouse.Stats.Web.Controllers
{
    /*
    The WebApiConfig class may require additional changes to add a route for this controller. Merge these statements into the Register method of the WebApiConfig class as applicable. Note that OData URLs are case sensitive.

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using NeshHouse.Stats.Web.Models;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<Game>("Games");
    builder.EntitySet<GameResult>("GameResults"); 
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    public class GamesController : ODataController
    {
        private HubContext db = new HubContext();

        // GET: odata/Games
        [EnableQuery]
        public IQueryable<Game> GetGames()
        {
            var results = db.Games;
            return results;
        }

        // GET: odata/Games(5)
        [EnableQuery]
        public SingleResult<Game> GetGame([FromODataUri] int key)
        {
            return SingleResult.Create(db.Games.Where(game => game.Id == key));
        }

        // PUT: odata/Games(5)
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Delta<Game> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Game game = await db.Games.FindAsync(key);
            if (game == null)
            {
                return NotFound();
            }

            patch.Put(game);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(game);
        }

        // POST: odata/Games
        public async Task<IHttpActionResult> Post(Game game)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Games.Add(game);
            await db.SaveChangesAsync();

            return Created(game);
        }

        // PATCH: odata/Games(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Game> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Game game = await db.Games.FindAsync(key);
            if (game == null)
            {
                return NotFound();
            }

            patch.Patch(game);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GameExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(game);
        }

        // DELETE: odata/Games(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            Game game = await db.Games.FindAsync(key);
            if (game == null)
            {
                return NotFound();
            }

            db.Games.Remove(game);
            await db.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/Games(5)/GameResults
        [EnableQuery]
        public IQueryable<GameResult> GetGameResults([FromODataUri] int key)
        {
            return db.Games.Where(m => m.Id == key).SelectMany(m => m.GameResults);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GameExists(int key)
        {
            return db.Games.Count(e => e.Id == key) > 0;
        }
    }
}

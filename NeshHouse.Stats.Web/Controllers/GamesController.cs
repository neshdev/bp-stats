using NeshHouse.Stats.Web.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.OData;

namespace NeshHouse.Stats.Web.Controllers
{
    //todo: [Authorize]
    public class GamesController : ODataController
    {
        HubContext db = new HubContext();

        [EnableQuery]
        public IQueryable<Game> Get()
        {
            return db.Games;
        }
        [EnableQuery]
        public SingleResult<Game> Get([FromODataUri] int key)
        {
            IQueryable<Game> result = db.Games.Where(p => p.Id == key);
            return SingleResult.Create(result);
        }
        
        private bool GameExists(int key)
        {
            return db.Games.Any(p => p.Id == key);
        }

        public async Task<IHttpActionResult> Post(Game product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            db.Games.Add(product);
            await db.SaveChangesAsync();
            return Created(product);
        }

        public async Task<IHttpActionResult> Patch([FromODataUri] int key, Delta<Game> product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var entity = await db.Games.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            product.Patch(entity);
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
            return Updated(entity);
        }
        public async Task<IHttpActionResult> Put([FromODataUri] int key, Game update)
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
                if (!GameExists(key))
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

        public async Task<IHttpActionResult> Delete([FromODataUri] int key)
        {
            var product = await db.Games.FindAsync(key);
            if (product == null)
            {
                return NotFound();
            }
            db.Games.Remove(product);
            await db.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
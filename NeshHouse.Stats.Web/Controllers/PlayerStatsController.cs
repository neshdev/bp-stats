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
using System.Web.Http.Description;
using NeshHouse.Stats.Web.Models;

namespace NeshHouse.Stats.Web.Controllers
{
    public class PlayerStatsController : ApiController
    {
        private BeerpongContext db = new BeerpongContext();

        // GET: api/PlayerStats
        public IQueryable<NeshHouse.Stats.Web.Models.Stats> GetStats()
        {
            return db.Stats;
        }

        // GET: api/PlayerStats/5
        [ResponseType(typeof(NeshHouse.Stats.Web.Models.Stats))]
        public async Task<IHttpActionResult> GetStats(int id)
        {
            NeshHouse.Stats.Web.Models.Stats stats = await db.Stats.FindAsync(id);
            if (stats == null)
            {
                return NotFound();
            }

            return Ok(stats);
        }

        // PUT: api/PlayerStats/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutStats(int id, NeshHouse.Stats.Web.Models.Stats stats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != stats.Id)
            {
                return BadRequest();
            }

            db.Entry(stats).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StatsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/PlayerStats
        [ResponseType(typeof(NeshHouse.Stats.Web.Models.Stats))]
        public async Task<IHttpActionResult> PostStats(NeshHouse.Stats.Web.Models.Stats stats)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Stats.Add(stats);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = stats.Id }, stats);
        }

        // DELETE: api/PlayerStats/5
        [ResponseType(typeof(NeshHouse.Stats.Web.Models.Stats))]
        public async Task<IHttpActionResult> DeleteStats(int id)
        {
            NeshHouse.Stats.Web.Models.Stats stats = await db.Stats.FindAsync(id);
            if (stats == null)
            {
                return NotFound();
            }

            db.Stats.Remove(stats);
            await db.SaveChangesAsync();

            return Ok(stats);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StatsExists(int id)
        {
            return db.Stats.Count(e => e.Id == id) > 0;
        }
    }
}
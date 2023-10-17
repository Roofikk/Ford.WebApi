using Ford.DataContext.Sqlite;
using Ford.Models;
using Ford.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ford.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorsesController : ControllerBase
    {
        IRepository<Horse, long> db;

        public HorsesController(IRepository<Horse, long> db)
        {
            this.db = db;
        }

        [HttpGet]
        [Route("/{userId}")]
        public async Task<ActionResult> Get(string userId, long? horseId)
        {
            var hs = await db.RetrieveAllAsync();

            if (hs is not null && hs.Any())
            {
                IEnumerable<Horse> userHorses = hs.Where(h => h.Users.Any(u => u.UserId == userId));
                if (horseId is null)
                {
                    return Ok(userHorses);
                }
                else
                {
                    Horse? horse = hs.FirstOrDefault(h => h.Users.Any(u => u.UserId == userId) && h.HorseId == horseId);
                    return Ok(horse);
                }
            }
            else
            {
                return NoContent();
            }
        }

        //[HttpPost]
        //public async Task<ActionResult<Horse>> Create(string? userId, Horse horse)
        //{

        //}

        //[HttpPut]
        //public async Task<ActionResult<Horse>> Update(Horse horse)
        //{

        //}

        //[HttpDelete]
        //public async Task<ActionResult> Delete()
        //{

        //}
    }
}

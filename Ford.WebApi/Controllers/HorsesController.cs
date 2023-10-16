using Ford.DataContext.Sqlite;
using Ford.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ford.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorsesController : ControllerBase
    {
        private readonly FordContext db;

        public HorsesController(FordContext db)
        {
            this.db = db;
        }

        [HttpPut]
        public IActionResult Create(Horse horse)
        {
            Horse? existed = db.Horses.FirstOrDefault(h => h.HorseId == horse.HorseId);

            if (existed is not null)
                return BadRequest("Current horse is exist");

            db.Horses.Add(horse);
            db.SaveChanges();
            return Ok(horse);
        }
    }
}

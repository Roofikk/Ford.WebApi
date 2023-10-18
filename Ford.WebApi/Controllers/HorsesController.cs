using AutoMapper;
using Ford.DataContext.Sqlite;
using Ford.Models;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Ford.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorsesController : ControllerBase
    {
        private readonly FordContext db;
        private readonly IMapper mapper;

        public HorsesController(FordContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{userId}")]
        public IActionResult Get(string userId, long? horseId)
        {
            IQueryable<Horse> dbHorses = db.Horses.Include(h => h.Saves);
            IEnumerable<Horse> horses = dbHorses.Where(h => h.Users.Any(u => u.UserId == userId));

            if (horses.Any())
            {
                if (horseId is not null)
                {
                    Horse? horse = horses.FirstOrDefault(h => h.HorseId == horseId);
                    HorseRetrievingDto horseDto = mapper.Map<HorseRetrievingDto>(horse);
                    return Ok(horseDto);
                }
                else
                {
                    IEnumerable<HorseRetrievingDto> horsesDto = mapper.Map<IEnumerable<HorseRetrievingDto>>(horses);
                    return Ok(horses);
                }
            }
            else
            {
                return NoContent();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get(long? horseId)
        {
            IQueryable<Horse> horsesDb = db.Horses.Include(h => h.Users).Include(h => h.Saves);

            if (horseId is not null)
            {
                var horse = await horsesDb.FirstOrDefaultAsync(h => h.HorseId == horseId);

                if (horse is not null)
                {
                    HorseRetrievingDto mappingHorse = mapper.Map<HorseRetrievingDto>(horse);
                    return Ok(horse);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                var horses = await horsesDb.ToListAsync();
                IEnumerable<HorseRetrievingDto> mapping = mapper.Map<IEnumerable<HorseRetrievingDto>>(horses);
                return Ok(mapping);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Horse>> Create([FromBody] HorseForCreationDto horse)
        {
            Horse mappingHorse = mapper.Map<Horse>(horse);

            if (horse.UserIds is not null && horse.UserIds.Any())
            {
                IEnumerable<User> users = db.Users.ToList().IntersectBy(horse.UserIds, u => u.UserId);

                if (users.Count() != horse.UserIds.Count())
                {
                    return BadRequest();
                }

                mappingHorse.Users = users.ToList();
            }

            db.Horses.Add(mappingHorse);
            await db.SaveChangesAsync();
            return Created($"api/[controller]?horseId={mappingHorse.HorseId}", mappingHorse);
        }

        //[HttpPut]
        //public async Task<ActionResult<Horse>> Update([FromBody]Horse horse)
        //{
        //    Horse? updatedHorse = await db.UpdateAsync(horse);

        //    if (updatedHorse is not null)
        //    {
        //        return Ok(updatedHorse);
        //    }
        //    else
        //    {
        //        return BadRequest();
        //    }
        //}

        //[HttpDelete]
        //public async Task<ActionResult> Delete(long id)
        //{
        //    bool result = await db.DeleteAsync(id);

        //    if (result)
        //    {
        //        return Ok();
        //    }
        //    else
        //    {
        //        return NotFound();
        //    }
        //}
    }
}

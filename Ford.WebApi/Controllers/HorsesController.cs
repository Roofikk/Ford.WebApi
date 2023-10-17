using AutoMapper;
using Ford.Models;
using Ford.WebApi.Dtos.Horse;
using Ford.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Ford.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HorsesController : ControllerBase
    {
        private readonly IHorseRepository db;
        private readonly IMapper mapper;

        public HorsesController(IHorseRepository db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("/{horseId}")]
        public async Task<ActionResult> Get(long horseId, string? userId)
        {
            var hs = await db.RetrieveAllAsync();

            if (hs is not null && hs.Any())
            {
                IEnumerable<Horse> userHorses = hs.Where(h => h.Users.Any(u => u.UserId == userId));
                if (userId is null)
                {
                    return Ok(userHorses);
                }
                else
                {
                    Horse? horse = hs.FirstOrDefault(h => h.Users.Any(u => u.UserId == userId) && h.HorseId == horseId);

                    if (horse is not null)
                    {
                        return Ok(horse);
                    }
                    else
                    {
                        return NoContent();
                    }
                }
            }
            else
            {
                return NoContent();
            }
        }

        [HttpPost]
        public async Task<ActionResult<Horse>> Create([FromBody]HorseForCreationDto horse)
        {
            Horse mappingHorse = mapper.Map<Horse>(horse);
            Horse? createdHorse = await db.CreateAsync(mappingHorse);

            if (createdHorse is not null)
            {
                return Created($"api/[controller]/{createdHorse.HorseId}", createdHorse);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut]
        public async Task<ActionResult<Horse>> Update(Horse horse)
        {
            Horse? updatedHorse = await db.UpdateAsync(horse);
            if (updatedHorse is not null)
            {
                return Ok(updatedHorse);
            }
            else
            {
                BadRequest();
            }
        }

        //[HttpDelete]
        //public async Task<ActionResult> Delete()
        //{

        //}
    }
}

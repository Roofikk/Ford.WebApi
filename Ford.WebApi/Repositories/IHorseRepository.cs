﻿using Ford.Models;

namespace Ford.WebApi.Repositories
{
    public interface IHorseRepository : IRepository<Horse, long>
    {
        //Task<bool> AddUser(string userId, Horse horse);
    }
}

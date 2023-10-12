using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ford.EntityModels
{
    public partial class HorseOwner
    {
        public int HorseId { get; set; }
        public Horse? Horse { get; set; }

        public int UserId {  get; set; }
        public User? User { get; set; }
    }
}

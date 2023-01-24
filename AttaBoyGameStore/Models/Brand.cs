﻿using System.ComponentModel.DataAnnotations;

namespace AttaBoyGameStore.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        public String Name { get; set; }

        public String Other { get; set; }
    }
}

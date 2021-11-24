using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.DTO
{
    public class UserDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Token { get; set; }

        public string PhotoUrl { get; set; }
        public string KnownAs { get; set; }
    }
}
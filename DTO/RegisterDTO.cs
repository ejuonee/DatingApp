using System.ComponentModel.DataAnnotations;

namespace DatingApp.DTO
{
    public class RegisterDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4)]
        public string Password { get; set; }
    }

    public class LoginDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class UserDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Token { get; set; }

        public string PhotoUrl { get; set; }
    }
}
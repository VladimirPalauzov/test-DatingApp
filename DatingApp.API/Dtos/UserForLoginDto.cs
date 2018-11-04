using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForLoginDto
    {
        public string Username { get; set; }

        [StringLength(8, MinimumLength = 4, ErrorMessage = "You must specify password between 5 and 8 characters")]
        public string Password { get; set; }
        
    }
}
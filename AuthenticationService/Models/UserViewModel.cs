using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models
{
    public class UserViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
        public string RedirectUrl { get; set; }

    }
}

using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Auth        
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email Requird")]
        //[EmailAddress(ErrorMessage = "Wrong Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password Required")]
        public string Password { get; set; } = string.Empty;
    }
}
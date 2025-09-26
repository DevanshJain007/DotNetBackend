using System.ComponentModel.DataAnnotations;

namespace BackendOfReactProject.DTO
{
    public class LoginDTOs
    {
         
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    
}
}

using System.ComponentModel.DataAnnotations;

namespace MultilingualCRUD_Api.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username or email is required")]
        [Display(Name = "Username or Email")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Create account if user doesn't exist")]
        public bool AutoCreate { get; set; } = false;

        public string? ErrorMessage { get; set; }
    }
}

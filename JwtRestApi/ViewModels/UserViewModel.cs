using System.ComponentModel.DataAnnotations;

namespace MultilingualCRUD_Api.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Roles")]
        public List<int> RoleIds { get; set; } = new List<int>();

        public List<Role> AvailableRoles { get; set; } = new List<Role>();
    }

}

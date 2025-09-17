
namespace MultilingualCRUD_Api.ViewModels
{
    public class DashboardViewModel
    {
        public User? CurrentUser { get; set; }
        public List<User> Users { get; set; } = new List<User>();
        public List<Role> Roles { get; set; } = new List<Role>();
        public string ActiveTab { get; set; } = "users";
    }
}

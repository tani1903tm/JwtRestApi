using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MultilingualCRUD_Api.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MultilingualCRUD_Api.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            // Check if user is authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard");
            }
            return RedirectToAction("Login", "Auth");
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
            var currentUser = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

            var users = await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();

            var roles = await _db.Roles.ToListAsync();

            var viewModel = new DashboardViewModel
            {
                CurrentUser = currentUser,
                Users = users,
                Roles = roles
            };

            return View(viewModel);
        }
    }
}

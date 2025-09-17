using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Cookies,Bearer")] // allow cookie or bearer
public class RolesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public RolesController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    [Authorize] // any authenticated user can read
    public async Task<IActionResult> GetAll() => Ok(await _db.Roles.ToListAsync());

    [HttpPost]
    [Authorize(Roles = "Admin")] // only admins can create roles
    public async Task<IActionResult> Create([FromBody] Role role)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == role.Name))
            return BadRequest("Role already exists");
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return Ok(role);
    }
}
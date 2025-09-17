using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Cookies,Bearer")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    public UsersController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    [Authorize] // any authenticated user can read
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).ToListAsync();
        var dtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
        });
        return Ok(dtos);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")] // only admin can create
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest("Email already exists");
        var user = new User { Username = dto.Username, Email = dto.Email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password) };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Username, user.Email });
    }

    [HttpPut("{id}")]
    [Authorize] // admin or the user themself
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        var isAdmin = User.IsInRole("Admin");
        var currentUserIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var isSelf = int.TryParse(currentUserIdClaim, out var currentUserId) && currentUserId == id;
        if (!isAdmin && !isSelf) return Forbid();

        if (!string.IsNullOrWhiteSpace(dto.Username)) user.Username = dto.Username;
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            var emailInUse = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != id);
            if (emailInUse) return BadRequest("Email already exists");
            user.Email = dto.Email;
        }
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.Username, user.Email });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")] // only admin can delete
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        // Make delete idempotent: return 204 even if user does not exist
        if (user == null) return NoContent();
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public System.Collections.Generic.List<string> Roles { get; set; }
}

public class CreateUserDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}

public class UpdateUserDto
{
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}
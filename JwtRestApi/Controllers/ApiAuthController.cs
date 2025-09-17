using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

[ApiController]
[Route("api/auth")]
public class ApiAuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IStringLocalizer<MultilingualCRUD_Api.Resources.SharedResources> _loc;

    public ApiAuthController(ApplicationDbContext db, IJwtService jwt, IStringLocalizer<MultilingualCRUD_Api.Resources.SharedResources> loc)
    {
        _db = db; _jwt = jwt; _loc = loc;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] AuthRequest req)
    {
        var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                      .FirstOrDefaultAsync(u => u.Username == req.UsernameOrEmail || u.Email == req.UsernameOrEmail);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = _loc["InvalidCredentials"] });

        var roles = user.UserRoles.Select(ur => ur.Role.Name);
        var access = _jwt.GenerateAccessToken(user, roles);
        var refresh = _jwt.GenerateRefreshToken();

        var refreshEntity = new RefreshToken { Token = refresh, ExpiresAt = System.DateTime.UtcNow.AddDays(7), UserId = user.Id };
        _db.RefreshTokens.Add(refreshEntity);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(access, refresh));
    }

    [HttpPost("login-or-create")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginOrCreate([FromBody] LoginOrCreateRequest req)
    {
        var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                      .FirstOrDefaultAsync(u => u.Username == req.UsernameOrEmail || u.Email == req.UsernameOrEmail);

        if (user != null)
        {
            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Unauthorized(new { message = _loc["InvalidCredentials"] });
        }
        else if (req.AutoCreate)
        {
            var inputValue = req.UsernameOrEmail.Trim();
            bool isEmail = inputValue.Contains('@');
            string username, email;

            if (isEmail)
            {
                email = inputValue;
                username = inputValue.Split('@')[0];
            }
            else
            {
                username = inputValue;
                email = $"{inputValue}@example.com";
            }

            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
            if (existingUser != null)
                return BadRequest(new { message = _loc["UserAlreadyExists"] });

            user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                          .FirstOrDefaultAsync(u => u.Id == user.Id);
        }
        else
        {
            return Unauthorized(new { message = _loc["InvalidCredentials"] });
        }

        var roles = user.UserRoles.Select(ur => ur.Role.Name);
        var accessToken = _jwt.GenerateAccessToken(user, roles);
        var refreshToken = _jwt.GenerateRefreshToken();

        var refreshEntity2 = new RefreshToken { Token = refreshToken, ExpiresAt = System.DateTime.UtcNow.AddDays(7), UserId = user.Id };
        _db.RefreshTokens.Add(refreshEntity2);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(accessToken, refreshToken));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var r = await _db.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(x => x.Token == req.RefreshToken);
        if (r == null || r.IsRevoked || r.ExpiresAt < System.DateTime.UtcNow)
            return Unauthorized(new { message = _loc["InvalidRefreshToken"] });

        var roles = r.User.UserRoles.Select(ur => ur.Role.Name);
        var newAccess = _jwt.GenerateAccessToken(r.User, roles);
        return Ok(new AuthResponse(newAccess, req.RefreshToken));
    }
}

// Using existing request/response records defined elsewhere in the project

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using MultilingualCRUD_Api.Resources;
using MultilingualCRUD_Api.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

public class AuthController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwt;
    private readonly IStringLocalizer _loc;

    public AuthController(ApplicationDbContext db, IJwtService jwt, IStringLocalizer<SharedResources> loc)
    {
        _db = db; _jwt = jwt; _loc = loc;
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Dashboard", "Home");
        }
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                          .FirstOrDefaultAsync(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);
            
            if (user != null)
            {
                // User exists, verify password
                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    model.ErrorMessage = _loc["InvalidCredentials"];
                    return View(model);
                }
            }
            else if (model.AutoCreate)
            {
                // User doesn't exist and auto-create is enabled
                var inputValue = model.UsernameOrEmail.Trim();
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
                
                // Check if username or email already exists
                var existingUser = await _db.Users.FirstOrDefaultAsync(u => 
                    u.Username == username || u.Email == email);
                if (existingUser != null)
                {
                    model.ErrorMessage = _loc["UserAlreadyExists"];
                    return View(model);
                }
                
                // Create new user
                user = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
                };
                
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                
                // Reload user with roles
                user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                              .FirstOrDefaultAsync(u => u.Id == user.Id);
            }
            else
            {
                // User doesn't exist and auto-create is disabled
                model.ErrorMessage = _loc["InvalidCredentials"];
                return View(model);
            }

            // Create claims and sign in
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            foreach (var role in user.UserRoles.Select(ur => ur.Role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync("Cookies", claimsPrincipal);

            return RedirectToAction("Dashboard", "Home");
        }
        catch (Exception ex)
        {
            model.ErrorMessage = "An error occurred during login. Please try again.";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Login");
    }

    /// <summary>
    /// Authenticates a user and returns JWT tokens
    /// </summary>
    /// <param name="req">Login credentials (username/email and password)</param>
    /// <returns>Access token and refresh token if authentication succeeds</returns>
    /// <response code="200">Returns access and refresh tokens</response>
    /// <response code="401">Invalid credentials</response>
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

    /// <summary>
    /// Authenticates a user and creates them if they don't exist
    /// </summary>
    /// <param name="req">Login credentials with auto-create option</param>
    /// <returns>Access token and refresh token if authentication succeeds or user is created</returns>
    /// <response code="200">Returns access and refresh tokens</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("login-or-create")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginOrCreate([FromBody] LoginOrCreateRequest req)
    {
        // First try to find existing user
        var user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                      .FirstOrDefaultAsync(u => u.Username == req.UsernameOrEmail || u.Email == req.UsernameOrEmail);
        
        if (user != null)
        {
            // User exists, verify password
            if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
                return Unauthorized(new { message = _loc["InvalidCredentials"] });
        }
        else if (req.AutoCreate)
        {
            // User doesn't exist and auto-create is enabled
            // Use the usernameOrEmail field for both username and email
            var inputValue = req.UsernameOrEmail.Trim();
            
            // Check if input looks like an email (contains @)
            bool isEmail = inputValue.Contains('@');
            string username, email;
            
            if (isEmail)
            {
                email = inputValue;
                username = inputValue.Split('@')[0]; // Use part before @ as username
            }
            else
            {
                username = inputValue;
                email = $"{inputValue}@example.com"; // Generate email from username
            }
            
            // Check if username or email already exists
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => 
                u.Username == username || u.Email == email);
            if (existingUser != null)
                return BadRequest(new { message = _loc["UserAlreadyExists"] });
            
            // Create new user
            user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
            };
            
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            
            // Reload user with roles
            user = await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                          .FirstOrDefaultAsync(u => u.Id == user.Id);
        }
        else
        {
            // User doesn't exist and auto-create is disabled
            return Unauthorized(new { message = _loc["InvalidCredentials"] });
        }

        // Generate tokens
        var roles = user.UserRoles.Select(ur => ur.Role.Name);
        var access = _jwt.GenerateAccessToken(user, roles);
        var refresh = _jwt.GenerateRefreshToken();

        var refreshEntity = new RefreshToken { Token = refresh, ExpiresAt = System.DateTime.UtcNow.AddDays(7), UserId = user.Id };
        _db.RefreshTokens.Add(refreshEntity);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(access, refresh));
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token
    /// </summary>
    /// <param name="req">Refresh token request</param>
    /// <returns>New access token</returns>
    /// <response code="200">Returns new access token</response>
    /// <response code="401">Invalid or expired refresh token</response>
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

public record AuthRequest(string UsernameOrEmail, string Password);
public record LoginOrCreateRequest(string UsernameOrEmail, string Password, bool AutoCreate);
public record AuthResponse(string AccessToken, string RefreshToken);
public record RefreshRequest(string RefreshToken);
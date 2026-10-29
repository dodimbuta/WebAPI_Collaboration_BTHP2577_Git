using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using user_service.Data;
using user_service.Models.DTOs;
using user_service.Models.Entities;

namespace user_service.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class UserController(UserDbContext context) : ControllerBase
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await context.Users.AsNoTracking()
            .OrderBy(user => user.Id)
            .Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName
            })
            .ToListAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("user/{id:int}")]
    public async Task<ActionResult<UserDto>> GetUser(int id, CancellationToken cancellationToken)
    {
        var user = await context.Users.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        return user is null ? NotFound(new { message = "User not found." }) : Ok(ToDto(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(
        [FromBody] UserRegistrationDto request, CancellationToken cancellationToken)
    {
        var normalizedEmail = request.Email.Trim();
        var normalizedUserName = request.UserName.Trim();
        if (await context.Users.AnyAsync(user => user.Email == normalizedEmail || user.UserName == normalizedUserName, cancellationToken))
            return Conflict(new { message = "A user with this username or email already exists." });

        var user = new User
        {
            UserName = normalizedUserName,
            PasswordHash = string.Empty,
            Email = normalizedEmail,
            FullName = request.FullName.Trim(),
            RoleProject = request.RoleProject.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Address = request.Address.Trim()
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, ToDto(user));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request, CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(
            item => item.UserName == request.UserName.Trim(), cancellationToken);
        if (user is null || !VerifyPassword(user, request.Password))
            return Unauthorized(new { message = "Username or password is incorrect." });

        // Upgrade hashes created by the previous SHA-256 implementation after a successful login.
        if (!user.PasswordHash.StartsWith("AQAAAA", StringComparison.Ordinal))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await context.SaveChangesAsync(cancellationToken);
        }

        return Ok(new { user.Id, user.UserName });
    }

    [HttpPut("user/{id:int}")]
    public async Task<IActionResult> UpdateUser(
        int id, [FromBody] UserUpdateDto request, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user is null)
            return NotFound(new { message = "User not found." });

        var email = request.Email.Trim();
        var username = request.UserName.Trim();
        if (await context.Users.AnyAsync(item => item.Id != id &&
            (item.Email == email || item.UserName == username), cancellationToken))
            return Conflict(new { message = "Another user already uses this username or email." });

        user.Email = email;
        user.UserName = username;
        user.FullName = request.FullName.Trim();
        user.RoleProject = request.RoleProject.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        user.Address = request.Address.Trim();
        if (!string.IsNullOrWhiteSpace(request.Password))
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await context.SaveChangesAsync(cancellationToken);
        return Ok(ToDto(user));
    }

    [HttpDelete("user/{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var user = await context.Users.FindAsync(new object[] { id }, cancellationToken);
        if (user is null)
            return NotFound(new { message = "User not found." });

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private bool VerifyPassword(User user, string password)
    {
        // ASP.NET Identity hashes use a version marker. Check it before parsing the hash,
        // because legacy SHA-256 values are also stored as base64 strings.
        if (user.PasswordHash.StartsWith("AQAAAA", StringComparison.Ordinal))
        {
            return _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password)
                != PasswordVerificationResult.Failed;
        }

        // Backward compatibility with existing SHA-256/base64 hashes.
        var legacyHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        var storedHashBytes = Encoding.UTF8.GetBytes(user.PasswordHash);
        var legacyHashBytes = Encoding.UTF8.GetBytes(legacyHash);
        return storedHashBytes.Length == legacyHashBytes.Length &&
               CryptographicOperations.FixedTimeEquals(storedHashBytes, legacyHashBytes);
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        FullName = user.FullName
    };
}

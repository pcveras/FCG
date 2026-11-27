using FCG.Api.Domain;
using FCG.Api.Dtos;
using FCG.Api.Infrastructure;
using FCG.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCG.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController(FcgDbContext db, IPasswordHasher hasher, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        var user = Domain.User.Create(req.Name, req.Email, req.Password, hasher);
        if (await db.Users.AnyAsync(u => u.Email == user.Email))
            return Conflict(new { error = "E-mail já cadastrado." });
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return Created($"/api/users/{user.Id}", new UserResponse(user.Id, user.Name, user.Email, user.Role));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var email = req.Email?.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user is null || string.IsNullOrEmpty(req.Password) || !hasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { error = "Credenciais inválidas." });
        return Ok(new { token = tokens.Generate(user) });
    }
}

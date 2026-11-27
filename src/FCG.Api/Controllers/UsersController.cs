using FCG.Api.Domain;
using FCG.Api.Dtos;
using FCG.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCG.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public class UsersController(FcgDbContext db) : ControllerBase
{
    private static UserResponse Map(Domain.User u) => new(u.Id, u.Name, u.Email, u.Role);

    [HttpGet]
    public async Task<IEnumerable<UserResponse>> List() =>
        (await db.Users.AsNoTracking().OrderBy(u => u.Name).ToListAsync()).Select(Map);

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var u = await db.Users.FindAsync(id);
        return u is null ? NotFound() : Ok(Map(u));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequest req)
    {
        var u = await db.Users.FindAsync(id);
        if (u is null) return NotFound();
        u.Update(req.Name, req.Email);
        if (await db.Users.AnyAsync(x => x.Email == u.Email && x.Id != id))
            return Conflict(new { error = "E-mail já cadastrado." });
        await db.SaveChangesAsync();
        return Ok(Map(u));
    }

    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, ChangeRoleRequest req)
    {
        var u = await db.Users.FindAsync(id);
        if (u is null) return NotFound();
        u.ChangeRole(req.Role);
        await db.SaveChangesAsync();
        return Ok(Map(u));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var u = await db.Users.FindAsync(id);
        if (u is null) return NotFound();
        db.Users.Remove(u);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

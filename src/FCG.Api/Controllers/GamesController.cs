using FCG.Api.Domain;
using FCG.Api.Dtos;
using FCG.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCG.Api.Controllers;

[ApiController]
[Route("api/games")]
[Authorize]
public class GamesController(FcgDbContext db) : ControllerBase
{
    private static object Map(Game g, DateTime now) =>
        new { g.Id, g.Title, g.Description, g.Genre, g.Price, CurrentPrice = g.CurrentPrice(now) };

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? genre)
    {
        var q = db.Games.Include(g => g.Promotions).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(genre)) q = q.Where(g => g.Genre == genre);
        var now = DateTime.UtcNow;
        return Ok((await q.OrderBy(g => g.Title).ToListAsync()).Select(g => Map(g, now)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var g = await db.Games.Include(x => x.Promotions).FirstOrDefaultAsync(x => x.Id == id);
        return g is null ? NotFound() : Ok(Map(g, DateTime.UtcNow));
    }

    [HttpPost, Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(GameRequest req)
    {
        var g = Game.Create(req.Title, req.Description, req.Genre, req.Price);
        db.Games.Add(g);
        await db.SaveChangesAsync();
        return Created($"/api/games/{g.Id}", Map(g, DateTime.UtcNow));
    }

    [HttpPut("{id:guid}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(Guid id, GameRequest req)
    {
        var g = await db.Games.Include(x => x.Promotions).FirstOrDefaultAsync(x => x.Id == id);
        if (g is null) return NotFound();
        g.Update(req.Title, req.Description, req.Genre, req.Price);
        await db.SaveChangesAsync();
        return Ok(Map(g, DateTime.UtcNow));
    }

    [HttpDelete("{id:guid}"), Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var g = await db.Games.FindAsync(id);
        if (g is null) return NotFound();
        if (await db.UserGames.AnyAsync(x => x.GameId == id))
            return Conflict(new { error = "Jogo já adquirido por usuários e não pode ser removido." });
        db.Games.Remove(g);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

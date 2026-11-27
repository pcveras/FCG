using System.Security.Claims;
using FCG.Api.Domain;
using FCG.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCG.Api.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
public class LibraryController(FcgDbContext db) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> List()
    {
        var items = await db.UserGames.AsNoTracking().Where(x => x.UserId == UserId)
            .Select(x => new { x.GameId, x.Game!.Title, x.PricePaid, x.AcquiredAt })
            .ToListAsync();
        return Ok(items);
    }

    [HttpPost("{gameId:guid}")]
    public async Task<IActionResult> Acquire(Guid gameId)
    {
        var userId = UserId;
        var game = await db.Games.Include(g => g.Promotions).FirstOrDefaultAsync(g => g.Id == gameId);
        if (game is null) return NotFound(new { error = "Jogo não encontrado." });
        if (!await db.Users.AnyAsync(u => u.Id == userId)) return Unauthorized();
        if (await db.UserGames.AnyAsync(x => x.UserId == userId && x.GameId == gameId))
            return Conflict(new { error = "Jogo já está na biblioteca." });

        var now = DateTime.UtcNow;
        var entry = new UserGame { UserId = userId, GameId = gameId, PricePaid = game.CurrentPrice(now), AcquiredAt = now };
        db.UserGames.Add(entry);
        await db.SaveChangesAsync();
        return Created($"/api/library", new { entry.GameId, game.Title, entry.PricePaid, entry.AcquiredAt });
    }
}

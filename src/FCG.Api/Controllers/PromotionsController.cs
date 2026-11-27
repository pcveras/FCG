using FCG.Api.Domain;
using FCG.Api.Dtos;
using FCG.Api.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FCG.Api.Controllers;

[ApiController]
[Route("api/promotions")]
[Authorize(Roles = Roles.Admin)]
public class PromotionsController(FcgDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await db.Promotions.AsNoTracking().ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(PromotionRequest req)
    {
        if (!await db.Games.AnyAsync(g => g.Id == req.GameId)) return NotFound(new { error = "Jogo não encontrado." });
        var p = Promotion.Create(req.GameId, req.DiscountPercent, req.StartsAt.ToUniversalTime(), req.EndsAt.ToUniversalTime());
        db.Promotions.Add(p);
        await db.SaveChangesAsync();
        return Created($"/api/promotions/{p.Id}", p);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await db.Promotions.FindAsync(id);
        if (p is null) return NotFound();
        db.Promotions.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

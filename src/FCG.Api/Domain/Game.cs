namespace FCG.Api.Domain;

public class Game
{
    private Game() { }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Genre { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public List<Promotion> Promotions { get; private set; } = [];

    public static Game Create(string title, string? description, string? genre, decimal price)
    {
        var game = new Game();
        game.Update(title, description, genre, price);
        return game;
    }

    public void Update(string title, string? description, string? genre, decimal price)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new DomainException("Título é obrigatório.");
        if (price < 0) throw new DomainException("Preço não pode ser negativo.");
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Genre = genre?.Trim() ?? string.Empty;
        Price = price;
    }

    /// <summary>Preço considerando a maior promoção ativa na data informada.</summary>
    public decimal CurrentPrice(DateTime now)
    {
        var discount = Promotions.Where(p => p.IsActive(now)).Select(p => p.DiscountPercent).DefaultIfEmpty(0).Max();
        return Math.Round(Price * (100 - discount) / 100, 2);
    }
}

public class Promotion
{
    private Promotion() { }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid GameId { get; private set; }
    public decimal DiscountPercent { get; private set; }
    public DateTime StartsAt { get; private set; }
    public DateTime EndsAt { get; private set; }

    public static Promotion Create(Guid gameId, decimal discountPercent, DateTime startsAt, DateTime endsAt)
    {
        if (discountPercent is <= 0 or > 100) throw new DomainException("Desconto deve estar entre 0 (exclusivo) e 100.");
        if (endsAt <= startsAt) throw new DomainException("Data final deve ser posterior à inicial.");
        return new Promotion { GameId = gameId, DiscountPercent = discountPercent, StartsAt = startsAt, EndsAt = endsAt };
    }

    public bool IsActive(DateTime now) => now >= StartsAt && now <= EndsAt;
}

public class UserGame
{
    public Guid UserId { get; set; }
    public Guid GameId { get; set; }
    public Game? Game { get; set; }
    public decimal PricePaid { get; set; }
    public DateTime AcquiredAt { get; set; }
}

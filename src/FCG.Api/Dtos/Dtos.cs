namespace FCG.Api.Dtos;

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record UpdateUserRequest(string Name, string Email);
public record ChangeRoleRequest(string Role);
public record GameRequest(string Title, string? Description, string? Genre, decimal Price);
public record PromotionRequest(Guid GameId, decimal DiscountPercent, DateTime StartsAt, DateTime EndsAt);
public record UserResponse(Guid Id, string Name, string Email, string Role);

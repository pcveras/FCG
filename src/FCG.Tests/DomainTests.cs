using FCG.Api.Domain;
using FCG.Api.Services;

namespace FCG.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData("a@b.com", true)]
    [InlineData("user@fiap.com.br", true)]
    [InlineData("semarroba.com", false)]
    [InlineData("a@b", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Email(string? email, bool expected) => Assert.Equal(expected, Validation.IsValidEmail(email));

    [Theory]
    [InlineData("Abcdef1!", true)]
    [InlineData("Abc1!", false)]
    [InlineData("abcdefgh1", false)]
    [InlineData("abcdefg!", false)]
    [InlineData("12345678!", false)]
    [InlineData(null, false)]
    public void Password(string? pwd, bool expected) => Assert.Equal(expected, Validation.IsStrongPassword(pwd));
}

public class UserTests
{
    private readonly IPasswordHasher _hasher = new Pbkdf2PasswordHasher();

    [Fact]
    public void Create_HashesPasswordAndNormalizesEmail()
    {
        var u = User.Create("Ana", "Ana@Mail.com", "Senha@123", _hasher);
        Assert.Equal("ana@mail.com", u.Email);
        Assert.NotEqual("Senha@123", u.PasswordHash);
        Assert.True(_hasher.Verify("Senha@123", u.PasswordHash));
        Assert.False(_hasher.Verify("outra", u.PasswordHash));
        Assert.Equal(Roles.User, u.Role);
    }

    [Fact]
    public void Create_WeakPassword_Throws() =>
        Assert.Throws<DomainException>(() => User.Create("Ana", "ana@mail.com", "123", _hasher));

    [Fact]
    public void Create_InvalidEmail_Throws() =>
        Assert.Throws<DomainException>(() => User.Create("Ana", "ana", "Senha@123", _hasher));

    [Fact]
    public void ChangeRole_Invalid_Throws() =>
        Assert.Throws<DomainException>(() => User.Create("Ana", "ana@mail.com", "Senha@123", _hasher).ChangeRole("Root"));
}

public class GameTests
{
    [Fact]
    public void CurrentPrice_AppliesBestActivePromotion()
    {
        var g = Game.Create("Jogo", null, null, 100m);
        var now = DateTime.UtcNow;
        g.Promotions.Add(Promotion.Create(g.Id, 10, now.AddDays(-1), now.AddDays(1)));
        g.Promotions.Add(Promotion.Create(g.Id, 25, now.AddDays(-1), now.AddDays(1)));
        g.Promotions.Add(Promotion.Create(g.Id, 90, now.AddDays(1), now.AddDays(2)));
        Assert.Equal(75m, g.CurrentPrice(now));
    }

    [Fact]
    public void Create_NegativePrice_Throws() =>
        Assert.Throws<DomainException>(() => Game.Create("Jogo", null, null, -1));

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Promotion_InvalidDiscount_Throws(decimal d) =>
        Assert.Throws<DomainException>(() => Promotion.Create(Guid.NewGuid(), d, DateTime.UtcNow, DateTime.UtcNow.AddDays(1)));
}

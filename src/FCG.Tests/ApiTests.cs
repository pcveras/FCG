using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FCG.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _db = Path.Combine(Path.GetTempPath(), $"fcg-{Guid.NewGuid()}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={_db}");
        builder.UseSetting("Admin:Email", "admin@fcg.com");
        builder.UseSetting("Admin:Password", "Admin@1234");
    }
}

public class ApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private async Task<HttpClient> Login(string email, string password)
    {
        var client = factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var token = (await res.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsBadRequest()
    {
        var res = await factory.CreateClient().PostAsJsonAsync("/api/auth/register", new { name = "A", email = "a@b.com", password = "abc" });
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Games_RequiresAuthentication()
    {
        var res = await factory.CreateClient().GetAsync("/api/games");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task FullFlow_AdminCreatesGameAndPromotion_UserBuysIt()
    {
        var anon = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Created,
            (await anon.PostAsJsonAsync("/api/auth/register", new { name = "Bia", email = "bia@fcg.com", password = "Senha@123" })).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict,
            (await anon.PostAsJsonAsync("/api/auth/register", new { name = "Bia", email = "bia@fcg.com", password = "Senha@123" })).StatusCode);

        var user = await Login("bia@fcg.com", "Senha@123");
        var admin = await Login("admin@fcg.com", "Admin@1234");

        Assert.Equal(HttpStatusCode.Forbidden,
            (await user.PostAsJsonAsync("/api/games", new { title = "X", price = 10 })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await user.GetAsync("/api/users")).StatusCode);

        var created = await admin.PostAsJsonAsync("/api/games", new { title = "Zelda", genre = "Aventura", price = 200 });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var gameId = (await created.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetGuid();

        var promo = await admin.PostAsJsonAsync("/api/promotions",
            new { gameId, discountPercent = 50, startsAt = DateTime.UtcNow.AddDays(-1), endsAt = DateTime.UtcNow.AddDays(1) });
        Assert.Equal(HttpStatusCode.Created, promo.StatusCode);

        var buy = await user.PostAsync($"/api/library/{gameId}", null);
        Assert.Equal(HttpStatusCode.Created, buy.StatusCode);
        Assert.Equal(100m, (await buy.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("pricePaid").GetDecimal());
        Assert.Equal(HttpStatusCode.Conflict, (await user.PostAsync($"/api/library/{gameId}", null)).StatusCode);

        var lib = await user.GetFromJsonAsync<JsonElement>("/api/library");
        Assert.Equal(1, lib.GetArrayLength());
    }
}

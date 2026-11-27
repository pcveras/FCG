namespace FCG.Api.Domain;

public class User
{
    private User() { }

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = Roles.User;
    public List<UserGame> Library { get; private set; } = [];

    public static User Create(string name, string email, string password, IPasswordHasher hasher, string role = Roles.User)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nome é obrigatório.");
        if (!Validation.IsValidEmail(email)) throw new DomainException("E-mail inválido.");
        if (!Validation.IsStrongPassword(password))
            throw new DomainException("Senha deve ter no mínimo 8 caracteres, com letras, números e caracteres especiais.");
        if (!Roles.IsValid(role)) throw new DomainException("Perfil inválido.");

        return new User
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = hasher.Hash(password),
            Role = role
        };
    }

    public void ChangeRole(string role)
    {
        if (!Roles.IsValid(role)) throw new DomainException("Perfil inválido.");
        Role = role;
    }

    public void Update(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Nome é obrigatório.");
        if (!Validation.IsValidEmail(email)) throw new DomainException("E-mail inválido.");
        Name = name.Trim();
        Email = email.Trim().ToLowerInvariant();
    }
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

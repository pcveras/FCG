using System.Net.Mail;

namespace FCG.Api.Domain;

public static class Validation
{
    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 254) return false;
        if (!MailAddress.TryCreate(email, out var addr) || addr.Address != email) return false;
        var domain = email[(email.LastIndexOf('@') + 1)..];
        return domain.Contains('.') && !domain.StartsWith('.') && !domain.EndsWith('.');
    }

    /// <summary>Mínimo de 8 caracteres com letras, números e caracteres especiais.</summary>
    public static bool IsStrongPassword(string? password) =>
        !string.IsNullOrEmpty(password)
        && password.Length >= 8
        && password.Any(char.IsLetter)
        && password.Any(char.IsDigit)
        && password.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
}

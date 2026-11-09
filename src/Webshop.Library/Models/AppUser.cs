namespace Webshop.Library.Models;

/// <summary>Indeholder de brugeroplysninger, som webshoppen skal bruge.</summary>
public sealed class AppUser
{
    public int Id { get; init; }
    public string Email { get; init; } = "";
    public string PasswordHash { get; init; } = "";
    public string Role { get; init; } = "Customer";
    public int FailedLoginCount { get; init; }
    public DateTimeOffset? LockedUntilUtc { get; init; }
}

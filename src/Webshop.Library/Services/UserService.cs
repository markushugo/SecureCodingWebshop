using Microsoft.AspNetCore.Identity;
using Webshop.Library.Data;
using Webshop.Library.Models;

namespace Webshop.Library.Services;

/// <summary>Beskriver resultatet af et loginforsøg.</summary>
public sealed record AuthenticationResult(bool Succeeded, AppUser? User, string Message);

/// <summary>Håndterer registrering, login og skift af adgangskode.</summary>
public sealed class UserService(StoreRepository repository, IPasswordHasher<AppUser> hasher)
{
    /// <summary>Validerer oplysningerne og opretter en ny kunde.</summary>
    public async Task<IReadOnlyList<string>> RegisterAsync(string email, string password)
    {
        email = NormalizeEmail(email);
        var errors = PasswordPolicy.Validate(password).ToList();
        if (email.Length > 254 || !email.Contains('@')) errors.Add("Indtast en gyldig e-mailadresse.");
        if (errors.Count > 0) return errors;
        if (await repository.FindUserByEmailAsync(email) is not null) return ["E-mailadressen kan ikke oprettes."];
        var pending = new AppUser { Email = email };
        var hash = hasher.HashPassword(pending, password);
        try { await repository.CreateUserAsync(email, hash); }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number is 2601 or 2627) { return ["E-mailadressen kan ikke oprettes."]; }
        await repository.WriteAuditAsync(null, "UserRegistered", true);
        return [];
    }

    /// <summary>Kontrollerer login og registrerer både korrekte og fejlede forsøg.</summary>
    public async Task<AuthenticationResult> AuthenticateAsync(string email, string password)
    {
        const string generic = "E-mail eller adgangskode er forkert.";
        email = NormalizeEmail(email);
        var user = await repository.FindUserByEmailAsync(email);
        if (user is null)
        {
            await repository.WriteAuditAsync(null, "Login", false, "Ukendt eller forkert login");
            return new(false, null, generic);
        }
        if (user.LockedUntilUtc > DateTimeOffset.UtcNow)
        {
            await repository.WriteAuditAsync(user.Id, "Login", false, "Midlertidigt spærret");
            return new(false, null, "Login er midlertidigt spærret. Prøv igen senere.");
        }
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            await repository.RegisterFailedLoginAsync(user.Id);
            await repository.WriteAuditAsync(user.Id, "Login", false, "Ukendt eller forkert login");
            return new(false, null, generic);
        }
        await repository.ResetFailedLoginAsync(user.Id);
        await repository.WriteAuditAsync(user.Id, "Login", true);
        return new(true, user, "");
    }

    /// <summary>Kontrollerer den gamle adgangskode og gemmer den nye sikkert.</summary>
    public async Task<IReadOnlyList<string>> ChangePasswordAsync(AppUser user, string currentPassword, string newPassword)
    {
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, currentPassword) == PasswordVerificationResult.Failed)
            return ["Den nuværende adgangskode er forkert."];
        var errors = PasswordPolicy.Validate(newPassword);
        if (errors.Count > 0) return errors;
        await repository.UpdatePasswordAsync(user.Id, hasher.HashPassword(user, newPassword));
        await repository.WriteAuditAsync(user.Id, "PasswordChanged", true);
        return [];
    }

    /// <summary>Finder en bruger ud fra e-mailadressen.</summary>
    public Task<AppUser?> FindAsync(string email) => repository.FindUserByEmailAsync(NormalizeEmail(email));

    /// <summary>Gør e-mailadressen ensartet før opslag og lagring.</summary>
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

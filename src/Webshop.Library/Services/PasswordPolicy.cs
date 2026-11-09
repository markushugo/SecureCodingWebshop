namespace Webshop.Library.Services;

/// <summary>Kontrollerer og vurderer styrken af adgangskoder.</summary>
public static class PasswordPolicy
{
    public const int MinimumLength = 12;

    /// <summary>Returnerer en liste med de adgangskodekrav, der ikke er opfyldt.</summary>
    public static IReadOnlyList<string> Validate(string password)
    {
        var errors = new List<string>();
        if (password.Length < MinimumLength) errors.Add($"Adgangskoden skal være mindst {MinimumLength} tegn.");
        if (!password.Any(char.IsUpper)) errors.Add("Brug mindst ét stort bogstav.");
        if (!password.Any(char.IsLower)) errors.Add("Brug mindst ét lille bogstav.");
        if (!password.Any(char.IsDigit)) errors.Add("Brug mindst ét tal.");
        if (!password.Any(ch => !char.IsLetterOrDigit(ch))) errors.Add("Brug mindst ét specialtegn.");
        return errors;
    }

    /// <summary>Beregner en enkel styrkescore fra 0 til 100.</summary>
    public static int Score(string password)
    {
        var score = Math.Min(50, password.Length * 4);
        if (password.Any(char.IsUpper)) score += 10;
        if (password.Any(char.IsLower)) score += 10;
        if (password.Any(char.IsDigit)) score += 15;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) score += 15;
        return Math.Min(100, score);
    }
}

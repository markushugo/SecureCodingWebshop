# Sikkerhedsvalg

- Passwords hashes med ASP.NET Core `PasswordHasher<TUser>` og gemmes aldrig i klartekst.
- Passwordpolitik: mindst 12 tegn samt stort/lille bogstav, tal og specialtegn. Styrkescore og validering beregnes på serveren uden JavaScript.
- Fem fejlede loginforsøg spærrer kontoen i 15 minutter. Ukendt bruger og forkert password giver samme normale fejl.
- Cookie er `HttpOnly`, `Secure`, `SameSite=Lax`, udløber efter 30 minutter og fornyes ved aktivitet.
- Razor Pages beskytter POST med antiforgery som standard. SQL er parameteriseret og typet.
- Checkout genlæser pris/lager og bruger én transaktion med låst lagerkontrol.
- Ordredetaljer filtreres med både ordre-id og bruger-id. Adminmappen kræver rollen `Admin`.
- Audit gemmer hændelsestype og resultat, men aldrig passwords, cookies eller SQL.
- Middleware sætter CSP, `nosniff` og `DENY` mod indlejring.

## Manuelle kontroller

1. Forsøg fem forkerte logins og kontroller spærring.
2. Åbn `/Admin/Products` som gæst og kunde; begge skal afvises.
3. Opret to kunder og kontroller, at ændret ordre-id ikke viser den anden kundes ordre.
4. Manipuler kurvens pris; checkout skal stadig bruge databaseprisen.
5. Forsøg checkout med større antal end lager; ingen halv ordre eller negativt lager må opstå.

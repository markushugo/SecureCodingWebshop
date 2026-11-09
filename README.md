# Secure Coding Webshop

En lille skolewebshop bygget med ASP.NET Core Razor Pages, et class library, MSSQL og ADO.NET. Projektet bruger ikke ORM, eksterne API'er eller rigtig betaling.

## Start lokalt

1. Åbn `database.sql` i SQL Server Management Studio og kør hele scriptet.
2. Sæt forbindelsen uden at gemme en hemmelighed i repositoryet:
   `dotnet user-secrets --project src/Webshop.UI set "ConnectionStrings:Webshop" "Server=(localdb)\\MSSQLLocalDB;Database=SecureCodingWebshop;Trusted_Connection=True;TrustServerCertificate=True"`
3. Kør `dotnet restore SecureCodingWebshop.slnx`.
4. Kør `dotnet run --project src/Webshop.UI`.
5. Opret en bruger gennem websiden. Hvis den skal være demo-admin, så kør den kommenterede `UPDATE` nederst i `database.sql`.

## Struktur

- `src/Webshop.UI`: Razor Pages, cookie-login og sessionkurv.
- `src/Webshop.Library`: modeller, passwordregler og ét parameteriseret ADO.NET-repository.
- `database.sql`: database, constraints og testprodukter.
- `SECURITY.md`, `THREAT_MODEL.md` og `TRACEABILITY.md`: korte projektdokumenter.

## Afgrænsninger

Checkout er kun en lokal simulation. Der er ingen mail, password recovery, MFA, betalingsudbyder eller fysisk sletning af produkter. Audit læses i SSMS.

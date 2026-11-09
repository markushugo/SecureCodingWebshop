# Kort trusselsmodel

| Risiko | Kontrol |
|---|---|
| Gættet password | Passwordhash, styrkekrav og loginspærring |
| SQL injection | Parameteriserede `SqlCommand`-kald |
| Kunde læser en andens ordre | Bruger-id fra claim indgår i SQL-filteret |
| Kunde ændrer produkter | Rollekrav på hele adminmappen |
| Manipuleret kurv/pris | Kun id/antal i session; pris/lager genlæses |
| Halv ordre/negativt lager | SQL-transaktion, `UPDLOCK` og lagerkontrol |
| XSS/CSRF/clickjacking | Razor encoding, antiforgery, CSP og `DENY` |
| Følsomme data i logs | Kontrollerede auditfelter uden hemmeligheder |

## Adgangsmatrix

| Funktion | Gæst | Kunde | Admin |
|---|---:|---:|---:|
| Produkter og kurv | Ja | Ja | Ja |
| Checkout og egne ordrer | Nej | Ja | Ja |
| Skifte eget password | Nej | Ja | Ja |
| Administrere produkter | Nej | Nej | Ja |

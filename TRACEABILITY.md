# Sporbarhed

| User story | Implementering |
|---|---|
| US1 Opret bruger | `/Account/Register`, `UserService.RegisterAsync` |
| US2 Login | `/Account/Login`, cookie og lockout |
| US3 Se produkter | `/Index`, `GetProductsAsync` |
| US4 Kurv | `/Cart`, `CartSession` |
| US5 Checkout | `/Checkout`, `CreateOrderAsync` |
| US6 Passwordændring | `/Account/ChangePassword` |
| US7 Søgning | `Index?Search=`, parameteriseret `LIKE` |
| US8 Ordrehistorik | `/Orders` og ejerskabsfilter |
| US9 Produktadmin | `/Admin/Products`, rollepolicy og audit |
| US10 Audit | `AuditEvent`, `WriteAuditAsync` |
| US11 Passwordhjælp | Serverberegnet Pass'O-score og `PasswordPolicy` |

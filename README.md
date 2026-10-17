# EverestRemit API (single project)

A .NET 8 ASP.NET Core Web API in **one project**, with separate folders for entities, DTOs, service interfaces, service implementations, controllers and database context.

## Included
- ASP.NET Core Identity with strong password rules and lockout
- JWT signing and validation (issuer, audience, signature, expiration)
- User/Admin roles; protected admin endpoints
- EF Core 8 + SQL Server + migration support
- Swagger bearer authentication, CORS allow-list and fixed-window rate limiting
- Separate models/services/interfaces/controllers for Auth, KYC, Beneficiaries, Exchange Rates, Deposits and Transfers

## Setup (PowerShell)
1. Install .NET 8 SDK and SQL Server/LocalDB.
2. Open this folder in Visual Studio and set `EverestRemit.Api` as startup project.
3. From this folder run:
   ```powershell
   dotnet restore
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Secret" "Use-a-unique-random-secret-of-at-least-32-bytes-123!"
   dotnet tool install --global dotnet-ef --version 8.*
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   dotnet run
   ```
   If `dotnet-ef` is already installed: `dotnet tool update --global dotnet-ef --version 8.*`.
4. In Visual Studio Package Manager Console, select this project as Default project and run `Add-Migration InitialCreate`, then `Update-Database`.
5. Swagger is available in Development at `/swagger`.

## Optional first admin
Configure both through User Secrets (never hardcode production credentials):
```powershell
dotnet user-secrets set "BootstrapAdmin:Email" "admin@example.com"
dotnet user-secrets set "BootstrapAdmin:Password" "A-unique-strong-password-123!"
```
At startup the API creates the Admin/User roles and bootstraps this admin if configured.

## Endpoints
- `POST /api/auth/register`, `POST /api/auth/login`
- `POST /api/kyc`, `GET /api/kyc/me` (User); `GET /api/kyc`, `PATCH /api/kyc/{id}/review` (Admin)
- `GET/POST /api/beneficiaries`, `PUT/DELETE /api/beneficiaries/{id}` (own records only)
- `GET /api/exchange-rates` (public), `POST /api/exchange-rates` (Admin)
- `GET /api/deposits/me`, `POST /api/deposits` (User); `GET /api/deposits`, `PATCH /api/deposits/{id}/review` (Admin)
- `GET /api/transfers/me`, `POST /api/transfers` (User); `GET /api/transfers`, `PATCH /api/transfers/{id}/status` (Admin)

## Important before production
This is a starter scaffold, not a certified remittance platform. It does not move money, implement double-entry ledger accounting, verify bank receipts, or provide full AML/sanctions controls. KYC image fields are URLs only; implement secure private uploads. Add email verification/password reset, audit logging, idempotency, concurrency controls, transaction limits, monitoring, automated tests, and professional security/compliance review before real funds. Keep secrets out of source control. Configure exact trusted CORS origins and production HTTPS.

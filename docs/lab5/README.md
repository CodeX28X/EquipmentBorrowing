# Lab 5 — Local setup and reproduction

Continue the original Lab 4 repository. Use .NET SDK 10.0.400 (global.json), PostgreSQL, EF CLI 10.0.12, and Windows PowerShell. Python 3 and psql are only needed for automated acceptance. No cloud or authentication is required.

## Setup
1. Create local PostgreSQL databases `equipment_borrowing_dev`, `equipment_borrowing_test`, `equipment_borrowing_lab4_verify`, and `equipment_borrowing_lab5_acceptance`. Use an authorized local role with schema permissions. Never commit passwords.
2. Configure Windows user environment variables `EQUIPMENTBORROWING_CONNECTION_STRING`, `EQUIPMENTBORROWING_TEST_CONNECTION_STRING`, `EQUIPMENTBORROWING_A06_CONNECTION_STRING` with respective local Npgsql connection strings. Format: `Host=localhost;Port=5432;Database=<database>;Username=<role>;Password=<secret>`. Reopen terminals after configuring. Use a secure prompt or local environment configuration rather than publishing values.
3. `dotnet restore`, `dotnet build`, `dotnet test`. Test database variables must refer to disposable databases. Install EF tool if needed: `dotnet tool install --global dotnet-ef --version 10.0.12`.
4. With the intended connection configured, `dotnet ef database update --project src/EquipmentBorrowing.Infrastructure --startup-project src/EquipmentBorrowing.Api`. Repeat for acceptance using `. .\scripts\lab5\Use-AcceptanceDatabase.ps1`; verify target first. Existing four migrations are retained.
5. In pgAdmin, connect explicitly to `equipment_borrowing_lab5_acceptance`, verify `select current_database();`, and execute all of `scripts/lab5/reset.sql`. This is destructive only in the disposable database. Use `state.sql` for readback. Fixture dates follow Asia/Manila; reset before each test. Never reset development data.

## Independent processes
API terminal: `dotnet run --project src/EquipmentBorrowing.Api --no-launch-profile -- --urls http://localhost:5080 --environment Development`.
Desktop terminal: `$env:EQUIPMENTBORROWING_API_URL = 'http://localhost:5080/api/v1/'`; `Remove-Item Env:EQUIPMENTBORROWING_CONNECTION_STRING -ErrorAction SilentlyContinue`; `dotnet run --project src/EquipmentBorrowing.Desktop`. The default URL is the same. API uses the database variable; the desktop does not.

`curl.exe -i http://localhost:5080/api/v1/equipment` works independently. Published contract: `/openapi/v1.json` (versioned, reviewed JSON shipped with API). Raw framework-generated document: `/openapi/generated/v1.json` for drift review; regenerate and reconcile the published contract whenever endpoints change. Both use OpenAPI 3.1. See [contract](../../src/EquipmentBorrowing.Api/openapi.lab5.json) and [traceability](Traceability.md).

## Acceptance
Close all API/desktop processes. `powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\lab5\Run-Acceptance.ps1` uses port 5081 and resets the disposable DB. It creates timestamped evidence under `docs/lab5/evidence`. Review logs for secrets before committing. T07/T08 are manual on port 5080. `fault-on.sql`/`fault-off.sql` are controlled test-only SQL scripts, not migrations; do not enable on development DB. Restore the process connection from the Windows user variable when finished.

See [baseline and architecture](Baseline.md) and [consolidated execution record](Acceptance.md). Original Lab 4 documentation is preserved.

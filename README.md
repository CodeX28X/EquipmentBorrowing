# Lab 5: API Integration and Acceptance Testing

See [Lab 5 setup and evidence](docs/lab5/README.md) for current API/desktop setup, contract, acceptance testing and traceability. The original Lab 4 README follows for historical continuity.

# Equipment Borrowing System — Laboratory Activity 4

A desktop equipment borrowing system built with **C# / .NET 10, Avalonia UI, CommunityToolkit.Mvvm, EF Core 10, Npgsql, and PostgreSQL 18**. This Lab 4 implementation extends the previous project with PostgreSQL persistence, student and equipment management, atomic multi-item checkout, borrowing histories, and borrowing search/filter/sort.

> **Current verification:** A local solution build and **90/90 automated tests passed** in the final reported local regression run, including the isolated A06 rollback integration test. The earlier shared-branch merge was `febc984`. Manual verification was reported for 21 multi-checkout checks, 10 history checks, and 16 search checks. Re-run verification on the final submission machine; results are environment-specific.

## 1. Requirements and scope

**Primary actor:** Equipment custodian / staff.

**Required use cases:** UC-01 View Equipment Catalog; UC-02 Find Available Equipment; UC-03 Borrow Equipment; UC-04 View Active Borrowings; UC-05 Return Equipment; UC-06 Check Out Multiple Equipment Items; UC-07 View Student Borrowing History; UC-08 View Equipment Borrowing History; UC-09 Search, Filter, and Sort Borrowings; UC-12 Register Student; UC-13 Update Student Information; UC-14 Change Student Borrowing Eligibility; UC-15 Register Equipment; UC-16 Update Equipment Information.

**Not required for Lab 4:** UC-10, UC-11 and UC-17–27 (deferred to Lab 5); UC-28–33 (future scope). Do not present them as required implemented workflows.

## 2. Solution architecture and current project structure

```text
Avalonia View (AXAML)
  -> ViewModel (CommunityToolkit.Mvvm)
  -> Application interfaces / service contracts
  -> EF Core repositories and PostgreSQL transaction services
  -> PostgreSQL 18
```

- `src/EquipmentBorrowing.Domain`: `Student`, `Equipment`, `Borrowing`, borrowing status, business invariants.
- `src/EquipmentBorrowing.Application`: repository interfaces, atomic operation contract, student/equipment management contracts, history/search reader contracts. Earlier service abstractions remain for compatibility and tests.
- `src/EquipmentBorrowing.Infrastructure`: EF Core DbContext, Npgsql configuration, repositories, transactions and migrations.
- `src/EquipmentBorrowing.Desktop`: Avalonia views, viewmodels, converters, navigation, and application composition in `App.axaml.cs`.
- `src/EquipmentBorrowing.Console`: earlier console application retained as project history.
- `tests/EquipmentBorrowing.Tests`: unit and PostgreSQL integration testing.

### Current solution structure

```text
EquipmentBorrowing/
├── EquipmentBorrowing.sln
├── README.md
├── docs/
│   ├── Lab4-UseCase-Diagram.png             # Lab 4 use-case diagram
│   ├── Lab4-UseCase-Catalog.md
│   ├── Lab4-Adapted-14-Case-Suite.md
│   ├── EquipmentBorrowing-AppendixC-Traceability.md
│   ├── Lab4-Verification-Evidence/
│   ├── database-diagram.png                # legacy diagram; verify before reuse
│   └── legacy-sqlite-migrations/           # retained from Activity 3
├── src/
│   ├── EquipmentBorrowing.Domain/
│   ├── EquipmentBorrowing.Application/
│   ├── EquipmentBorrowing.Infrastructure/
│   │   ├── Migrations/                    # four PostgreSQL migrations
│   │   ├── Persistence/
│   │   ├── Repositories/
│   │   └── Services/
│   ├── EquipmentBorrowing.Console/
│   └── EquipmentBorrowing.Desktop/
│       ├── ViewModels/
│       ├── Views/
│       ├── Converters/
│       ├── App.axaml.cs                    # dependency composition
│       └── MainWindow.axaml
└── tests/EquipmentBorrowing.Tests/
```

The source tree is intentionally summarized rather than listing every generated and implementation file. The root-level legacy `.db` files, if present, are not the active Lab 4 PostgreSQL database. Preserve the original README separately if a full historical archive is required.

### Dependency direction

```text
Domain (independent core)
   ↑
Application (business operations, abstractions)
   ↑
Infrastructure (implements Application contracts; references Domain)

Desktop (Avalonia composition root) → Application / Domain / Infrastructure
Console (legacy demo)            → Application / Infrastructure
Tests                            → Domain / Application / Infrastructure
```

The **Views** use **ViewModels**, not raw SQL or `DbContext`. The ViewModels consume application contracts, and concrete EF Core/Npgsql components live in Infrastructure. This extends the repository-interface and MVVM separation introduced in Activities 1 and 2.

The Desktop uses a DbContext factory; **each database operation creates its own short-lived DbContext** rather than sharing a DbContext across concurrent UI commands. Read-only repositories use asynchronous database access. PostgreSQL atomic borrowing operations use a transaction, lock and revalidate current records, and commit the selected borrowing rows and availability changes together. Unknown outcomes after an attempted commit must be verified by a fresh read rather than blindly retried.

## 3. Prerequisites

- Windows with the **.NET 10 SDK** and a compatible IDE (Visual Studio or CLI).
- **PostgreSQL 18** running locally or on an accessible server.
- `dotnet-ef` tooling compatible with the EF Core major version used in the project.
- Git (for repository workflow).

Check installation:

```powershell
dotnet --info
dotnet ef --version
psql --version
```

## 4. Configure PostgreSQL securely

Create **two separate databases** using a PostgreSQL administrator account:

```sql
create database equipment_borrowing_dev;
create database equipment_borrowing_test;
```

Use least-privilege PostgreSQL roles where practical; the migration operator needs schema-management privileges. Do not commit passwords, `.env` files, or connection strings with secrets.

At minimum, provide these **process environment variables** for the corresponding commands:

- `EQUIPMENTBORROWING_CONNECTION_STRING` → `equipment_borrowing_dev` (Desktop and EF design-time factory)
- `EQUIPMENTBORROWING_TEST_CONNECTION_STRING` → `equipment_borrowing_test` (regular integration tests)
- `EQUIPMENTBORROWING_A06_CONNECTION_STRING` → `equipment_borrowing_lab4_verify` (isolated A06 fault-injection test; disposable and migrated separately)

An Npgsql connection string uses the format `Host=localhost;Port=5432;Database=equipment_borrowing_dev;Username=YOUR_ROLE;Password=YOUR_PASSWORD`. Substitute actual credentials **locally**; this example must not be treated as a valid secret. Keep the two database targets distinct.

**PowerShell example without echoing the password:**

```powershell
$secure = Read-Host 'Development database password' -AsSecureString
$plain = [System.Net.NetworkCredential]::new('', $secure).Password
$builder = [System.Data.Common.DbConnectionStringBuilder]::new()
$builder['Host'] = 'localhost'
$builder['Port'] = 5432
$builder['Database'] = 'equipment_borrowing_dev'
$builder['Username'] = 'postgres' # replace with your database role
$builder['Password'] = $plain
$property = $builder.GetType().GetProperty("ConnectionString")
$env:EQUIPMENTBORROWING_CONNECTION_STRING = [string]$property.GetValue($builder)
Remove-Variable plain, secure
```

Repeat for the test variable with `equipment_borrowing_test`; for the isolated A06 test, use the separately migrated disposable `equipment_borrowing_lab4_verify` database. Do not point A06 at the development or ordinary test database. In Windows PowerShell 5.1, use the reflection-based property access above rather than `$builder.ConnectionString`, which can resolve unexpectedly as a dictionary key. Setting these variables in the current shell avoids writing credentials to the repository. Saving a variable permanently at user scope stores a retrievable credential; do so only on a trusted local computer.

## 5. Database migrations

The operational PostgreSQL migrations are in `src/EquipmentBorrowing.Infrastructure/Migrations/`:

1. `20261117182213_InitialPostgreSql`
2. `20261117182722_AlignEquipmentIdentifierNormalization`
3. `20261117192450_AddStudentIdSequence`
4. `20261119130134_AddEquipmentIdSequence`

**Do not use `docs/legacy-sqlite-migrations/` for PostgreSQL.** Those files are retained only as historical work from an earlier laboratory activity. Likewise, any legacy `EquipmentBorrowing.db` SQLite files are not the current operational database.

Run migrations for the development database after setting the **development** environment variable:

```powershell
dotnet ef migrations list `
  --project src/EquipmentBorrowing.Infrastructure `
  --startup-project src/EquipmentBorrowing.Infrastructure `
  --context EquipmentBorrowingDbContext

dotnet ef database update `
  --project src/EquipmentBorrowing.Infrastructure `
  --startup-project src/EquipmentBorrowing.Infrastructure `
  --context EquipmentBorrowingDbContext
```

To migrate the isolated test database, temporarily set `EQUIPMENTBORROWING_CONNECTION_STRING` to the **verified** value of `EQUIPMENTBORROWING_TEST_CONNECTION_STRING`, run the same update, and restore its original value in `finally`. Do not apply test migrations to the development database by accident.

Confirm schema through a fresh PostgreSQL connection:

```sql
select "MigrationId" from "__EFMigrationsHistory" order by "MigrationId";
select to_regclass('public."StudentIdSequence"');
select to_regclass('public."EquipmentIdSequence"');
```

Expected migration history contains all four entries. The tables are `Students`, `Equipment`, `Borrowings`, and `__EFMigrationsHistory`. There are uniqueness, relationship, date/status, and active-borrowing integrity constraints configured through EF Core migrations.

## 6. Initial demonstration data

Use **fictional test records**, created through Student Management and Equipment Management on the development database, not hardcoded production data. A useful demonstration set contains at least two eligible students, one ineligible student, four distinct equipment items, active and returned borrowings, and at least one student and item with no borrowing history. This lets the custodian demonstrate success, empty results, rejects, and history preservation.

**Do not assume automatic seeding on Desktop startup.** Existing `DatabaseSeeder.cs` or historical SQLite content does not prove that startup runs seeding. Ensure a clean migrated database can start without fixtures; populate demo records explicitly where appropriate.

## 7. Build, run, and test

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --project src/EquipmentBorrowing.Desktop
```

For integration tests, ensure that the regular **test** environment variable targets `equipment_borrowing_test`, the A06-specific variable targets `equipment_borrowing_lab4_verify`, and all four migrations have been applied to both targets before running the complete suite:

```powershell
dotnet test
git diff --check
```

Most recent reported result: **90 passed, 0 failed, 0 skipped** (including the A06 controlled rollback integration test). If integration tests report `Set EQUIPMENTBORROWING_TEST_CONNECTION_STRING`, configure that variable in the active terminal instead of editing tests or pointing them at the development database.

## 8. Main workflows

- **Equipment**: browse and locate available equipment, register items, edit descriptive information; checkout/return controls availability.
- **Borrow Equipment**: select student and one or more distinct available items, choose a common expected return date, and perform a PostgreSQL-backed checkout. The active-borrowing cap is **three** items per student.
- **Active Borrowings**: view outstanding loans and return individual items.
- **Students**: register, edit, and change borrowing eligibility. Existing loans remain returnable if eligibility is disabled.
- **Student History / Equipment History**: query both active and returned records without changing historical identifiers.
- **Search Borrowings**: case-insensitive partial search across student name, student number and equipment name; filter All/Active/Returned; sort by checkout or expected return date ascending/descending with a stable tie-breaker.

## 9. Correctness and failure handling

- Identifiers are trimmed and normalized for uniqueness in PostgreSQL; blank required fields are rejected.
- A newly registered student is eligible; new equipment starts available.
- An item may have only one active borrowing. An ineligible student cannot begin another loan but may return an existing one.
- An expected return date must be later than checkout date; returning before checkout is invalid.
- Multi-item checkout under UC-06 is **all-or-nothing** for two or more selected distinct items; no partial known-failure checkout should persist.
- Returned loans are retained for history; editing student/equipment descriptions does not rewrite linked history.
- Display distinct success, rejection, empty, loading, and error feedback; do not claim a successful commit on a communication failure with uncertain outcome.

## 10. Submission documentation and evidence

Lab 4 explicitly requires an **updated use-case diagram and catalog**, the **adapted fourteen-case suite**, a **use-case-to-acceptance evidence matrix**, and **Appendix C A01–A19** evidence, including both positive and negative outcomes, fresh-connection PostgreSQL readback, restart persistence, controlled rollback after a write attempt, and a clean disposable database migration demonstration. Automated tests complement but do not replace these artifacts.

Each partner submits individually and supplies **both partner full names and student IDs** in their own submission. Do not publish student IDs in the public README if individual/private submission fields are available.

### Documentation and evidence index

- [Updated use-case diagram](docs/Lab4-UseCase-Diagram.png) ([editable PlantUML](docs/Lab4-UseCase-Diagram.puml))
- [Use-case catalog](docs/Lab4-UseCase-Catalog.md) — 14 required cases within the instructor-defined full catalog
- [Adapted fourteen-case specifications](docs/Lab4-Adapted-14-Case-Suite.md)
- [Appendix C traceability and evidence status](docs/EquipmentBorrowing-AppendixC-Traceability.md)
- `docs/Lab4-Verification-Evidence/` — saved evidence logs: `A06-Controlled-Rollback.txt`, `A17-Schema-Verification.txt`, `A17-Integrity-Tests.txt`, `A18-Failure-Handling.txt`, `A18-Recovery.txt`, `Final-90-Test-Results.txt`, and `Git-History.txt`.

**Verified evidence (reported local execution):** the A06 isolated controlled rollback test passed; A17 applied four migrations to a disposable PostgreSQL database and rejected five invalid writes with `ROLLBACK`; A18 known database-unavailable and subsequent normal checkout/return tests passed; the complete suite passed 90/90. **Not yet independently demonstrated:** lost acknowledgment during commit. The operation must be treated as *unknown*, not automatically retried; inspect persisted records through a fresh connection first. Other Appendix C rows requiring UI screenshots, restart demonstrations, or per-case database comparisons remain marked pending until those artifacts are actually collected.

## 11. Project development history (Activities 1–4)

This repository builds on successive coursework iterations. The sections below document that progression **historically**; descriptions of older storage and older service wiring should not be used to operate the current Lab 4 desktop application.

| Activity | Historical focus | Retained contribution | Current Lab 4 status |
|---|---|---|---|
| **1** | Layered solution, Domain entities, repository interfaces, original borrowing/return services, console demonstration, in-memory repositories | Core separation of concerns; original domain and application abstractions | The original classes remain in the solution, but PostgreSQL transaction services handle operational desktop checkout/return |
| **2** | Avalonia MVVM desktop, equipment catalog, borrowing and returning, navigation and user feedback | Views, ViewModels, `CommunityToolkit.Mvvm`, `MainWindow`, and composition pattern | Expanded to seven functional UI screens: Equipment, Borrow, Active Borrowings, Student Management, Student History, Equipment History, Borrowing Search |
| **3** | SQLite persistence through EF Core, three entity tables, schema migrations and read-only queries | EF Core configuration patterns, relationship constraints, repository design | SQLite migration files are archived in `docs/legacy-sqlite-migrations/`; not used for current PostgreSQL database |
| **4** | PostgreSQL transaction correctness, student/equipment management, atomic multi-item checkout, histories, search/filter/sort | Four active PostgreSQL migrations, transactional services, extended UI, tests | Current operational implementation and submission scope |

### Historical Activity 3 SQLite data and demonstration

The earlier README described demo students **Juan Dela Cruz, Maria Santos, Pedro Reyes, and Ana Garcia**; demo equipment **Laptop 01, Laptop 02, Projector 01, and Camera 01**; and a sample active borrowing. Those are **historical Activity 3 seed examples**, not a promise that the current PostgreSQL desktop seeds these rows automatically. Use fictitious test data and verify it against a fresh PostgreSQL connection when preparing Lab 4 evidence.

The earlier README also referenced `docs/database-queries.sql` for SQL exercises. That file is **not present in the latest reported file inventory**. Do not include it as an existing Lab 4 deliverable unless it is restored and reviewed. Likewise, the older `docs/database-diagram.png` requires verification against the PostgreSQL schema.

### Historical test progression

The former README documented **41/41 passing tests** for the earlier solution. After the shared-branch merge, **89/89 tests** were reported; following addition of the separate A06 controlled-rollback integration test, the most recent full run passed **90/90 tests**. Manual checks were reported separately: **21 multi-item checkout**, **10 history**, and **16 search/filter/sort**. Earlier counts are retained only as historical milestones, not as current results.

## 12. Architecture reflections from Activities 1–3 (historical record)

The following reflection questions and answers are retained from the earlier README, with minor grammar and clarity edits. References to *future* SQLite or Avalonia implementations reflect the time the answers were originally written; those technologies were subsequently incorporated in Activities 2–3, and PostgreSQL became the active storage provider in Activity 4.

### Activity 1 — Architectural reflection

#### 1. Why should the application service depend on a repository interface instead of directly depending on a database implementation?

Application service should depend on a repository interface so that the business logic is separate and not tied to a specific database or storage technology. This allows different repositories to be implemented in the future without breaking the application logic, such as the current in-memory repositories or a later SQLite implementation, to be used without changing the application service because the repository will handle that.

#### 2. Which parts of your current solution could remain unchanged if SQLite were introduced at that stage?

The Domain entities and their business rules could remain unchanged. The Application services and repository interfaces could also remain unchanged. New SQLite-based repository implementations could be added to the Infrastructure layer to provide persistent storage. Because the Infrastructure will be the one that will handle the database, it will also be the one that will be changed if SQLite will be added later

#### 3. Which project would eventually contain Avalonia Views?

The presentation/UI project was planned to contain Avalonia Views in the next activity. At that stage, the Console project served as the minimal presentation layer for the demonstration of the basic requirements of the system.

#### 4. Should an Avalonia button directly execute database queries? Why or why not?

An Avalonia button should not directly execute database queries, it should only trigger an application use case or application service. This would keep the user interface be separated from business logic and data-access concerns, and preserves the layered architecture.

#### 5. What part of your implementation represents the actual business operation requested by the actor?

The part of the implementation that represents the actual business operation requested by the actor which are the students, is the BorrowEquipmentService.BorrowAsync() method. Its purpose is to coordinate validation, repository access, borrowing creation, and to change the equipment state required to complete the borrowing operation.

### Activity 2 — Architectural reflection

#### 1. Why should the View not call a repository directly?

The View shouldn’t call a repository directly because its main purpose is to display the UI and handle user interaction. Calling repositories directly would mix the presentation layer with the data-access layer. This would make the application harder to maintain and test. The View should communicate with the ViewModel instead, which handles the interaction with the application layer.

#### 2. Why should business rules not be implemented in the ViewModel?

Business rules should not be implemented in the ViewModel because they do not belong there. The ViewModel should mainly handle presentation state, user input, commands, and user-facing messages. Keeping the rules in the Application layer allows the same borrowing logic to be reused by different interfaces. It also prevents the ViewModel from becoming too complicated and tightly coupled to the business logic.

#### 3. What is the responsibility of the ViewModel?

The ViewModel connects the View to the application's functionality while keeping the UI separate from the business logic. It manages presentation state such as selected students, selected equipment, observable collections, and status messages. It will also handle commands that are triggered by the user and calls the appropriate application services based on what the user intends. This allows the View to focus mainly on layout, controls, and data binding.

#### 4. Why can the existing Application layer work without knowing that Avalonia is being used?

The existing Application layer is independent of Avalonia because it uses repository abstractions and business operations instead of UI code. Since Avalonia is merely a component of the presentation layer, the desktop interface can make use of the current application services.

#### 5. What advantage is gained from registering dependencies in one composition point?

The advantage gained from registering dependencies in one composition point is that managing and replacing dependencies is simpler by centralizing object creation. In order to maintain the application state, this project also makes sure that shared in-memory repository instances are used throughout.

#### 6. If the in-memory repository were replaced by SQLite later, which parts of the current interface should remain largely unchanged?

The parts of the current interface that should remain largely unchanged are ViewModels, Views, repository interfaces, Domain entities, and Application services. The primary modification would be in the Infrastructure layer, where SQLite-based implementations would take the place of the in-memory repository implementations

### Activity 3 — Architectural reflection

#### 1. Why did the application not need to be completely rewritten when SQLite was introduced?

Because the app was already set up with Clean Architecture and the Dependency Inversion Principle. The core business logic and ViewModels only care about repository interfaces (IStudentRepository, IEquipmentRepository, IBorrowingRepository), not how data is actually stored. Swapping the in-memory lists for SQLite was just a matter of swapping out the implementation inside the Infrastructure layer.

#### 2. Why should the ViewModel not use DbContext directly?

ViewModels are meant to handle UI logic, not query a database. If you put DbContext directly inside a ViewModel, you end up tying your presentation layer straight to Entity Framework. That makes the code messier, ruins the separation of concerns, and makes unit testing the ViewModels almost impossible without spinning up a full database.

#### 3. What responsibility does the repository implementation now perform?

It acts as the middleman between the application layer and the database. It takes standard repository method calls, turns them into EF Core LINQ queries, runs them through the EquipmentBorrowingDbContext, and hands back clean domain entities to the application.

#### 4. What is the purpose of an EF Core migration?

It acts like version control for your database schema. Instead of manually creating tables or running raw SQL scripts, migrations keep track of code-first model changes and automatically create or update the SQLite database tables so everything stays in sync.

#### 5.  Why are foreign keys important in the borrowing database?

They keep your data clean and valid (referential integrity). Having StudentId and EquipmentId as foreign keys in the Borrowings table makes sure nobody can create a borrowing record for a student or equipment item that doesn't actually exist in the database.

#### 6.  Why can a read-only query benefit from AsNoTracking()?

By default, EF Core tracks every entity it fetches so it can detect changes later. When you're just fetching data to display on a screen, turning on AsNoTracking() tells EF Core to skip tracking those objects. That saves memory, lowers CPU overhead, and speeds up the query.

#### 7.  What would happen to the rest of the application if the SQLite implementation were replaced later by another database provider?

Most of the interface and core business abstractions can remain unchanged. You’d keep all your Views, ViewModels, Domain entities, and Application services untouched. Provider-specific queries, transaction behavior, infrastructure configuration, and migrations may still need to change, as the PostgreSQL transition demonstrated.

---

**Documentation verification:** The newest local run reported **90/90 passing tests**; test logs and PostgreSQL checks were captured in `docs/Lab4-Verification-Evidence/`. These results do not by themselves establish that every Appendix C UI screenshot, restart-persistence check, and expected-versus-observed record has been collected. The uncertain-commit network scenario has been reviewed in code but not reproduced by fault injection. Confirm remaining manual evidence and final Git branch state before submission.

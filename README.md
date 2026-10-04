# Equipment Borrowing System

## Overview

TA desktop application for managing student equipment borrowing and returning. The application was developed using C#, .NET 10, Avalonia UI, and follows a layered architecture with Domain, Application, Infrastructure, and Desktop projects.
## Solution Structure

```text
EquipmentBorrowing/
│
├── EquipmentBorrowing.sln
├── README.md
│
├── src/
│   ├── EquipmentBorrowing.Domain/
│   │   ├── Entities/
│   │   │   ├── Borrowing.cs
│   │   │   ├── Equipment.cs
│   │   │   └── Student.cs
│   │   │
│   │   └── Enums/
│   │       └── BorrowingStatus.cs
│   │
│   ├── EquipmentBorrowing.Application/
│   │   ├── IBorrowingRepository.cs
│   │   ├── IEquipmentRepository.cs
│   │   ├── IStudentRepository.cs
│   │   │
│   │   └── Services/
│   │       ├── BorrowEquipmentService.cs
│   │       ├── BorrowResult.cs
│   │       ├── ReturnEquipmentService.cs
│   │       └── ReturnResult.cs
│   │
│   ├── EquipmentBorrowing.Infrastructure/
│   │   ├── Migrations/
│   │   │   ├── 20261003141009_InitialCreate.cs
│   │   │   ├── 20261003141009_InitialCreate.Designer.cs
│   │   │   └── EquipmentBorrowingDbContextModelSnapshot.cs
│   │   ├── Persistence/
│   │   │   ├── Configurations/
│   │   │   │   ├── BorrowingConfiguration.cs
│   │   │   │   ├── EquipmentConfiguration.cs
│   │   │   │   └── StudentConfiguration.cs
│   │   │   ├── DatabaseSeeder.cs
│   │   │   ├── EquipmentBorrowingDbContext.cs
│   │   │   └── EquipmentBorrowingDbContextFactory.cs
│   │   ├── Repositories/
│   │   │   ├── EfBorrowingRepository.cs
│   │   │   ├── EfEquipmentRepository.cs
│   │   │   ├── EfStudentRepository.cs
│   │   │   ├── InMemoryBorrowingRepository.cs
│   │   │   ├── InMemoryEquipmentRepository.cs
│   │   │   └── InMemoryStudentRepository.cs
│   │   └── EquipmentBorrowing.Infrastructure.csproj
│   │
│   ├── EquipmentBorrowing.Console/
│   │   ├── EquipmentBorrowing.Console.csproj
│   │   └── Program.cs
│   │
│   └── EquipmentBorrowing.Desktop/
│       ├── Converters/
│       │   ├── AvailabilityConverter.cs
│       │   └── EquipmentDisplayConverter.cs
│       │
│       ├── ViewModels/
│       │   ├── ActiveBorrowingsViewModel.cs
│       │   ├── BorrowViewModel.cs
│       │   └── EquipmentViewModel.cs
│       │
│       ├── Views/
│       │   ├── ActiveBorrowingsView.axaml
│       │   ├── ActiveBorrowingsView.axaml.cs
│       │   ├── BorrowView.axaml
│       │   ├── BorrowView.axaml.cs
│       │   ├── EquipmentView.axaml
│       │   └── EquipmentView.axaml.cs
│       │
│       ├── App.axaml
│       ├── App.axaml.cs
│       ├── MainWindow.axaml
│       ├── MainWindow.axaml.cs
│       ├── Program.cs
│       └── EquipmentBorrowing.Desktop.csproj
│
└── tests/
    └── EquipmentBorrowing.Tests/
        ├── Domain/
        ├── Application/
        └── Infrastructure/
```

### Domain

The Domain layer contains the core business entities and rules of the system.

It contains:

* `Student`
* `Equipment`
* `Borrowing`
* `BorrowingStatus`

The Domain layer has no dependency on the Application, Infrastructure, or Desktop layers.

### Application

The Application layer contains the application's use cases and repository abstractions.

It contains:

* `BorrowEquipmentService`
* `BorrowResult`
* `ReturnEquipmentService`
* `ReturnResult`
* `IStudentRepository`
* `IEquipmentRepository`
* `IBorrowingRepository`

Application services coordinate the system's business operations using repository interfaces rather than concrete infrastructure implementations.

### Infrastructure

Activity 3 replaced the previous in-memory storage with SQLite using Entity Framework Core.

The database contains three main tables:

STUDENTS
- StudentId (PK)
- StudentName
- StudentYear
- IsAllowedToBorrow

EQUIPMENT
- EquipmentId (PK)
- EquipmentName
- IsAvailable

BORROWINGS
- BorrowingId (PK)
- StudentId (FK)
- EquipmentId (FK)
- BorrowedAt
- ExpectedReturnDate
- Status

Relationships:

STUDENTS 1 -------- * BORROWINGS * -------- 1 EQUIPMENT

Entity configurations define the primary keys, required fields, unique indexes, foreign keys, and indexes.

Entity Framework Core

The project uses:

EquipmentBorrowingDbContext
StudentConfiguration
EquipmentConfiguration
BorrowingConfiguration
EfStudentRepository
EfEquipmentRepository
EfBorrowingRepository

The repositories use asynchronous EF Core operations such as ToListAsync(), FirstOrDefaultAsync(), CountAsync(), and SaveChangesAsync().

Display-only queries use AsNoTracking() where entity modification is not required.

Database Migration

The initial migration is:

InitialCreate

The migration creates the Students, Equipment, and Borrowings tables together with their keys, relationships, and indexes.

The application applies existing migrations during startup and checks whether seed records already exist before inserting them.

Seed Data

The application contains initial student and equipment records for testing.

Students:

Juan Dela Cruz
Maria Santos
Pedro Reyes
Ana Garcia

Equipment:

Laptop 01
Laptop 02
Projector 01
Camera 01

An initial active borrowing is also created for testing the unavailable equipment scenario.

Database Queries

Required SQL examples are stored in:

docs/database-queries.sql

The file contains queries for:

Retrieving students
Retrieving equipment
Filtering available equipment
Joining active borrowings with students and equipment
Counting active borrowings
Updating equipment availability
Retrieving a student's borrowing records

The Infrastructure layer depends on the Application and Domain layers.

### Desktop

The Desktop project provides the Avalonia UI presentation layer.

It contains:

* `Views` for the Avalonia user interface
* `ViewModels` for presentation state and commands
* `Converters` for UI-specific value formatting
* `App.axaml` and `App.axaml.cs` for application resources and composition
* `MainWindow` for the main application window and navigation

The Desktop project uses **CommunityToolkit.Mvvm** for observable properties, observable collections, and commands.

The Desktop project receives its application dependencies through constructor injection and the application composition root.

### Console

The Console project provides a minimal executable demonstration of the application's original use case flow.

It composes the Application services with the Infrastructure repository implementations and demonstrates equipment borrowing requests.

### Tests

The Tests project contains automated tests for the Domain, Application, and Infrastructure behavior.

The test suite uses xUnit to verify the implemented business rules, application service behavior, and repository implementations.

## 2. Dependency Direction

The solution follows a layered dependency structure where application logic depends on abstractions rather than concrete infrastructure implementations.

```text
                         Desktop
                       Presentation
                           │
                           ▼
                     Application
                    │           │
                    ▼           │
                  Domain        │
                    ▲           │
                    │           ▼
              Infrastructure
```

The actual project relationships are:

```text
Domain
  ↑
Application
  ↑
Infrastructure

Desktop
  ├──→ Application
  ├──→ Domain
  └──→ Infrastructure

Console
  ├──→ Application
  └──→ Infrastructure

Tests
  ├──→ Domain
  ├──→ Application
  └──→ Infrastructure
```

The dependency direction keeps the core business logic independent of presentation and storage concerns.

The Application layer depends on repository interfaces such as `IStudentRepository`, `IEquipmentRepository`, and `IBorrowingRepository`. The Infrastructure layer provides the concrete in-memory implementations of these interfaces.

The Desktop project acts as the composition root for the graphical application. It creates the shared repository instances and application services, then supplies those dependencies to the ViewModels.

This allows the presentation layer to use the existing Application and Domain logic without duplicating the business rules.

## 3. Desktop Project and MVVM

The Desktop project adds the Avalonia presentation layer without replacing the existing Domain, Application, or Infrastructure layers.

The main UI areas are:

* **Equipment** — displays equipment ID, name, and availability.
* **Borrow Equipment** — allows a student, equipment item, and expected return date to be selected.
* **Active Borrowings** — displays current borrowing records and provides the Return Equipment action.

The presentation layer follows the MVVM pattern:

```text
View
  ↓
Command / Binding
  ↓
ViewModel
  ↓
Application Service
  ↓
Repository Interface
  ↓
Infrastructure Repository
```

For equipment listing:

```text
EquipmentView
    ↓
EquipmentViewModel
    ↓
IEquipmentRepository
    ↓
EfEquipmentRepository
```

For borrowing:

```text
BorrowView
    ↓
BorrowViewModel
    ↓
BorrowEquipmentService
    ↓
IStudentRepository
IEquipmentRepository
IBorrowingRepository
    ↓
Ef repositories
```

For returning equipment:

```text
ActiveBorrowingsView
    ↓
ActiveBorrowingsViewModel
    ↓
ReturnEquipmentService
    ↓
IBorrowingRepository
    ↓
EfBorrowingRepository
```

The Views do not access repositories directly, and business rules remain in the Domain and Application layers.

## 4. Equipment Listing

The Equipment section provides a graphical list of the equipment available in the system.

Each equipment item displays:

* Equipment ID
* Equipment name
* Availability status

The equipment collection is provided through an `ObservableCollection<Equipment>` in `EquipmentViewModel`.

The View uses XAML data binding and an `ItemsControl` to display the collection rather than manually creating a separate UI control for each equipment item.

The application also uses an `AvailabilityConverter` to display user-friendly availability values:

```text
true  → Available
false → Unavailable
```

The equipment list is automatically loaded when the view is displayed and can also be refreshed through the ViewModel's load command.

## 5. Borrow Equipment Flow

The Desktop application provides a graphical flow for borrowing equipment.

```text
User selects student
        ↓
User selects equipment
        ↓
User selects expected return date
        ↓
Borrow Equipment command
        ↓
BorrowViewModel
        ↓
BorrowEquipmentService
        ↓
Application validation
        ↓
Repository access
        ↓
Borrowing created
        ↓
Equipment marked unavailable
        ↓
BorrowResult returned
        ↓
ViewModel displays feedback
        ↓
Updated data can be refreshed in the interface
```

The `BorrowEquipmentService` remains responsible for the business validation involved in borrowing equipment.

The service:

1. Finds the student.
2. Checks whether the student is allowed to borrow.
3. Checks the student's active borrowing count.
4. Finds the requested equipment.
5. Checks equipment availability.
6. Creates a `Borrowing` domain object.
7. Marks the equipment as borrowed.
8. Adds the borrowing through the repository.
9. Returns a `BorrowResult`.

The ViewModel performs only presentation-level checks, such as requiring the student, equipment, and expected return date to be selected before invoking the service.

Business rules are not duplicated in the ViewModel.

## 6. Return Equipment Flow

The Active Borrowings section displays the currently active borrowing records.

Each active borrowing displays information including:

* Student
* Equipment
* Borrowed date
* Expected return date
* Borrowing status
* Return Equipment action

The return flow is:

```text
User selects Return Equipment
        ↓
ActiveBorrowingsView
        ↓
ActiveBorrowingsViewModel
        ↓
ReturnEquipmentService
        ↓
Find active borrowing
        ↓
Borrowing marked as Returned
        ↓
Equipment marked as Available
        ↓
ReturnResult returned
        ↓
ViewModel displays feedback
        ↓
Active Borrowings list refreshes
```

`ReturnEquipmentService` locates the active borrowing using the student and equipment identifiers.

When a valid active borrowing is found:

1. The borrowing is marked as returned through the Domain entity.
2. The equipment is marked as available.
3. A successful `ReturnResult` is returned.
4. The ViewModel reloads the active borrowing collection.

If no active borrowing is found, the service returns a failure result instead of allowing the operation to continue.

The ViewModel does not reproduce these business rules.

## 7. Navigation and Shared State

The Desktop application provides navigation between:

* Equipment
* Borrow Equipment
* Active Borrowings

The main window provides the navigation controls and displays the selected view.

The same repository instances are shared by the application's composition root. This is important because borrowing and returning must affect the same in-memory application state.

For example:

```text
Shared Equipment Repository
        │
        ├── Equipment ViewModel
        │
        └── BorrowEquipmentService

Shared Borrowing Repository
        │
        ├── BorrowEquipmentService
        │
        ├── ReturnEquipmentService
        │
        └── ActiveBorrowingsViewModel
```

This prevents separate repository instances from maintaining different states when navigating between the application's views.

For example, when equipment is borrowed:

```text
Equipment
Available
    ↓
Borrow operation
    ↓
Equipment
Unavailable
```

After the equipment is returned:

```text
Equipment
Unavailable
    ↓
Return operation
    ↓
Equipment
Available
```

## 8. Validation and Feedback

The application keeps validation responsibilities in the appropriate layer.

### Presentation-level validation

The ViewModels check for missing required selections before calling the application service.

Examples include:

* No student selected
* No equipment selected
* No expected return date selected

These checks prevent the application service from being called with incomplete user input.

### Application and Domain validation

The Application service and Domain objects handle business rules such as:

* Student does not exist
* Student is not allowed to borrow
* Equipment does not exist
* Equipment is unavailable
* Maximum active borrowing limit is reached
* Expected return date is invalid
* No active borrowing exists for a return request
* An already returned borrowing cannot be returned again

The application returns `BorrowResult` or `ReturnResult` objects containing the outcome and message.

The ViewModels display these results to the user through the interface.

## 9. Use Case Mapping

### Borrow Equipment

```text
Actor:
Student

Use Case:
Borrow equipment

Application Service:
BorrowEquipmentService

Domain Objects Used:
- Student
- Equipment
- Borrowing

Repository Interfaces Used:
- IStudentRepository
- IEquipmentRepository
- IBorrowingRepository

Infrastructure Implementations Used:
- EfStudentRepository
- EfEquipmentRepository
- EfBorrowingRepository
```

The `BorrowEquipmentService.BorrowAsync()` method coordinates the equipment borrowing operation.

### Return Equipment

```text
Actor:
Student / Equipment administrator

Use Case:
Return equipment

Application Service:
ReturnEquipmentService

Domain Objects Used:
- Borrowing
- Equipment

Repository Interface Used:
- IBorrowingRepository

Infrastructure Implementation Used:
- EfBorrowingRepository
```

The `ReturnEquipmentService.ReturnAsync()` method coordinates the equipment return operation.

## 10. Application Flow Demonstration

The Console application demonstrates the original application flow.

### Successful Case

```text
Student requests available equipment
                ↓
      BorrowEquipmentService
                ↓
     Application validates request
                ↓
      Repository interfaces
                ↓
       Domain objects and rules
                ↓
         Borrowing created
                ↓
       Operation succeeds
                ↓
         Console displays result
```

The Desktop application provides the graphical presentation of the same application use case.

### Failure Case

The application can reject an unsuccessful borrowing request when a business rule is violated, such as attempting to borrow equipment that is already unavailable.

```text
Second Borrowing Attempt

Success: False
Message: Equipment is currently unavailable.
```

The Desktop application also displays validation and business-rule feedback when an operation cannot be completed.

## 11. Testing

The system includes automated tests covering the Domain, Application, and Infrastructure behavior.

The current test suite contains:

```text
41 tests
41 succeeded
0 failed
0 skipped
```

Tests verify behavior such as:

* Student validation
* Equipment validation
* Equipment availability changes
* Borrowing creation
* Borrowing status changes
* Repository operations
* Student borrowing restrictions
* Equipment availability checks
* Maximum active borrowing limits
* Successful borrowing operations
* Failed borrowing operations
* Equipment return behavior

The solution can be verified using:

```powershell
dotnet build
dotnet test
```

The final verification completed successfully with all **41 tests passing** and the solution building without errors or warnings.

The Desktop application was also manually verified for the main Activity 2 workflows, including:

* Equipment listing
* Equipment availability display
* Student selection
* Equipment selection
* Expected return date selection
* Successful equipment borrowing
* Active borrowing display
* Successful equipment return
* Equipment becoming available after return
* Re-borrowing returned equipment
* Validation and business-rule feedback
* Navigation between the main application areas

## 12. Activity 1 Architectural Reflection

### 1. Why should the application service depend on a repository interface instead of directly depending on a database implementation?

Application service should depend on a repository interface so that it the business logic is separate and not tied to a specific database or storage technology. This allows different repositories to be mplementations in the future without breaking the application logic, such as the current in-memory repositories or a future SQLite implementation, to be used without changing the application service because the repository will handle that.

### 2. Which parts of your current solution could remain unchanged if SQLite were added later?

The Domain entities and their business rules could remain unchanged. The Application services and repository interfaces could also remain unchanged. New SQLite-based repository implementations could be added to the Infrastructure layer to provide persistent storage. Because the Infrastructure will be the one that will handle the database, it will also be the one that will be changed if SQLite will be added later

### 3. Which project would eventually contain Avalonia Views?

The presentation/UI project that would eventually contain Avalonia Views will be a future Avalonia project. The current Console project in this program serves as the minimal presentation layer for the demonstration of the basic requirements of the system.

### 4. Should an Avalonia button directly execute database queries? Why or why not?

An Avalonia button should not directly execute database queries, it should only trigger an application use case or application service. This would keep the user interface be separated from business logic and data-access concerns, and preservers the layered architecture.

### 5. What part of your implementation represents the actual business operation requested by the actor?

The part of the implementation that represents the actual business operation requested by the actor which are the students, is the BorrowEquipmentService.BorrowAsync() method. Its purpose is to coordinate validation, repository access, borrowing creation, and to change the equipment state required to complete the borrowing operation.

## 13. Activity 2 Architectural Reflection

Answer briefly:

### 1. Why should the View not call a repository directly?

The View shouldn’t call a repository directly because its main purpose is to display the UI and handle user interaction. Calling repositories directly would mix the presentation layer with the data-access layer. This would make the application harder to maintain and test. The View should communicate with the ViewModel instead, which handles the interaction with the application layer.

### 2. Why should business rules not be implemented in the ViewModel?

Business rules should not be implemented in the ViewModel because they do not belong there. The ViewModel should mainly handle presentation state, user input, commands, and user-facing messages. Keeping the rules in the Application layer allows the same borrowing logic to be reused by different interfaces. It also prevents the ViewModel from becoming too complicated and tightly coupled to the business logic.

### 3. What is the responsibility of the ViewModel?

The ViewModel connects the View to the application's functionality while keeping the UI separate from the business logic. It manages presentation state such as selected students, selected equipment, observable collections, and status messages. It will also handle commands that are triggered by the user and calls the appropriate application services based on what the user intends. This allows the View to focus mainly on layout, controls, and data binding.

### 4. Why can the existing Application layer work without knowing that Avalonia is being used?\

The existing Application layer is independent of Avalonia because it uses repository abstractions and business operations instead of UI code. Since Avalonia is merely a component of the presentation layer, the desktop interface can make use of the current application services.

### 5. What advantage is gained from registering dependencies in one composition point?

The advantage gained from registering dependencies in one composition point is managing and replacing dependencies is made simpler by centralizing object creation. In order to maintain the application state, this project also makes sure that shared in-memory repository instances are used throughout.

### 6. If the in-memory repository were replaced by SQLite later, which parts of the current interface should remain largely unchanged?

The parts of the current interface that should remain largely unchanged are ViewModels, Views, repository interfaces, Domain entities, and Application services.The primary modification would be in the Infrastructure layer, where SQLite-based implementations would take the place of the in-memory repository implementations

### 13. Activity 3 Architectural Reflection

### 1. Why did the application not need to be completely rewritten when SQLite was introduced?

Because the app was already set up with Clean Architecture and the Dependency Inversion Principle. The core business logic and ViewModels only care about repository interfaces (IStudentRepository, IEquipmentRepository, IBorrowingRepository), not how data is actually stored. Swapping the in-memory lists for SQLite was just a matter of swapping out the implementation inside the Infrastructure layer.

### 2. Why should the ViewModel not use DbContext directly?

ViewModels are meant to handle UI logic, not query a database. If you put DbContext directly inside a ViewModel, you end up tying your presentation layer straight to Entity Framework. That makes the code messier, ruins the separation of concerns, and makes unit testing the ViewModels almost impossible without spinning up a full database.

### 3. What responsibility does the repository implementation now perform?

It acts as the middleman between the application layer and the database. It takes standard repository method calls, turns them into EF Core LINQ queries, runs them through the EquipmentBorrowingDbContext, and hands back clean domain entities to the application.

### 4. What is the purpose of an EF Core migration?

It acts like version control for your database schema. Instead of manually creating tables or running raw SQL scripts, migrations keep track of code-first model changes and automatically create or update the SQLite database tables so everything stays in sync.

### 5.  Why are foreign keys important in the borrowing database?

They keep your data clean and valid (referential integrity). Having StudentId and EquipmentId as foreign keys in the Borrowings table makes sure nobody can create a borrowing record for a student or equipment item that doesn't actually exist in the database.

### 6.  Why can a read-only query benefit from AsNoTracking()?

By default, EF Core tracks every entity it fetches so it can detect changes later. When you're just fetching data to display on a screen, turning on AsNoTracking() tells EF Core to skip tracking those objects. That saves memory, lowers CPU overhead, and speeds up the query.

### 7.  What would happen to the rest of the application if the SQLite implementation were replaced later by another database provider?

Practically nothing outside of Infrastructure. You’d keep all your Views, ViewModels, Domain entities, and Application services untouched. You’d only need to swap the database provider package in the Infrastructure layer (like switching .UseSqlite() to PostgreSQL or SQL Server) and run fresh migrations.

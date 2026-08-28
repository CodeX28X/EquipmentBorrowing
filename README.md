# Equipment Borrowing System

## Overview

The Equipment Borrowing System is a software application for managing the borrowing of campus equipment by students.

The system is implemented using a layered architecture that separates domain logic, application use cases, and infrastructure concerns. The current implementation includes domain entities and business rules, repository abstractions, application services, in-memory repository implementations, a console demonstration application, and automated tests.

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
│   │       └── BorrowResult.cs
│   │
│   ├── EquipmentBorrowing.Infrastructure/
│   │   └── Repositories/
│   │       ├── InMemoryBorrowingRepository.cs
│   │       ├── InMemoryEquipmentRepository.cs
│   │       └── InMemoryStudentRepository.cs
│   │
│   └── EquipmentBorrowing.Console/
│       ├── EquipmentBorrowing.Console.csproj
│       └── Program.cs
│
└── tests/
    └── EquipmentBorrowing.Tests/
        ├── Domain/
        ├── Application/
        └── Infrastructure/
````

### Domain

The Domain layer contains the core business entities and rules of the system.

It contains:

* `Student`
* `Equipment`
* `Borrowing`
* `BorrowingStatus`

The Domain layer has no project-to-project dependencies and does not depend on the Application or Infrastructure layers.

### Application

The Application layer contains the application's use cases and repository abstractions.

It contains:

* `BorrowEquipmentService`
* `BorrowResult`
* `IStudentRepository`
* `IEquipmentRepository`
* `IBorrowingRepository`

The Application layer depends only on the Domain layer. Application services use repository interfaces rather than depending directly on database or infrastructure implementations.

### Infrastructure

The Infrastructure layer contains concrete implementations of the repository abstractions.

It currently provides:

* `InMemoryStudentRepository`
* `InMemoryEquipmentRepository`
* `InMemoryBorrowingRepository`

The Infrastructure layer depends on the Application and Domain layers.

### Console

The Console project provides a minimal executable demonstration of the application flow.

It composes the Application services with the Infrastructure repository implementations and demonstrates both successful and unsuccessful equipment borrowing requests.

The Console project depends on the Application and Infrastructure layers.

### Tests

The Tests project contains automated tests for the Domain, Application, and Infrastructure behavior.

The test suite uses xUnit to verify the implemented business rules, application service behavior, and repository implementations.

## 2. Dependency Direction

The solution follows a layered dependency structure where higher-level application logic depends on abstractions rather than concrete infrastructure implementations.

```text
              Console / Future UI
                      │
                      ▼
                 Application
                  │       ▲
                  ▼       │
                Domain    │
                          │
                  Infrastructure
```

The actual project references are:

```text
Domain
  ↑
Application
  ↑
Infrastructure

Console
  ├──→ Application
  └──→ Infrastructure

Tests
  ├──→ Domain
  ├──→ Application
  └──→ Infrastructure
```

The dependency direction ensures that the core business logic remains independent of external storage and presentation concerns.

The Application layer depends on repository interfaces such as `IStudentRepository`, `IEquipmentRepository`, and `IBorrowingRepository`. The Infrastructure layer provides the concrete implementations of these interfaces.

This allows the current in-memory repositories to be replaced by another persistence mechanism without requiring changes to the core business logic.

## 3. Use Case Mapping

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
- InMemoryStudentRepository
- InMemoryEquipmentRepository
- InMemoryBorrowingRepository
```

The `BorrowEquipmentService.BorrowAsync()` method coordinates the equipment borrowing operation.

The service:

1. Finds the student.
2. Checks whether the student is allowed to borrow.
3. Checks the student's active borrowing count.
4. Finds the requested equipment.
5. Checks equipment availability.
6. Creates a `Borrowing` domain object.
7. Marks the equipment as borrowed.
8. Persists the borrowing and equipment changes through the repository interfaces.
9. Returns a `BorrowResult` indicating whether the operation succeeded or failed.

## 4. Application Flow Demonstration

The Console application demonstrates the required application flow.

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

Example result:

```text
=== Equipment Borrowing System ===

Attempting to borrow equipment...
Success: True
Message: Equipment borrowed successfully.

=== Resulting State ===
Student: Juan Dela Cruz
Equipment: Laptop
Equipment Available: False
Borrowing Status: Active
```

### Failure Case

The Console application also demonstrates an unsuccessful request by attempting to borrow equipment that is already unavailable.

```text
Second Borrowing Attempt

Success: False
Message: Equipment is currently unavailable.
```

A graphical interface is not required for the demonstration. The Console application provides the minimal executable demonstration of the implemented use case.

## 5. Testing

The system includes automated tests covering the implemented Domain, Application, and Infrastructure behavior.

The current test suite contains:

```text
29 tests
29 succeeded
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

The solution can be verified using:

```powershell
dotnet build
dotnet test
```

## 6. Reflection

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

```
```

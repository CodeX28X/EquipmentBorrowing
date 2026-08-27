# Equipment Borrowing System

## Overview

The Equipment Borrowing System is a software application for managing the borrowing of campus equipment by students.

The system is implemented using a layered architecture that separates domain logic, application use cases, and infrastructure concerns. The current implementation includes domain entities and business rules, repository abstractions, application services, in-memory repository implementations, and automated tests.

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
│   └── EquipmentBorrowing.Infrastructure/
│       └── Repositories/
│           ├── InMemoryBorrowingRepository.cs
│           ├── InMemoryEquipmentRepository.cs
│           └── InMemoryStudentRepository.cs
│
└── tests/
    └── EquipmentBorrowing.Tests/
        ├── Domain/
        ├── Application/
        └── Infrastructure/
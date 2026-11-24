# Lab 5 API-to-use-case traceability

Rules, endpoints and tests follow the instructor reference matrix. Implementation names refer to `src/EquipmentBorrowing.Api/Controllers`, `src/EquipmentBorrowing.Desktop/ViewModels`, and `src/EquipmentBorrowing.Infrastructure/Services` as applicable. Automated references are original `evidence/<run>/events.jsonl` filtered by test ID; manual references are filename prefixes in `evidence/manual/`. Evidence gaps are explicit.

| UC | BR | API | Tests | Implementation | Evidence |
|---|---|---|---|---|---|
| UC-01 | BR-05–07,10,18–19 | API-01 | T01,07 | `EquipmentController.GetAll / EquipmentViewModel` | Automated T01; T07-01,02 |
| UC-02 | BR-10,18–19 | API-01 | T01,07 | `EquipmentController.GetAll / EquipmentViewModel` | Automated T01; T07-02 |
| UC-03 | BR-08–11,14–16,18–19 | API-10,13 | T04,07 | `BorrowingsController.Create/GetById / BorrowViewModel` | Automated T04; T07-05-checkout1/2 |
| UC-04 | BR-14,18–19 | API-14 | T01,06,07 | `BorrowingsController.GetAll / ActiveBorrowingsViewModel` | Automated T01, T06; T07-05-checkout2 |
| UC-05 | BR-10,13–16,18–19 | API-12,13 | T04,07 | `BorrowingsController.Return/GetById / ActiveBorrowingsViewModel` | Automated T04; T07-06 |
| UC-06 | BR-08–12,14–16,18–19 | API-11 | T05,07 | `CheckoutRequestsController.Create / PostgreSqlAtomicBorrowingService.CheckoutAsync / BorrowViewModel` | Automated T05; T05 automated; desktop multi-item not captured |
| UC-07 | BR-14,18–19 | API-02,03,15 | T01,06,07 | `StudentsController.GetAll/GetById / BorrowingHistoriesController.GetStudentHistory / BorrowingHistoryViewModel` | Automated T01, T06; T07-06 |
| UC-08 | BR-14,18–19 | API-01,07,16 | T01,06,07 | `EquipmentController.GetAll/GetById / BorrowingHistoriesController.GetEquipmentHistory / BorrowingHistoryViewModel` | Automated T01, T06; T06 automated; separate desktop history not captured |
| UC-09 | BR-17–19 | API-14 | T06,07 | `BorrowingsController.GetAll / BorrowingSearchViewModel` | Automated T06; T06 automated; Phase 5 reported smoke |
| UC-12 | BR-01–02,16,18–19 | API-04 | T02,07 | `StudentsController.Create / StudentManagementViewModel` | Automated T02; T07-03 |
| UC-13 | BR-01–03,14,16,18–19 | API-02,03,05 | T02,07 | `StudentsController.GetAll/GetById/Update / StudentManagementViewModel` | Automated T02; T07-03,07,08 |
| UC-14 | BR-04,08,13–14,18–19 | API-02,03,06 | T02,07 | `StudentsController.GetAll/GetById/SetEligibility / StudentManagementViewModel` | Automated T02; T02 automated; Phase 7A eligibility smoke |
| UC-15 | BR-05–06,10,16,18–19 | API-08 | T03,07 | `EquipmentController.Create / EquipmentViewModel` | Automated T03; T07-04 |
| UC-16 | BR-05–07,10,14,16,18–19 | API-01,07,09 | T03,07 | `EquipmentController.GetAll/GetById/Update / EquipmentViewModel` | Automated T03; T07-04,08 |

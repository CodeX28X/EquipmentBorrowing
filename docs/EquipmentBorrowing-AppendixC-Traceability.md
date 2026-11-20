# Equipment Borrowing — Lab 4 Appendix C Traceability and Evidence

**Evidence status:** Updated from the supplied worksheet using the reported local tests and captured logs on November 20, 2026. An automated passing test does not substitute for instructor-required GUI screenshots, restart demonstrations, or fresh database readbacks for other acceptance cases.

**Primary actor:** Equipment Custodian / Staff. **Scope:** fourteen required cases: UC-01–09 and UC-12–16. UC-10–11 and UC-17–27 are deferred to Lab 5; UC-28–33 are future scope.

## A01–A19 matrix

| ID | Use case / topic | Main component / coverage | Current evidence | Remaining action before claiming full Appendix C completion |
|---|---|---|---|---|
| A01 | UC-01 Catalog | `EquipmentView`, `EquipmentViewModel`, read repository | Existing desktop and manual workflow reported | Catalog screenshot with fresh-connection DB comparison; empty and load-failure states |
| A02 | UC-02 Available equipment | Equipment view / reader | Existing functionality reported | Available-only filter, no-match and load-failure screenshots/readback |
| A03 | UC-03 Single checkout | `BorrowViewModel`, `PostgreSqlAtomicBorrowingService` | Automated checkout and business-rule regression tests | Capture per-case rejected states, fresh readback and restart persistence |
| A04 | UC-04 Active borrowings | `ActiveBorrowingsViewModel`, `EfActiveBorrowingsReader` | Existing workflow reported | Joined DB query, empty and error UI evidence |
| A05 | UC-05 Return equipment | Atomic return service, Active Borrowings UI | Checkout/return integration tests passed | Fresh readback after return, restart persistence, stale/repeated return screen evidence |
| A06 | UC-06 Atomic multi-checkout | `PostgreSqlAtomicBorrowingService`, `PostgreSqlA06RollbackTests` | **Automated controlled post-write failure/rollback and recovery test passed (1/1)**; full suite 90/90 | Refer to test implementation/log; capture additional instructor-requested UI/readback screenshots if needed |
| A07 | UC-07 Student borrowing history | `BorrowingHistoryViewModel`, `EfBorrowingHistoryReader` | 10 history checks reported across UC-07/08 | Two students, empty result, stale selection, asynchronous load and SQL comparison |
| A08 | UC-08 Equipment borrowing history | History ViewModel / reader | Manual history checks reported | Two items, empty result, stale selection, asynchronous load and SQL comparison |
| A09 | UC-09 Search/filter/sort | `BorrowingSearchViewModel`, `EfBorrowingSearchReader` | 16 manual search/filter/sort checks reported | Capture query/filter/sort combinations, stable ordering DB comparison, no-match/error UI |
| A12 | UC-12 Register student | Student Management service/ViewModel | Management integration tests previously reported | Required-field, normalized uniqueness, default eligibility, restart readback |
| A13 | UC-13 Update student | `Student.UpdateInformation`, management service | StudentYear regression corrected; complete suite passed | Preserve ID, foreign keys and history; contact clearing and duplicate rejection evidence |
| A14 | UC-14 Borrowing eligibility | Management and checkout/return services | Eligibility and return tests previously reported | UI state, disable checkout, allow return, re-enable and retry evidence |
| A15 | UC-15 Register equipment | `EquipmentViewModel`, management service | Management integration/manual tests reported | Normalized duplicate rejection, availability default, restart readback |
| A16 | UC-16 Update equipment | `Equipment.UpdateInformation`, management service | Management tests reported | Edit borrowed item without changing availability/identity/history, clear description |
| A17 | Clean PostgreSQL migrations and integrity | Four EF Core migrations, DB constraints | **Verified:** clean disposable migration, four history rows, constraints/indexes/sequences; five intentionally invalid writes rejected and transaction rolled back | Keep captured SQL evidence; attach screenshot if explicitly demanded |
| A18 | Failure / recovery | DbContext factory; atomic service error classification | **Verified:** database unavailable → `Failed`; subsequent checkout+return succeeds (each 1/1). A06 also demonstrates rollback/recovery | **Unverified:** actual lost commit acknowledgment. Document `Unknown`, refresh/read before any retry; capture app-facing failure/recovery UI if required |
| A19 | Submission completeness | README, use cases, docs, evidence logs, Git history | Documentation pack and seven evidence logs assembled; 90/90 final regression | Finish remaining Appendix C manual records, confirm partner submission fields, review staged files/security, commit/push |

## Captured verification logs

All paths below are relative to `docs/Lab4-Verification-Evidence/`:

| Evidence artifact | What it supports |
|---|---|
| `A06-Controlled-Rollback.txt` | Isolated A06 integration-test result against disposable `equipment_borrowing_lab4_verify` |
| `A17-Schema-Verification.txt` | Four migrations, schema tables, constraints, indexes and sequences |
| `A17-Integrity-Tests.txt` | Five named constraint rejections, `DO`, then `ROLLBACK` |
| `A18-Failure-Handling.txt` | Checkout while database endpoint is unavailable returns `Failed` |
| `A18-Recovery.txt` | Subsequent working checkout and return persist correctly |
| `Final-90-Test-Results.txt` | Full regression suite: 90 passed, zero failed, zero skipped |
| `Git-History.txt` | Export of local Git history at the time of capture |

**A17 logging note:** PostgreSQL writes `NOTICE` messages to stderr; Windows PowerShell may decorate a redirected NOTICE with `NativeCommandError`. The saved integrity log recorded all five PASS notices, `A17: All five constraint tests passed`, `DO`, and `ROLLBACK`, and the command exited successfully. Do not misclassify such a NOTICE as a SQL constraint-test failure.

## Record template for any remaining acceptance item

- **A-ID / associated use case:**
- **Test database and fixture IDs (no secrets):**
- **Before state / DB query and observed rows:**
- **Action and expected result:**
- **Actual UI response / observed result:**
- **Fresh-connection DB readback and result:**
- **Restart / failure / empty-state observation (where requested):**
- **Screenshot, log, or commit location:**
- **Disposition:** Passed / Failed / Not demonstrated

## A06 — Controlled failure after attempted write

Use a **separately migrated disposable database** (`equipment_borrowing_lab4_verify`) configured via `EQUIPMENTBORROWING_A06_CONNECTION_STRING`, never the regular development or test database. The integration test executes actual `PostgreSqlAtomicBorrowingService.CheckoutAsync()` with two items, deliberately triggers a PostgreSQL failure after an INSERT is attempted, verifies `Failed`, opens a new DbContext to check there are no persisted borrowings and both items remain available, removes the test-only fault and verifies a successful checkout. This passed as one targeted test and as part of the 90/90 regression suite. The program's uncertainty handling during a lost commit acknowledgment is a **different** scenario and must not be described as guaranteed rollback.

## A17 — Clean schema and database integrity

The disposable verification database received all four PostgreSQL migrations from an initially empty schema. Recorded SQL checks inspected `__EFMigrationsHistory`, tables, constraints, filtered unique active-borrowing index, unique normalized identifier indexes, and two ID sequences. Controlled attempted writes verified rejection of duplicate normalized student number, duplicate normalized equipment identifier, missing student FK, missing equipment FK, and a second active borrowing of the same equipment. A final `ROLLBACK` left the temporary fixture rows uncommitted.

## A18 — Failure, recovery, and uncertain commit

`Checkout_WhenDatabaseUnavailable_ReturnsFailed` passed using an unreachable connection endpoint. `CheckoutAndReturn_PersistCorrectly` passed using a working database. These show known pre-commit failure and later successful database operations; they do **not** prove that the application experienced an actual in-flight network outage or that an unknown commit was simulated. The service sets `commitAttempted` immediately before `CommitAsync`: exceptions thereafter return `Unknown()` and must not trigger a blind retry. Inspect persisted rows through a fresh database connection before starting another operation.

## Submission hygiene

Keep temporary `PHASE6-README.txt`, `PHASE7-README.txt`, and `PHASE8-INSTRUCTIONS.txt` out of the submitted package. Exclude passwords, local `.env`, and actual connection strings. Confirm both partners' full names and student IDs on their **individual** submission forms or permitted private submission document. The latest known shared merge was `febc984`; the final documentation commit/push remains to be verified.

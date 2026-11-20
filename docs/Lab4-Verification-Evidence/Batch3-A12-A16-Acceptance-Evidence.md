# Equipment Borrowing System — Batch 3 Acceptance Evidence

**Laboratory:** ITSD81 Laboratory Activity 4 — Advanced Data-Driven Workflows and PostgreSQL Integration  
**Acceptance cases:** A12 / UC-12, A13 / UC-13, A14 / UC-14, A15 / UC-15, A16 / UC-16  
**Primary actor:** Equipment Custodian / Laboratory Staff  
**Database:** PostgreSQL (`equipment_borrowing_dev`)  
**Report status:** **Batch 3 reported complete by the student. Screenshots, per-case observed outcomes and final database readbacks have not been supplied for independent review.**

## 1. Summary

| Case | Required behavior | Student report | Evidence status |
|---|---|---|---|
| A12 — Register Student | Create one unique student, validate required fields and normalized duplicates; survives restart | Batch 3 complete | UI and SQL proof to attach |
| A13 — Update Student Information | Valid descriptive edits preserve student ID, borrowing FKs/history; reject invalid edits | Batch 3 complete | Before/after identity/history proof to attach |
| A14 — Change Student Eligibility | Restrict new checkout without cancelling active loan; return while restricted; restore eligibility and borrow | Batch 3 complete | UI sequence and SQL proof to attach |
| A15 — Register Equipment | One uniquely identified available equipment item, no active borrowing; reject blank/duplicate values | Batch 3 complete | UI and migration/SQL proof to attach |
| A16 — Update Equipment Information | Descriptive edits retain equipment ID, current availability and history; reject invalid edits | Batch 3 complete | Before/after identity/history proof to attach |

*The student confirmed overall completion, not individual observation records for every negative branch.*

## 2. A12 — UC-12 Register Student

**Setup:** New fictional student number, not already present; valid required attributes and optional contact.  
**Action:** Register and retrieve student; retry with blank required number/name, an existing student number, and a case-/space-variant of that number; restart application.  
**Expected:** Exactly one student row with stable identity and normalized-unique institutional number; eligibility defaults to permitted; duplicate and blank submissions add no records; persisted data survives restart.  
**Observed:** Student reported Batch 3 completed; specific generated ID and individual rejection messages not supplied.  
**Evidence:** `UI-Screenshots/Batch3/A12-student-registration.png`, `A12-duplicate-student.png`, `A12-invalid-student.png`, and fresh SQL results — **pending attachment**.

## 3. A13 — UC-13 Update Student Information

**Setup:** Existing student with at least one borrowing record; record original `StudentId` and related borrowing IDs before edits.  
**Action:** Edit allowed student details and clear optional contact; attempt blank required fields and duplicate student number; restart.  
**Expected:** Same `StudentId` and borrowing relationships, updated descriptive fields, optional contact stored as absent, no changes after rejected edits; changes persist after restart.  
**Observed:** Student reported Batch 3 completed; exact before/after database values not supplied.  
**Evidence:** `UI-Screenshots/Batch3/A13-student-update.png`, `A13-student-history-preserved.png`, `A13-invalid-update.png`, before/after PostgreSQL readback — **pending attachment**.

## 4. A14 — UC-14 Change Student Borrowing Eligibility

**Setup:** Eligible student with an active borrowing, plus another distinct available item.  
**Action:** Restrict eligibility; attempt a new otherwise-valid checkout; return original active item; restore eligibility and perform a valid checkout; repeat eligibility setting.  
**Expected:** Restriction persists; new issues rejected while restricted; existing active borrowing not cancelled; return remains possible; after permitting, a valid new checkout succeeds; repeated-state feedback is understandable.  
**Observed:** Student reported Batch 3 completed; individual observed messages and SQL state transitions not supplied.  
**Evidence:** `UI-Screenshots/Batch3/A14-eligibility-restricted.png`, `A14-checkout-rejected.png`, `A14-return-while-ineligible.png`, `A14-eligibility-restored.png`, SQL state comparisons — **pending attachment**.

## 5. A15 — UC-15 Register Equipment

**Setup:** Distinct fictional identifier and name; optional description.  
**Action:** Register equipment; test blank name/identifier and normalized case/space duplicate; inspect initial availability and borrowing count; restart.  
**Expected:** Exactly one row, initially available, with no active borrowing; invalid/duplicate requests add no row; saved item survives restart.  
**Observed:** The student initially observed a **failed registration** while the development database lacked `EquipmentIdSequence`. Investigation showed four existing equipment records and no `EquipmentIdSequence` in `pg_sequences`; `CAM-002` did not exist at that time. The student subsequently indicated the migration issue was addressed and Batch 3 completed, but **a fresh post-fix successful registration SQL result was not supplied**.  
**Evidence:** `UI-Screenshots/Batch3/A15-equipment-registration.png`, `A15-duplicate-equipment.png`, `A15-invalid-equipment.png`, new-row SQL output and migration history — **pending attachment**.

**Migration verification (required to reconcile earlier failure):**
```sql
select "MigrationId"
from "__EFMigrationsHistory"
order by "MigrationId";

select schemaname, sequencename, last_value
from pg_sequences
where schemaname = 'public'
order by sequencename;
```

Expected to include `20261119130134_AddEquipmentIdSequence` and `EquipmentIdSequence`.

## 6. A16 — UC-16 Update Equipment Information

**Setup:** Equipment with existing borrowing history (ideally currently borrowed); record `EquipmentId`, `IsAvailable`, and history IDs before edit.  
**Action:** Change name/identifier/description as permitted, including clearing optional description; attempt blank required field, duplicate identifier, and an availability change via descriptive editing; restart.  
**Expected:** Allowed edits persist on the same `EquipmentId`, with unchanged availability and preserved borrowing links; unavailable equipment stays unavailable while checked out; rejected updates make no changes.  
**Observed:** Student reported Batch 3 completed; per-scenario comparison not supplied.  
**Evidence:** `UI-Screenshots/Batch3/A16-equipment-update.png`, `A16-availability-preserved.png`, `A16-history-preserved.png`, `A16-restart-verification.png`, before/after PostgreSQL readback — **pending attachment**.

## 7. Fresh PostgreSQL readback (read-only)

Execute with a new PostgreSQL connection and substitute the **actual registered identifiers**, not imaginary data. If your identifiers match the suggested fixtures, you may use `LAB4-B3-001` and `LAB4-B3-EQ-001`.

```sql
select
    "StudentId", "StudentNumber", "StudentName", "IsAllowedToBorrow"
from "Students"
where "StudentNumber" = 'LAB4-B3-001';

select
    "EquipmentId", "EquipmentIdentifier", "EquipmentName", "IsAvailable"
from "Equipment"
where "EquipmentIdentifier" = 'LAB4-B3-EQ-001';

select
    b."BorrowingId", b."StudentId", b."EquipmentId",
    b."BorrowedAt", b."ExpectedReturnDate",
    b."ActualReturnDate", b."Status"
from "Borrowings" b
join "Students" s on s."StudentId" = b."StudentId"
where s."StudentNumber" = 'LAB4-B3-001'
order by b."BorrowingId";
```

**Attach:** `Batch3-PostgreSQL-Readback.txt` — **pending**. For edits, record original and final IDs, availability/eligibility, and borrowing IDs to show relationship preservation.

## 8. Screenshot and evidence checklist

- [ ] A12 successful registration, blank-field and normalized duplicate rejection, restart
- [ ] A13 valid edit, optional-contact clearing, invalid edit, identity/history preserved
- [ ] A14 restrict, reject new checkout, preserve active loan, allow return, re-enable, successful new issue
- [ ] A15 successful equipment registration, default availability, no active loan, normalized duplicates, restart
- [ ] A15 verify `EquipmentIdSequence` migration and sequence exists after fix
- [ ] A16 valid descriptive edit, rejected edits, current availability preserved, history preserved, restart
- [ ] PostgreSQL fresh readback with actual IDs and meaningful before/after comparisons
- [ ] Exact observed results and supporting filenames recorded for each executed scenario

## 9. Sign-off

**Student-reported result:** Batch 3 completed.  
**Independent evidence audit:** Pending review of screenshots and fresh database readbacks.  
**Known historical defect:** Initial A15 registration attempt failed due to missing `EquipmentIdSequence` in development; subsequent fix was reported, not independently reverified by supplied query outputs.  
**Outstanding negative/error branches:** Must remain *Pending* unless a real screenshot, SQL result, or targeted automated test proves them.

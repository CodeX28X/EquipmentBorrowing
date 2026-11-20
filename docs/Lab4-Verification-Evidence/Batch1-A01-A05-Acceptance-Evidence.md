# Equipment Borrowing — Laboratory Activity 4
## Appendix C — Batch 1 Acceptance Evidence (A01–A05)

**Date of reported verification:** 20 November 2026  
**Branch/commit:** Record the actual merged `main` commit used for the final demonstration.  
**System:** Avalonia desktop application / PostgreSQL (`equipment_borrowing_dev`)  
**Primary actor:** Equipment Custodian / Laboratory Staff  
**Evidence provenance:** The pass results below are **student-reported manual checks**, not independently witnessed or accompanied by screenshots/SQL outputs in this report. Do not label unverified scenarios as passed.

## 1. Results summary

| ID | Required use case | Reported successful-path result | Outstanding evidence / acceptance scenarios |
|---|---|---|---|
| A01 / UC-01 | View Equipment Catalog | Catalog data was previously shown to exist in PostgreSQL (four rows at the time of inspection); catalog visible after UC-02 loading fix is implied but **not explicitly certified in the Batch 1 result submission**. | Screenshot comparing UI to fresh SQL; empty catalog; database-connection failure distinguished from empty state. |
| A02 / UC-02 | Find Available Equipment | User previously reported the newly added availability filter working. Before/after expectation from the earlier SQL snapshot: 4 total records, 3 available; confirm screenshots match current database. | Capture All/Available-only screenshots, fresh SQL comparison, no-available-items state, and connection-error state. |
| A03 / UC-03 | Borrow Equipment | **PASS — student reported.** Single-item checkout successful. | Preserve success screenshot and fresh SQL readback; document ineligible, missing/stale, unavailable, over-limit, missing/invalid-date rejections with evidence. |
| A04 / UC-04 | View Active Borrowings | **PASS — student reported.** New active borrowing displayed. | Preserve UI and fresh SQL comparison; empty active-list and connection-error behavior. |
| A05 / UC-05 | Return Equipment | **PASS — student reported.** Return completed successfully. | Preserve pre/post SQL for **same BorrowingId** and UI screenshots; repeat return and stale/missing selection rejection. |
| BR-19 | Persistence across application restart | **PASS — student reported.** State persisted on restart. | Screenshot and new PostgreSQL readback after restart; attach timestamps/evidence paths. |

### Overall status
**Primary A03–A05 success path and restart: student-reported PASS.** A01/A02 supporting screenshots and negative/empty/error scenarios still require documentation and/or execution. **Do not mark the complete A01–A05 Appendix C matrix fully accepted until those checks are recorded.**

## 2. Database baseline already observed

The user previously queried `equipment_borrowing_dev` and observed:

| EquipmentId | EquipmentIdentifier | EquipmentName | IsAvailable at that observation |
|---:|---|---|---|
| 1 | LAP-001 | Laptop 01 | true |
| 2 | LAP-002 | Laptop 02 | true |
| 3 | PROJ-001 | Projector 01 | true |
| 4 | CAM-001 | Camera 01 | false |

**Important:** This is a historical snapshot, not proof of the state after checkout/return or at final submission.

## 3. Attach acceptance evidence

Place screenshots in `docs/Lab4-Verification-Evidence/UI-Screenshots/` and link them here after they actually exist.

| Evidence file | Purpose | Captured? |
|---|---|---|
| `A01-equipment-catalog.png` | All equipment with availability | [ ] |
| `A02-all-equipment.png` | Available-only unchecked | [ ] |
| `A02-available-equipment.png` | Available-only checked | [ ] |
| `A03-checkout-success.png` | Checkout success notification | [ ] |
| `A03-checkout-rejected.png` | Representative invalid/unavailable checkout | [ ] |
| `A04-active-borrowings.png` | New active borrowing in UI | [ ] |
| `A05-return-success.png` | Return success | [ ] |
| `A05-restart-verification.png` | Persisted state after restart | [ ] |
| `Batch1-Database-Readback.txt` | Fresh SQL evidence before/after/restart | [ ] |

## 4. Fresh PostgreSQL readback

Run in `psql` against `equipment_borrowing_dev` using a new connection. Never place passwords in the evidence file.

```sql
select current_database() as verified_database;

select "EquipmentId", "EquipmentIdentifier", "EquipmentName", "IsAvailable"
from "Equipment"
order by "EquipmentId";

select "EquipmentId", "EquipmentIdentifier", "EquipmentName"
from "Equipment"
where "IsAvailable" = true
order by "EquipmentId";

select
    b."BorrowingId",
    s."StudentNumber",
    e."EquipmentIdentifier",
    b."BorrowedAt",
    b."ExpectedReturnDate",
    b."ActualReturnDate",
    b."Status",
    e."IsAvailable"
from "Borrowings" b
join "Students" s on s."StudentId" = b."StudentId"
join "Equipment" e on e."EquipmentId" = b."EquipmentId"
where e."EquipmentIdentifier" = 'LAP-001'
order by b."BorrowingId" desc;
```

For a specific checkout, record the `BorrowingId` and student identifier, then compare **the same BorrowingId** at each of these checkpoints:

1. After checkout: status `0` (Active), actual return date `NULL`, equipment unavailable.
2. After return: status `1` (Returned), return date recorded, equipment available.
3. After restart: same record still returned, equipment available, history preserved.

If you already completed the return, the database cannot reconstruct a contemporaneous before-return readback unless you captured it then. Do **not** label a present-day query as a past snapshot. The existing integration-test logs may support the missing technical verification; otherwise run a fresh controlled checkout/return on safe test data.

## 5. Scenario worksheet — finish before claiming full acceptance

Use actual observed feedback and file names; `Pending` means not yet evidenced.

| ID | Scenario | Expected outcome | Observed result | Evidence | Status |
|---|---|---|---|---|---|
| A01 | Catalog loaded with records | Matches database availability and identity | __________ | __________ | Pending |
| A01 | Empty catalog in controlled setup | Clear empty feedback | __________ | __________ | Pending |
| A01 | Failed catalog connection | Error feedback, not empty state | __________ | __________ | Pending |
| A02 | All vs available-only listing | Excludes unavailable equipment when filtered | Reported working | __________ | Reported pass; attach evidence |
| A02 | No available matches | Explicit no-match feedback | __________ | __________ | Pending |
| A02 | Failed available-item load | Error feedback | __________ | __________ | Pending |
| A03 | Single checkout | Active row created, equipment unavailable | Reported PASS | __________ | Reported pass; attach evidence |
| A03 | Ineligible/missing/unavailable/over-limit/invalid dates | Rejected; no new borrowing | __________ | __________ | Pending or link verified tests |
| A04 | View active borrowings | Correct student/equipment/dated active row | Reported PASS | __________ | Reported pass; attach evidence |
| A04 | Empty active list | Explicit empty feedback | __________ | __________ | Pending |
| A04 | Failed active load | Error feedback | __________ | __________ | Pending |
| A05 | Return active equipment | Same row returned, date set, item available | Reported PASS | __________ | Reported pass; attach evidence |
| A05 | Repeat/stale return | Rejected without mutation | __________ | __________ | Pending or link verified tests |
| A05 | Reopen application | Returned status and availability persisted | Reported PASS | __________ | Reported pass; attach evidence |

## 6. Final evidence integrity review

- [ ] The branch used for screenshots matches the merged feature on `main`.
- [ ] Every screenshot and SQL file listed actually exists.
- [ ] The readback is performed after operations using a fresh database connection.
- [ ] The same borrowing ID is traced through active → returned → restart.
- [ ] Required negative, empty, and connection-error cases have genuine supporting evidence or accurately described gaps.
- [ ] No credentials or real student-sensitive records are exposed in screenshots/logs.
- [ ] This report is saved in `docs/Lab4-Verification-Evidence/` and committed if it is to be part of the submitted repository.

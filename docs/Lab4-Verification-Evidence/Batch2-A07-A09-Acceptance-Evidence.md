# Equipment Borrowing System — Batch 2 Acceptance Evidence

**Laboratory:** ITSD81 Laboratory Activity 4 — Advanced Data-Driven Workflows and PostgreSQL Integration  
**Acceptance cases:** A07 / UC-07, A08 / UC-08, A09 / UC-09  
**Primary actor:** Equipment Custodian / Laboratory Staff  
**Database:** PostgreSQL (`equipment_borrowing_dev`)  
**Report status:** **Manual walkthrough reported complete by the student; supporting screenshots and fresh PostgreSQL readbacks have not been supplied for independent review.**  
**Evidence rule:** Do not mark a subscenario independently verified unless its screenshot, SQL output, automated test, or recorded observation is attached.

## 1. Summary

| Case | Required behavior | Reported outcome | Evidence status |
|---|---|---|---|
| A07 — Student Borrowing History | Student-specific active and returned borrowing records; distinguish between students, no-history selection, stale selection, and load failures | Student reported Batch 2 complete | UI screenshots and fresh DB comparison pending attachment |
| A08 — Equipment Borrowing History | Equipment-specific active and returned history; distinguish between items, no-history selection, stale selection, and load failures | Student reported Batch 2 complete | UI screenshots and fresh DB comparison pending attachment |
| A09 — Search, Filter, Sort | Case-insensitive partial student name, institutional number, equipment name; status filters; both date sort fields and directions; combinations, reset, no-match, error/loading behavior | Student reported Batch 2 complete | UI screenshots, query/result comparisons pending attachment |

*“Reported complete” reflects the student's statement, not independently observed proof of every Appendix C branch.*

## 2. A07 — UC-07 View Student Borrowing History

**Setup:** Select two students with distinguishable active/returned borrowing histories and one student with no borrowings. Use genuine existing fixture identities.  
**Action:** Open Student History; choose each student successively; inspect borrowing identifiers, equipment names, checkout/expected/actual return dates, and statuses. Test no-history, missing/stale selection, and controlled load failure separately where possible.  
**Expected:** Only records linked to the selected student appear. Returned records remain visible. A missing history is an empty state rather than a database error; failed loads display an error; switching selection does not leave stale records on screen. Loading remains responsive.  
**Observed:** Student reported completion of the Batch 2 workflow; item-by-item observations not supplied.  
**Assessment:** Main path **reported passed**; negative/error branches **not independently evidenced**.

**Suggested evidence files:**
- `UI-Screenshots/Batch2/A07-student-history.png`
- `UI-Screenshots/Batch2/A07-different-student.png`
- `UI-Screenshots/Batch2/A07-empty-history.png`
- `UI-Screenshots/Batch2/A07-database-error.png` (only if actually demonstrated)

## 3. A08 — UC-08 View Equipment Borrowing History

**Setup:** Select two equipment items with distinguishable active/returned histories and one item with no borrowing history.  
**Action:** Open Equipment History; switch between selected items and compare all displayed records against PostgreSQL. Test empty history, stale selection, and load failure separately.  
**Expected:** The selected item's current and returned borrowing records are displayed accurately with correct student and date details. Unrelated records are excluded; empty and connection-error messages remain distinguishable. Asynchronous loading does not block the window.  
**Observed:** Student reported completion of the Batch 2 workflow; no detailed per-scenario observed values or screenshots supplied.  
**Assessment:** Main path **reported passed**; negative/error branches **not independently evidenced**.

**Suggested evidence files:**
- `UI-Screenshots/Batch2/A08-equipment-history.png`
- `UI-Screenshots/Batch2/A08-different-equipment.png`
- `UI-Screenshots/Batch2/A08-empty-history.png`
- `UI-Screenshots/Batch2/A08-database-error.png` (only if actually demonstrated)

## 4. A09 — UC-09 Search, Filter, and Sort Borrowings

**Setup:** Use a dataset with active and returned records, multiple students, distinct equipment, and different checkout/expected return dates.  
**Action and expected outcome:**

| Test | Action | Expected result | Observed/evidence |
|---|---|---|---|
| A09-01 | Search partial student name, including different case | Case-insensitive correct matches | Reported batch complete; evidence to attach |
| A09-02 | Search partial institutional/student number | Correct matches | Reported batch complete; evidence to attach |
| A09-03 | Search partial equipment name | Correct matches | Reported batch complete; evidence to attach |
| A09-04 | Search with surrounding whitespace | Search text trimmed | Not separately evidenced |
| A09-05 | Filter `Active` | Only active records | Reported batch complete; evidence to attach |
| A09-06 | Filter `Returned` | Only returned records | Reported batch complete; evidence to attach |
| A09-07 | Filter `All` | Both statuses | Reported batch complete; evidence to attach |
| A09-08 | Checkout date: ascending and descending | Dates ordered as requested; stable borrowing-ID tie-break | Not separately evidenced |
| A09-09 | Expected return date: ascending and descending | Dates ordered as requested; stable borrowing-ID tie-break | Not separately evidenced |
| A09-10 | Combine search, status, and sort | Intersection of selected criteria | Reported batch complete; evidence to attach |
| A09-11 | Reset criteria | Unfiltered result set restored | Reported batch complete; evidence to attach |
| A09-12 | Search term with no matches | Explicit no-results state, not connection error | Reported batch complete; evidence to attach |
| A09-13 | Controlled database-load failure | Clear error and responsive loading state | Not separately evidenced |

**Suggested evidence files:**
- `UI-Screenshots/Batch2/A09-search-student-name.png`
- `UI-Screenshots/Batch2/A09-search-student-number.png`
- `UI-Screenshots/Batch2/A09-search-equipment.png`
- `UI-Screenshots/Batch2/A09-filter-active.png`
- `UI-Screenshots/Batch2/A09-filter-returned.png`
- `UI-Screenshots/Batch2/A09-sort-checkout-date.png`
- `UI-Screenshots/Batch2/A09-sort-expected-return.png`
- `UI-Screenshots/Batch2/A09-combined-criteria.png`
- `UI-Screenshots/Batch2/A09-reset-criteria.png`
- `UI-Screenshots/Batch2/A09-no-match.png`
- `UI-Screenshots/Batch2/A09-database-error.png` (only if actually demonstrated)

## 5. Fresh PostgreSQL readback (execute and retain actual output)

Use a **new `psql` connection** after UI operations. This is a read-only query; attach the resulting output rather than claiming it was already executed.

```sql
select
    b."BorrowingId",
    s."StudentId",
    s."StudentNumber",
    e."EquipmentId",
    e."EquipmentIdentifier",
    b."BorrowedAt",
    b."ExpectedReturnDate",
    b."ActualReturnDate",
    b."Status"
from "Borrowings" b
join "Students" s on s."StudentId" = b."StudentId"
join "Equipment" e on e."EquipmentId" = b."EquipmentId"
order by b."BorrowedAt" desc, b."BorrowingId" desc;
```

**SQL output attachment:** `Batch2-PostgreSQL-Readback.txt` — **pending**.  
**Comparison notes:** Record the actual selected student/equipment identifiers, matched borrowing IDs, filter membership, and sorting order here after inspection.

## 6. Screenshot and evidence checklist

- [ ] A07 student 1, student 2, no-history (and stale/error where demonstrated)
- [ ] A08 equipment 1, equipment 2, no-history (and stale/error where demonstrated)
- [ ] A09 searches by three supported fields
- [ ] A09 Active/Returned/All statuses
- [ ] A09 both sort dates, ascending and descending
- [ ] A09 combined criteria, reset, no-match
- [ ] Connection-error and asynchronous-loading evidence (or exact automated test reference)
- [ ] Fresh PostgreSQL readback and UI-vs-database reconciliation
- [ ] Expected and **actually observed** results added for each executed test

## 7. Sign-off

**Student-reported result:** Batch 2 completed.  
**Independent evidence audit:** Pending screenshot and readback review.  
**Outstanding Appendix C branches:** Mark any unexecuted stale-selection, failure/loading, or tie-break-order checks as *Pending*, never *Passed* without supporting evidence.

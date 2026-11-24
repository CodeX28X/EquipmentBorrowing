# Consolidated Lab 5 acceptance execution record

Environment: local Windows, .NET 10, PostgreSQL, disposable `equipment_borrowing_lab5_acceptance`; fixture Manila date 2026-11-24. Automated run on 2026-11-24 around 21:26–21:27 +08:00. The original evidence is retained unchanged in its timestamped directory (`events.jsonl`, `summary.json`, `openapi.json`, `api-*.log`). Manual screenshots are in `evidence/manual/`. The original automated run recorded 651 events, 437 passing assertions (including two setup assertions), 111 HTTP requests, 89 PostgreSQL snapshots, 13 SQL events, and zero failed assertions. This is not a claim that all manual or semantic contract tests passed.

| ID | Expected | Observed | Disposition | Evidence |
|---|---|---|---|---|
| T01 | Independent valid reads, empty/missing behavior, all 16 accurate contracts, no writes | 127 assertions passed; original OpenAPI schema permitted string IDs and omitted some constraints | Automated retest passed; supplementary schema verification performed; full semantic audit not exhaustive | events.jsonl test=T01; original openapi.json |
| T02 | Create/update student, uniqueness, eligibility, history, restart | 57 assertions passed | Pass (original run) | events.jsonl test=T02; fresh state and logs |
| T03 | Create/update equipment, protected availability, uniqueness, restart | 41 assertions passed | Pass (original run) | events.jsonl test=T03 |
| T04 | Single checkout/return, invalid operations rejected, restart persistence | 53 assertions passed | Pass (original run) | events.jsonl test=T04 |
| T05 | Atomic multi-checkout, limit boundaries and controlled post-write rollback | 107 assertions passed; safe HTTP 500 on controlled fault, fresh compared state unchanged | Pass (original run) | events.jsonl test=T05; api-397.log |
| T06 | Histories and query membership/order, no writes | 50 assertions passed | Pass (original run) | events.jsonl test=T06 |
| T07 | Desktop HTTP-only, mutations, histories, independent update and refresh | Manual images show independent API, credential-free desktop, student/equipment edit, checkout, return, history, external update and DB readback | Principal workflows demonstrated; some separate intermediate database reads and equipment history capture not established | evidence/manual/T07-*.png; Phase 5 reported dependency inspection |
| T08 | No response vs safe 500, recover, restart/readback, uncertainty safeguard | Manual images show transport error, received 500, successful recovery, ST-800 persisted | Core scenarios and retry safeguard demonstrated; actual lost commit acknowledgement not induced | evidence/manual/T08-*.png; T05 rollback evidence |

## Issues, diagnosis, corrections, retests

- Lab 4 regression originally required disposable A06 connection configuration. User configured it and reported 90/90 tests passed.
- Phase 5 patch could not add an existing untracked HTTP client. Backed it up outside repository, validated/applied patch, build and 90/90 tests passed; user reported 21/21 smoke checks.
- Phase 6 patch context failed on mixed LF/CRLF. Revalidated with `--ignore-space-change`, applied; build and 90/90 passed. No application changes from line-ending normalization.
- Acceptance script initially misread `DbConnectionStringBuilder` in PowerShell. Explicit setter/getter and dictionary methods corrected it; local acceptance target confirmed. Missing API EF design package then fixed; four migrations applied. Controlled dataset smoke checks reported 8/8.
- Original T01 semantic issue: integer/string schemas, enum and uniqueness documentation, unknown fields. Corrected published contract now ships with API; automated live retest passed. Original evidence must not be overwritten.
- T05 intentional database trigger failure is an expected test outcome, not a defect. HTTP 500 plus unchanged independent persisted state verifies rollback.
- T08 invalid database failure and connection failure are deliberate scenarios. Actual uncertain commit acknowledgement remains untested; do not infer rollback from a transport failure.

## Final verification addendum

Final verification date: 2026-11-24 (Asia/Manila).

- Final solution build succeeded.
- Regression suite: 90 passed, 0 failed, 0 skipped.
- Final automated acceptance retest: T01-T06 passed.
- Original automated evidence: `evidence/20261124-212625/`.
- Final automated retest: `evidence/20261124-221220/`.
- Both original and final evidence are retained unchanged.
- Supplementary OpenAPI verification confirmed integer identifiers,
  borrowing status enumeration, minimum and unique checkout items,
  and rejection of additional student request properties.
- A fully exhaustive semantic comparison of every OpenAPI schema
  constraint was not performed.

### T08 supplementary retry safeguard verification

The following eight runtime checks were reported successful:

1. Attempted mutation while API was stopped.
2. Unknown outcome warning displayed.
3. Immediate retry blocked.
4. API restarted successfully.
5. Student list refreshed successfully.
6. Independent database state verified.
7. Registration succeeded after verification.
8. Exactly one ST-900 record existed afterward.

Evidence:

- `evidence/manual/T08-05-unknown-outcome.png`
- `evidence/manual/T08-06-retry-blocked.png`
- `evidence/manual/T08-07-verified-retry.png`

The API was stopped before the mutation request. This verifies
conservative uncertainty handling and refresh-before-retry behavior,
not an actual loss of acknowledgement during transaction commit.

### Evidence limitations

Sixteen manual screenshots are retained. T07 principal workflows
are demonstrated, but separate equipment-history screenshots and
all intermediate PostgreSQL reads were not independently established.
No missing evidence or unexecuted test is represented as completed.

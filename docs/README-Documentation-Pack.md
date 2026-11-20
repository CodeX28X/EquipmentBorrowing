# Equipment Borrowing — Lab 4 Documentation Pack

This guide indexes the Laboratory Activity 4 deliverables prepared against the instructor's handout and adapted fourteen-case use-case suite. It distinguishes completed automated/database checks from manual Appendix C evidence still requiring confirmation.

## Document index

| Repository path | Purpose |
|---|---|
| `README.md` | Current PostgreSQL application, setup, architecture, tests and historical reflections |
| `docs/Lab4-UseCase-Diagram.png` | Diagram image for submission |
| `docs/Lab4-UseCase-Diagram.svg` | Resolution-independent diagram |
| `docs/Lab4-UseCase-Diagram.puml` | Editable PlantUML diagram source |
| `docs/Lab4-UseCase-Diagram.dot` | Alternate Graphviz diagram source |
| `docs/Lab4-UseCase-Catalog.md` | Full 33-case catalog with 14 required Lab 4 cases |
| `docs/Lab4-Adapted-14-Case-Suite.md` | Fourteen required adapted use-case specifications |
| `docs/EquipmentBorrowing-AppendixC-Traceability-Phase8B.md` | A01–A19 evidence mapping and honest completion status |
| `docs/Lab4-Verification-Evidence/` | Seven captured local execution logs |
| `docs/README-legacy.md` | Archived previous-activity documentation |

## Verified technical evidence — November 20, 2026

- Full xUnit regression suite: **90 passed, 0 failed, 0 skipped**.
- A06: isolated controlled PostgreSQL failure after attempted write; fresh-context rollback and recovery assertions passed.
- A17: all four migrations applied to a disposable database; inspected constraints and indexes; five invalid writes rejected and rolled back.
- A18: pre-commit database-unavailable test and subsequent normal checkout/return test both passed. A real lost-commit acknowledgment was **not simulated**.
- Source documentation and Git history were captured; final commit and push are **not yet confirmed**.

## Remaining submission verification

1. Review instructor-required UI screenshots and before/after/restart database readbacks for Appendix C rows that still indicate pending evidence. Automated tests alone are not a claim that every acceptance artifact exists.
2. Check the diagram/catalog and fourteen-case specifications against the current UI and handout; defer UC-10–11 and UC-17–27 to Lab 5; mark UC-28–33 as future scope.
3. Review staged files for accidental secrets, temporary phase notes, local databases and outdated claims. Keep only intended artifacts.
4. Confirm both partners' full names and student IDs in the appropriate **individual** submission information; do not add private IDs to the repository unnecessarily.
5. Record final Git commit, push status and submission archive after performing them. Do not claim completion in advance.

## Database safety

Operational development database: `equipment_borrowing_dev`; ordinary integration tests: `equipment_borrowing_test`; A06 fault-injection: separately migrated disposable `equipment_borrowing_lab4_verify`. Configure them with the appropriate environment variables; never point the A06 trigger test at the development database. Never commit credentials.

# Laboratory Activity 4 — Updated Use Case Catalog

**Project:** Campus Equipment Borrowing System
**Primary actor:** Laboratory Staff / Equipment Custodian
**Operational database:** PostgreSQL
**Authoritative classification:** Instructor Appendix A

The 14 required use cases are modeled as independent actor-level user goals. Internal validation, retry handling, and PostgreSQL transactions are implementation steps, **not separate UML use cases**. No `include` or `extend` relationships are required.

| ID | Use Case | Instructor classification | Lab 4 obligation |
|---|---|---|---|
| UC-01 | View Equipment Catalog | Required continuation | Implement and demonstrate |
| UC-02 | Find Available Equipment | Required continuation | Implement and demonstrate |
| UC-03 | Borrow Equipment | Required continuation | Implement and demonstrate |
| UC-04 | View Active Borrowings | Required continuation | Implement and demonstrate |
| UC-05 | Return Equipment | Required continuation | Implement and demonstrate |
| UC-06 | Check Out Multiple Equipment Items | Required new | Implement and demonstrate |
| UC-07 | View Student Borrowing History | Required new | Implement and demonstrate |
| UC-08 | View Equipment Borrowing History | Required new | Implement and demonstrate |
| UC-09 | Search, Filter, and Sort Borrowings | Required new | Implement and demonstrate |
| UC-10 | View Overdue Borrowings | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-11 | View Current Equipment Holder | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-12 | Register Student | Required new | Implement and demonstrate |
| UC-13 | Update Student Information | Required new | Implement and demonstrate |
| UC-14 | Change Student Borrowing Eligibility | Required new | Implement and demonstrate |
| UC-15 | Register Equipment | Required new | Implement and demonstrate |
| UC-16 | Update Equipment Information | Required new | Implement and demonstrate |
| UC-17 | Retire Equipment from Circulation | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-18 | Mark Equipment as Under Maintenance | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-19 | Restore Equipment to Circulation | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-20 | Record Equipment Condition Upon Return | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-21 | Renew or Extend an Active Borrowing | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-22 | Return Multiple Equipment Items | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-23 | View Borrowings Due Soon | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-24 | View Student's Active Borrowing Count | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-25 | View Equipment Utilization Summary | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-26 | Manage Equipment Categories | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-27 | Void an Erroneous Borrowing Without Erasing Its History | Deferred to Laboratory Activity 5 | Catalog only; not required to implement |
| UC-28 | Request an Equipment Reservation | Future scope | Catalog only; not required to implement |
| UC-29 | Approve, Reject, or Cancel Reservations | Future scope | Catalog only; not required to implement |
| UC-30 | Review Staff Action Audit History | Future scope | Catalog only; not required to implement |
| UC-31 | Manage Staff Accounts and Permissions | Future scope | Catalog only; not required to implement |
| UC-32 | Export Borrowing Reports | Future scope | Catalog only; not required to implement |
| UC-33 | Integrate External Services or Notifications | Future scope | Catalog only; not required to implement |

## Scope summary

- Required continuation: UC-01 through UC-05 (5 cases).
- Required new: UC-06–09 and UC-12–16 (9 cases).
- Deferred to Laboratory Activity 5: UC-10–11 and UC-17–27 (13 cases).
- Future scope: UC-28–33 (6 cases).

## Implementation note

Required cases are represented in the diagram and adapted specifications. Deferred and future cases remain classified here and do not need additional specification tables for Lab 4.

## Source

ITSD81 Lab Activity 4.pdf, Appendix A.

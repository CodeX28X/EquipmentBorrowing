-- Read-only evidence. Run independently of HTTP before/after mutations.
SELECT current_database() AS database, now() AS observed_at, (now() AT TIME ZONE 'Asia/Manila')::date AS local_date;
SELECT "StudentId","StudentNumber","StudentName","ContactInformation","IsAllowedToBorrow" FROM public."Students" ORDER BY "StudentId";
SELECT "EquipmentId","EquipmentIdentifier","EquipmentName","Description","IsAvailable" FROM public."Equipment" ORDER BY "EquipmentId";
SELECT "BorrowingId","StudentId","EquipmentId","BorrowedAt","ExpectedReturnDate","ActualReturnDate","Status" FROM public."Borrowings" ORDER BY "BorrowingId";

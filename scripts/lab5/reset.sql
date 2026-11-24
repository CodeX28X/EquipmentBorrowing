-- DESTRUCTIVE: only for the explicitly named disposable acceptance database.
BEGIN;
DO $$ BEGIN
 IF current_database() <> 'equipment_borrowing_lab5_acceptance' THEN
  RAISE EXCEPTION 'Refusing reset outside disposable Lab 5 database';
 END IF;
END $$;
DROP TRIGGER IF EXISTS lab5_fail_second_insert ON public."Borrowings";
DROP FUNCTION IF EXISTS public.lab5_fail_second_insert();
TRUNCATE public."Borrowings", public."Equipment", public."Students" RESTART IDENTITY;
INSERT INTO public."Students" ("StudentId","StudentNumber","StudentName","IsAllowedToBorrow") VALUES
(1,'ST-100','Ada Cruz',true),(2,'ST-200','Ben Diaz',false),(3,'ST-300','Cara Lim',true),(4,'ST-400','Dani Ong',true);
INSERT INTO public."Equipment" ("EquipmentId","EquipmentIdentifier","EquipmentName","IsAvailable") VALUES
(1,'EQ-001','Camera',false),(2,'EQ-002','Laptop',true),(3,'EQ-003','Projector',true),(4,'EQ-004','Tablet',false),(5,'EQ-005','Microphone',false),(6,'EQ-006','Tripod',true),(7,'EQ-007','Speaker',true);
-- Actual Manila date, with the same offsets as the instructor's November 10 fixture.
WITH clock AS (SELECT (now() AT TIME ZONE 'Asia/Manila')::date AS d)
INSERT INTO public."Borrowings" ("StudentId","EquipmentId","BorrowedAt","ExpectedReturnDate","ActualReturnDate","Status")
SELECT v.s,v.e,d+v.b,d+v.due,CASE WHEN v.r IS NULL THEN NULL ELSE d+v.r END,v.status
FROM clock CROSS JOIN (VALUES (1,1,-9,-1,NULL::integer,0),(1,3,-8,2,-2,1),(3,4,-7,2,NULL,0),(3,5,-7,2,NULL,0)) v(s,e,b,due,r,status);
SELECT setval('public."StudentIdSequence"'::regclass,4,true);
SELECT setval('public."EquipmentIdSequence"'::regclass,7,true);
COMMIT;
SELECT current_database() AS database, (now() AT TIME ZONE 'Asia/Manila')::date AS fixture_date;

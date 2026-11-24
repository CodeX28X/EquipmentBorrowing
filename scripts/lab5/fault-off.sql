BEGIN;
DO $$ BEGIN
 IF current_database() <> 'equipment_borrowing_lab5_acceptance' THEN RAISE EXCEPTION 'Wrong database'; END IF;
END $$;
DROP TRIGGER IF EXISTS lab5_fail_second_insert ON public."Borrowings";
DROP FUNCTION IF EXISTS public.lab5_fail_second_insert();
COMMIT;

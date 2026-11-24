-- Only after resetting the disposable fixture. S4 must have no borrowings.
BEGIN;
DO $$ BEGIN
 IF current_database() <> 'equipment_borrowing_lab5_acceptance' THEN RAISE EXCEPTION 'Wrong database'; END IF;
 IF EXISTS (SELECT 1 FROM public."Borrowings" b JOIN public."Students" s ON s."StudentId"=b."StudentId" WHERE s."StudentNumber"='ST-400') THEN RAISE EXCEPTION 'Reset fixture before enabling fault'; END IF;
END $$;
CREATE FUNCTION public.lab5_fail_second_insert() RETURNS trigger LANGUAGE plpgsql AS $$
BEGIN
 IF EXISTS (SELECT 1 FROM public."Students" WHERE "StudentId"=NEW."StudentId" AND "StudentNumber"='ST-400') AND
 (SELECT count(*) FROM public."Borrowings" WHERE "StudentId"=NEW."StudentId") >= 2 THEN
 RAISE EXCEPTION 'Lab 5 controlled failure after second INSERT' USING ERRCODE='P0001';
 END IF;
 RETURN NEW;
END $$;
CREATE TRIGGER lab5_fail_second_insert AFTER INSERT ON public."Borrowings" FOR EACH ROW EXECUTE FUNCTION public.lab5_fail_second_insert();
COMMIT;

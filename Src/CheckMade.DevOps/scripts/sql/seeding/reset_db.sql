-- run from terminal with
--  for local: `psql -d cm_ops -f Src/CheckMade.DevOps/scripts/sql/seeding/reset_test_setup_data.sql`
--  for prd: `[psql conn string from psw manager] -f Src/CheckMade.DevOps/scripts/sql/seeding/reset_test_setup_data.sql`

-- Deleting all records from local DB
-- Order of table matters due to FK constraints!
-- Thanks to CASCADE only need to specify the tables at the top of the FK dependency chain.

TRUNCATE derived_workflow_states CASCADE;
TRUNCATE derived_workflow_bridges CASCADE;
TRUNCATE live_event_venues CASCADE;
TRUNCATE live_event_series CASCADE;
TRUNCATE users_employment_history CASCADE;
TRUNCATE vendors CASCADE;
TRUNCATE users CASCADE;

-- Check which privileges are granted to the user for the tables in the database. 
SELECT grantee, privilege_type
FROM information_schema.role_table_grants
WHERE grantee = 'cmappuser' AND table_catalog = 'cm_ops';

-- Check all users and their roles for the current database. The default app user should only have 'rolecanlogin'.
SELECT rolname, rolsuper, rolcreaterole, rolcreatedb, rolcanlogin FROM pg_roles;



-- 1. Confirm the index exists
SELECT indexname, tablename, indexdef
FROM pg_indexes
WHERE tablename = 'roles' AND indexname LIKE '%live_event%';

-- 2. Check if it's being used with EXPLAIN
EXPLAIN ANALYZE
SELECT r.token AS role_token, r.role_type AS role_type, r.status AS role_status,
       soa.name AS sphere_name, soa.details AS sphere_details, soa.trade AS sphere_trade,
       lev.name AS venue_name, lev.status AS venue_status,
       le.id AS live_event_id, le.name AS live_event_name,
       le.start_date AS live_event_start_date, le.end_date AS live_event_end_date,
       le.status AS live_event_status
FROM live_events le
         LEFT JOIN roles r on r.live_event_id = le.id
         LEFT JOIN spheres_of_action soa on soa.live_event_id = le.id
         JOIN live_event_venues lev on le.venue_id = lev.id
ORDER BY le.id, r.id, soa.id;
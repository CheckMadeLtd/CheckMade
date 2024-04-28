-- Check which privileges are granted to the user for the tables in the database. 
SELECT grantee, privilege_type
FROM information_schema.role_table_grants
WHERE grantee = 'citus' AND table_catalog = 'cm_ops';

-- Check all users and their roles for the current database. The default app user should only have 'rolecanlogin'.
SELECT rolname, rolsuper, rolcreaterole, rolcreatedb, rolcanlogin FROM pg_roles;


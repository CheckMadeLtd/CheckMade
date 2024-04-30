-- IMPORTANT - Script cannot be run directly from within the IDE:
-- It requires superuser rights (which the IDE's DB Tool shouldn't have)
-- ==> run only with psql e.g. via CLI with: `psql -f [path to this sql script]`

-- Notes on usage of DO BLOCKs:
-- A DO BLOCK represents a single Transaction. All statements succeed or rollback!
-- Variables can be scoped only inside of DO Blocks! Nested Do Blocks are not possible.

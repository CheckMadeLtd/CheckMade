-- IMPORTANT - Script cannot be run directly from within the IDE:
-- It requires superuser rights (which the IDE's DB Tool shouldn't have)
-- ==> run only with psql e.g. via CLI with: `psql -f [path to this sql script]`

-- Notes on usage of DO BLOCKs:
-- A DO BLOCK represents a single Transaction. All statements succeed or rollback!
-- Variables can be scoped only inside of DO Blocks! Nested Do Blocks are not possible.

DO $$
    DECLARE
        dbName CONSTANT text := 'cm_ops';
        userName CONSTANT text := 'cm_app_user';
        userPassword CONSTANT text := 'my_local_dev_db_psw';  -- for local development only, hence save
        tableName CONSTANT text := 'messages';
    BEGIN
        IF EXISTS (SELECT FROM pg_database WHERE datname = dbName) THEN
            RAISE NOTICE 'Database % already exists, skipping creation.', dbName;
        ELSE
            EXECUTE FORMAT('CREATE DATABASE %I', dbName);
        END IF;

        IF EXISTS (SELECT FROM pg_user WHERE usename = userName) THEN
            RAISE NOTICE 'User % already exists, skipping creation.', userName;
        ELSE
            EXECUTE FORMAT('CREATE USER %I WITH PASSWORD %L', userName, userPassword);
        END IF;

        EXECUTE FORMAT('GRANT ALL PRIVILEGES ON DATABASE %I TO %I', dbName, userName);

        EXECUTE FORMAT('CREATE TABLE IF NOT EXISTS %I (ID SERIAL PRIMARY KEY, Details jsonb NOT NULL)', tableName);
        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
    END;
$$;

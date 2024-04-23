
DO $$
    DECLARE
        userName CONSTANT text := 'cm_app_user';
        tableName CONSTANT text := 'messages';
    BEGIN
        EXECUTE FORMAT('CREATE TABLE IF NOT EXISTS %I (ID SERIAL PRIMARY KEY, Details jsonb NOT NULL)', tableName);
        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
    END
$$;



DO $$
    DECLARE
        userName CONSTANT text := 'cmappuser';
        tableName CONSTANT text := 'tlgr_messages';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    tlgr_user_id BIGINT NOT NULL,
                    details JSONB NOT NULL)',
                tableName);
        
        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
        EXECUTE FORMAT('GRANT USAGE, SELECT, UPDATE ON SEQUENCE %I_id_seq TO %I', tableName, userName);
    END
$$;


DO $$
    DECLARE
        userName CONSTANT text := 'cm_app_user';
        tableName CONSTANT text := 'telegram_messages';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    timestamp timestamp NOT NULL,
                    telegram_user_id bigint NOT NULL,
                    details jsonb NOT NULL
                )'
            , tableName);
        
        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
    END
$$;


DO $$
    DECLARE
        userName CONSTANT text := 'cmappuser';
        tableName CONSTANT text := 'roles';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    token varchar(6) NOT NULL,
                    role_type SMALLINT NOT NULL,
                    status SMALLINT NOT NULL,
                    last_data_migration SMALLINT)',
                tableName);

        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
        EXECUTE FORMAT('GRANT USAGE, SELECT, UPDATE ON SEQUENCE %I_id_seq TO %I', tableName, userName);
    END
$$;

DO $$
    DECLARE
        userName CONSTANT text := 'cmappuser';
        tableName CONSTANT text := 'tlg_client_port_roles';
        roleTable CONSTANT text := 'roles';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    role_id INTEGER NOT NULL REFERENCES %I(id),
                    tlg_user_id BIGINT NOT NULL,
                    tlg_chat_id BIGINT NOT NULL,
                    activation_date TIMESTAMP NOT NULL,
                    deactivation_date TIMESTAMP,
                    status SMALLINT NOT NULL,
                    last_data_migration SMALLINT)', 
                tableName, roleTable);

        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
        EXECUTE FORMAT('GRANT USAGE, SELECT, UPDATE ON SEQUENCE %I_id_seq TO %I', tableName, userName);
    END
$$;
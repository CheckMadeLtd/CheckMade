DO $$
    DECLARE
        userName CONSTANT text := 'cmappuser';
        tableName CONSTANT text := 'users';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    mobile varchar(20) NOT NULL UNIQUE,
                    first_name varchar(255) NOT NULL,
                    middle_name varchar(255),
                    last_name varchar(255) NOT NULL,
                    email varchar(255),
                    status SMALLINT NOT NULL,
                    last_data_migration SMALLINT)',
                tableName);

        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
        EXECUTE FORMAT('GRANT USAGE, SELECT, UPDATE ON SEQUENCE %I_id_seq TO %I', tableName, userName);
    END
$$;

ALTER TABLE roles ADD COLUMN user_id INTEGER UNIQUE REFERENCES users(id);
ALTER TABLE roles ALTER COLUMN user_id SET NOT NULL;

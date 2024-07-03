-- After realising that employment history based on updatedable json details in user table is actually more complicated

DO $$
    DECLARE
        userName CONSTANT text := 'cmappuser';
        tableName CONSTANT text := 'users_employment_history';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    start_date TIMESTAMP NOT NULL,
                    end_date TIMESTAMP,
                    user_id INT NOT NULL REFERENCES users(id),
                    vendor_id INT NOT NULL REFERENCES vendors(id),
                    details JSONB NOT NULL,
                    status SMALLINT NOT NULL,
                    last_data_migration SMALLINT,
                    CHECK ((status <> 1 AND end_date IS NOT NULL) OR (status = 1 AND end_date IS NULL)))',
                tableName);

        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
        EXECUTE FORMAT('GRANT USAGE, SELECT, UPDATE ON SEQUENCE %I_id_seq TO %I', tableName, userName);
    END
$$;

CREATE UNIQUE INDEX users_employment_history_user_vendor_when_active
    ON users_employment_history (user_id, vendor_id)
    WHERE status = 1;
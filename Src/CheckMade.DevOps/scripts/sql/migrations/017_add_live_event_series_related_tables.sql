DO $$
    DECLARE
        userName CONSTANT text := 'cmappuser';
        tableName CONSTANT text := 'live_event_series';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    name varchar(255) NOT NULL UNIQUE,
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
        tableName CONSTANT text := 'live_event_venues';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    name varchar(255) NOT NULL UNIQUE,
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
        tableName CONSTANT text := 'live_events';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    name varchar(255) NOT NULL UNIQUE,
                    start_date TIMESTAMP NOT NULL,
                    end_date TIMESTAMP NOT NULL,
                    venue_id INT NOT NULL REFERENCES live_event_venues(id),
                    live_event_series_id INT NOT NULL REFERENCES live_event_series(id),
                    status SMALLINT NOT NULL,
                    last_data_migration SMALLINT)',
                tableName);

        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
        EXECUTE FORMAT('GRANT USAGE, SELECT, UPDATE ON SEQUENCE %I_id_seq TO %I', tableName, userName);
    END
$$;

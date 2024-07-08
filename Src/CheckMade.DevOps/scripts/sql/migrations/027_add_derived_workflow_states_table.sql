-- 'derived' stands for 'derived fact' i.e. an intermediary result from a calculation on a raw fact. 
-- by implication, ephemeral, i.e. records can be deleted e.g. after a LiveEvent lest we need to migrate data
-- for future reporting, they are just derived again, with/from the latest business logic

DO $$
    DECLARE
        userName CONSTANT text := 'cmappuser';
        tableName CONSTANT text := 'derived_workflow_states';
    BEGIN
        EXECUTE FORMAT(
                'CREATE TABLE IF NOT EXISTS %I (
                    id SERIAL PRIMARY KEY,
                    resultant_workflow varchar(6) NOT NULL,
                    in_state BIGINT NOT NULL,
                    last_data_migration SMALLINT)',
                tableName);

        EXECUTE FORMAT('GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE %I TO %I', tableName, userName);
        EXECUTE FORMAT('GRANT USAGE, SELECT, UPDATE ON SEQUENCE %I_id_seq TO %I', tableName, userName);
    END
$$;

ALTER TABLE tlg_inputs ADD COLUMN derived_workflow_states_id INTEGER REFERENCES derived_workflow_states(id);

CREATE TABLE IF NOT EXISTS derived_workflow_bridges (
    id SERIAL PRIMARY KEY,
    src_input_id INT NOT NULL REFERENCES tlg_inputs(id),
    dst_user_id BIGINT NOT NULL,
    dst_chat_id BIGINT NOT NULL,
    dst_interaction_mode SMALLINT NOT NULL,
    dst_message_id INT NOT NULL,
    last_data_migration SMALLINT
);

GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE derived_workflow_bridges TO cmappuser;
GRANT USAGE, SELECT, UPDATE ON SEQUENCE derived_workflow_bridges_id_seq TO cmappuser;


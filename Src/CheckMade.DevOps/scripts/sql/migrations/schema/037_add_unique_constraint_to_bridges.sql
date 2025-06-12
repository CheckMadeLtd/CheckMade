ALTER TABLE derived_workflow_bridges
    ADD CONSTRAINT logical_unique_constraint
        UNIQUE (dst_chat_id, dst_message_id);
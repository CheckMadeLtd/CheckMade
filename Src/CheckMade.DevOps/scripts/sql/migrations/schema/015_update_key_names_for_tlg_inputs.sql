-- This was forgotten in mig. 008
ALTER TABLE tlg_inputs RENAME CONSTRAINT tlgr_messages_pkey TO tlg_inputs_pkey;
ALTER INDEX tlgr_messages_user_id RENAME TO tlg_inputs_user_id;
ALTER SEQUENCE tlgr_messages_id_seq RENAME TO tlg_inputs_id_seq;

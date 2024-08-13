ALTER TABLE tlg_inputs ADD COLUMN message_id int DEFAULT 0;
ALTER TABLE tlg_inputs ALTER COLUMN message_id SET NOT NULL;
ALTER TABLE tlg_inputs ALTER COLUMN message_id DROP DEFAULT;

ALTER TABLE tlg_inputs DROP COLUMN timestamp;
ALTER TABLE tlg_inputs ADD COLUMN date timestamptz DEFAULT current_timestamp;
ALTER TABLE tlg_inputs ALTER COLUMN date SET NOT NULL;
ALTER TABLE tlg_inputs ALTER COLUMN date DROP DEFAULT;

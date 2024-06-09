ALTER TABLE tlgr_updates RENAME TO tlg_inputs;
ALTER TABLE tlg_inputs RENAME COLUMN update_type TO input_type;
ALTER TABLE tlg_inputs RENAME COLUMN bot_type TO channel;
ALTER TABLE tlgr_messages ADD COLUMN last_data_migration SMALLINT;
UPDATE tlgr_messages SET last_data_migration = 1;

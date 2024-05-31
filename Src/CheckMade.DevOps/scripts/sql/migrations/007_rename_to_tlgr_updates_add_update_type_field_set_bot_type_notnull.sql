ALTER TABLE tlgr_messages RENAME TO tlgr_updates;
ALTER TABLE tlgr_updates ALTER COLUMN bot_type SET NOT NULL;

/* Three step process necessary to add a new NOT NULL column! 
   1) Add column, which will set '-1' for existing records,
   2) Make that field non nullable
   3) Remove the DEFAULT so it doesn't apply to future new records */ 
ALTER TABLE tlgr_updates ADD COLUMN update_type SMALLINT DEFAULT -1;
ALTER TABLE tlgr_updates ALTER COLUMN update_type SET NOT NULL;
ALTER TABLE tlgr_updates ALTER COLUMN update_type DROP DEFAULT;


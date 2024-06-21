-- For some reason, at this point, we have two unexpected features in our schema:
-- 1. The user_id FK declared in migration 012 was not created with NOT NULL and
-- 2. There is a second fk by name of roles_user_id_key (instead of fkey) - no idea where that came from! 
-- The below script fixes these two issues!

ALTER TABLE roles DROP CONSTRAINT roles_user_id_key;
ALTER TABLE roles ALTER COLUMN user_id SET NOT NULL;

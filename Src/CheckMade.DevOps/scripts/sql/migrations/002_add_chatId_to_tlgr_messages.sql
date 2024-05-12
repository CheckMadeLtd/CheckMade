ALTER TABLE tlgr_messages ADD COLUMN chat_id BIGINT;
CREATE INDEX tlgr_messages_tlgr_user_id_index ON tlgr_messages (tlgr_user_id);

ALTER TABLE tlgr_messages
    RENAME COLUMN tlgr_user_id TO user_id;

ALTER INDEX tlgr_messages_tlgr_user_id_index
    RENAME TO tlgr_messages_user_id;

ALTER TABLE tlgr_messages
    ALTER COLUMN chat_id SET NOT NULL;
-- This script is executed in CI environment by the apply_migration.sh script
-- Probably NOT suitable to reuse in local DEV env because of more long-lived DB leading to different id's. 

WITH new_user1 AS (
    INSERT INTO users (mobile, first_name, middle_name, last_name, email, status, language_setting)
        VALUES ('+447538521999', '_Daniel', 'IntegrationTest', '_Gorin', 'daniel-integrtest-checkmade@neocortek.net', 0, 0)
    RETURNING id 
) 

INSERT INTO roles (token, role_type, status, user_id) 
VALUES ('AAA111', 1002, 0, (SELECT id FROM new_user1));

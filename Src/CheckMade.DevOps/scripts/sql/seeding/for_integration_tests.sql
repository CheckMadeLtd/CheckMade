-- This script is executed in CI environment by the apply_migration.sh script
-- Probably NOT suitable to reuse in local DEV env because of more long-lived DB leading to different id's. 

WITH new_user1 AS (
    INSERT INTO users (mobile, first_name, middle_name, last_name, email, status, language_setting)
        VALUES ('+447538521999', '_Daniel', 'IntegrationTest', '_Gorin', 'daniel-integrtest-checkmade@neocortek.net', 0, 0)
    RETURNING id 
) 

INSERT INTO roles (token, role_type, status, user_id) 
VALUES ('AAA111', 1002, 0, (SELECT id FROM new_user1));

-- To test correct handling of absence of optional value
WITH new_user_without_email AS (
    INSERT INTO users (mobile, first_name, middle_name, last_name, status, language_setting)
        VALUES ('+4999999999', '_Patrick','IntegrationTest', '_Bauer', 0, 0)
        RETURNING id
)

INSERT INTO roles (token, role_type, status, user_id)
VALUES ('AAA112', 1003, 0, (SELECT id FROM new_user_without_email));

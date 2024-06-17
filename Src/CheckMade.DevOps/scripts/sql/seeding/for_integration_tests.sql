-- This script is executed in CI environment by the apply_migration.sh script 

INSERT INTO users (mobile, first_name, last_name, email, status) 
VALUES ('+447538521741', 'Daniel', 'Gorin', 'dan-cm-testing@neocortek.net', 0); 

INSERT INTO roles (token, role_type, status, user_id) VALUES ('AAA111', 1002, 0, 1);

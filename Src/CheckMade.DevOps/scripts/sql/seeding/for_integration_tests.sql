-- This script is executed in CI environment by the apply_migration.sh script 

INSERT INTO roles (token, role_type, status) VALUES ('AAA111', 1002, 0);
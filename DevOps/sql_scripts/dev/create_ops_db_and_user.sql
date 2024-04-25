CREATE DATABASE cm_ops;
-- for local development only, hence save
CREATE USER cm_app_user WITH PASSWORD 'my_local_dev_db_psw';
GRANT ALL PRIVILEGES ON DATABASE cm_ops TO cm_app_user;

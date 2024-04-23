CREATE DATABASE cm_ops;
-- No security breach here, this psw is for the local dev DB only!
CREATE USER cm_app_user LOGIN PASSWORD 'my_local_dev_db_psw';
GRANT ALL PRIVILEGES ON DATABASE cm_ops TO cm_app_user;

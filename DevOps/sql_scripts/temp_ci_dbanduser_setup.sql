CREATE DATABASE cm_ops;
CREATE USER cm_app_user WITH PASSWORD 'my_ci_db_psw';
GRANT ALL PRIVILEGES ON DATABASE cm_ops TO cm_app_user;

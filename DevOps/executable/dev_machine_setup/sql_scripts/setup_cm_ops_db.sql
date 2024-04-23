-- This script requires superuser rights i.e. cannot be run directly from within Rider. Run e.g. via CLI with:
-- `psql -U [superuser] -d postgres -f [path to this sql script]`

CREATE DATABASE cm_ops;

-- No security breach here, this psw is for the local dev DB only!
CREATE USER cm_app_user LOGIN PASSWORD 'my_local_dev_db_psw';
GRANT ALL PRIVILEGES ON DATABASE cm_ops TO cm_app_user;

CREATE TABLE messages (
    ID SERIAL PRIMARY KEY,
    Details jsonb NOT NULL
);

GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE messages TO cm_app_user;

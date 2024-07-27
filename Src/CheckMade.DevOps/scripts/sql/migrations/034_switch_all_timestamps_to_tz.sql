ALTER TABLE live_events DROP COLUMN start_date;
ALTER TABLE live_events ADD COLUMN start_date timestamptz NOT NULL DEFAULT current_timestamp;
ALTER TABLE live_events ALTER COLUMN start_date DROP DEFAULT;

ALTER TABLE live_events DROP COLUMN end_date;
ALTER TABLE live_events ADD COLUMN end_date timestamptz NOT NULL DEFAULT current_timestamp;
ALTER TABLE live_events ALTER COLUMN end_date DROP DEFAULT;

ALTER TABLE tlg_agent_role_bindings DROP COLUMN activation_date;
ALTER TABLE tlg_agent_role_bindings ADD COLUMN activation_date timestamptz NOT NULL DEFAULT current_timestamp;
ALTER TABLE tlg_agent_role_bindings ALTER COLUMN activation_date DROP DEFAULT;

ALTER TABLE tlg_agent_role_bindings DROP COLUMN deactivation_date;
ALTER TABLE tlg_agent_role_bindings ADD COLUMN deactivation_date timestamptz;

ALTER TABLE users_employment_history DROP COLUMN start_date;
ALTER TABLE users_employment_history ADD COLUMN start_date timestamptz NOT NULL DEFAULT current_timestamp;
ALTER TABLE users_employment_history ALTER COLUMN start_date DROP DEFAULT;

ALTER TABLE users_employment_history DROP COLUMN end_date;
ALTER TABLE users_employment_history ADD COLUMN end_date timestamptz;

-- Script to remove last_data_migration column from all tables
-- PostgreSQL will automatically handle dropping any associated indexes/constraints

ALTER TABLE agent_role_bindings DROP COLUMN last_data_migration;
ALTER TABLE derived_workflow_bridges DROP COLUMN last_data_migration;
ALTER TABLE derived_workflow_states DROP COLUMN last_data_migration;
ALTER TABLE inputs DROP COLUMN last_data_migration;
ALTER TABLE live_event_series DROP COLUMN last_data_migration;
ALTER TABLE live_event_venues DROP COLUMN last_data_migration;
ALTER TABLE live_events DROP COLUMN last_data_migration;
ALTER TABLE roles DROP COLUMN last_data_migration;
ALTER TABLE roles_to_spheres_assignments DROP COLUMN last_data_migration;
ALTER TABLE spheres_of_action DROP COLUMN last_data_migration;
ALTER TABLE users DROP COLUMN last_data_migration;
ALTER TABLE users_employment_history DROP COLUMN last_data_migration;
ALTER TABLE vendors DROP COLUMN last_data_migration;
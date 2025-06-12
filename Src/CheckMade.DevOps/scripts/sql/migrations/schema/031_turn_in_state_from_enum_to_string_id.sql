ALTER TABLE derived_workflow_states DROP COLUMN in_state;

ALTER TABLE derived_workflow_states ADD COLUMN in_state varchar(6) DEFAULT '';
ALTER TABLE derived_workflow_states ALTER COLUMN in_state SET NOT NULL;
ALTER TABLE derived_workflow_states ALTER COLUMN in_state DROP DEFAULT;

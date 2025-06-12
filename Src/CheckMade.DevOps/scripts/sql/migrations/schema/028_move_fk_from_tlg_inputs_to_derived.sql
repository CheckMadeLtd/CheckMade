-- Context: one-to-one relationship between tlg_input and derived_workflow_state
-- In 027 created derived table with FK from inputs to derived
-- But then realised, better to have the FK in derived, referencing input, because I want to be able to easily delete
-- the derived data. The dependency should point from the secondary (derived) to the primary (raw) data. 

ALTER TABLE tlg_inputs DROP COLUMN derived_workflow_states_id;
ALTER TABLE derived_workflow_states ADD COLUMN tlg_inputs_id INTEGER REFERENCES tlg_inputs(id);

ALTER TABLE inputs RENAME COLUMN entity_guid TO workflow_guid;
alter index public.inputs_entity_guid rename to inputs_workflow_guid;
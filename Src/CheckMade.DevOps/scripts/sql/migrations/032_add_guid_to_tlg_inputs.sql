ALTER TABLE tlg_inputs ADD COLUMN entity_guid UUID;
CREATE INDEX tlg_inputs_entity_guid ON tlg_inputs (entity_guid);
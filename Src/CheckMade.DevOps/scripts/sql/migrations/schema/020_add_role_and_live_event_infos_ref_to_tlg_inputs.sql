-- These are optional (e.g. for inputs from unauthorized users) so we don't set them NOT NULL
ALTER TABLE tlg_inputs ADD COLUMN role_id INTEGER REFERENCES roles(id);
ALTER TABLE tlg_inputs ADD COLUMN live_event_id INTEGER REFERENCES live_events(id);

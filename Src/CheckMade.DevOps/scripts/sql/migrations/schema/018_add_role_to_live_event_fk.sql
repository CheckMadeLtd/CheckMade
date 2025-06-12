ALTER TABLE roles ADD COLUMN live_event_id INTEGER REFERENCES live_events(id);
ALTER TABLE roles ALTER COLUMN live_event_id SET NOT NULL;
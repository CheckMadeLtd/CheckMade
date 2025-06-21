-- Should solve one of the slow queries in prd
CREATE INDEX idx_roles_live_event_id ON roles(live_event_id);

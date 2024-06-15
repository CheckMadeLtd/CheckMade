-- This allows us to use 'token' to look up a role e.g. when adding a TlgClientPortModeRole
ALTER TABLE roles ADD CONSTRAINT token_unique UNIQUE (token);
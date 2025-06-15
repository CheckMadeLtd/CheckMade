-- Migration: Remove tlg_ prefixes from tables and columns
-- This script removes tlg_ prefixes from table names, column names, and related database objects
-- Foreign keys and permissions should update automatically in PostgreSQL

-- Step 1: Rename column in derived_workflow_states table
ALTER TABLE derived_workflow_states RENAME COLUMN tlg_inputs_id TO inputs_id;

-- Step 2: Rename tlg_inputs table to inputs
ALTER TABLE tlg_inputs RENAME TO inputs;

-- Step 3: Rename tlg_inputs sequence
ALTER SEQUENCE tlg_inputs_id_seq RENAME TO inputs_id_seq;

-- Step 4: Rename indexes related to inputs table
ALTER INDEX tlg_inputs_pkey RENAME TO inputs_pkey;
ALTER INDEX tlg_inputs_entity_guid RENAME TO inputs_entity_guid;
ALTER INDEX tlg_inputs_user_id RENAME TO inputs_user_id;


-- Step 5: Rename tlg_agent_role_bindings table to agent_role_bindings
ALTER TABLE tlg_agent_role_bindings RENAME TO agent_role_bindings;

-- Step 6: Rename tlg_agent_role_bindings sequence
ALTER SEQUENCE tlg_agent_role_bindings_id_seq RENAME TO agent_role_bindings_id_seq;

-- Step 7: Rename columns in agent_role_bindings table
ALTER TABLE agent_role_bindings RENAME COLUMN tlg_user_id TO user_id;
ALTER TABLE agent_role_bindings RENAME COLUMN tlg_chat_id TO chat_id;

-- Step 8: Rename the complex index for agent_role_bindings
ALTER INDEX tlg_agent_role_bindings_pkey RENAME TO agent_role_bindings_pkey;
ALTER INDEX tlg_agent_role_bindings_role_tlg_user_tlg_chat_mode_when_active
    RENAME TO agent_role_bindings_role_user_chat_mode_when_active;

-- Step 9: Rename foreign key constraints to remove tlg_ prefixes
ALTER TABLE agent_role_bindings RENAME CONSTRAINT tlg_agent_role_bindings_role_id_fkey TO agent_role_bindings_role_id_fkey;
ALTER TABLE inputs RENAME CONSTRAINT tlg_inputs_live_event_id_fkey TO inputs_live_event_id_fkey;
ALTER TABLE inputs RENAME CONSTRAINT tlg_inputs_role_id_fkey TO inputs_role_id_fkey;
ALTER TABLE derived_workflow_states RENAME CONSTRAINT derived_workflow_states_tlg_inputs_id_fkey TO derived_workflow_states_inputs_id_fkey;
    

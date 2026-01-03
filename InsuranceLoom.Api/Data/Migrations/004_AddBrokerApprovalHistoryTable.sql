-- Migration: Add broker_approval_history table for auditing broker approvals
-- Created: 2024-01-03

CREATE TABLE IF NOT EXISTS broker_approval_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    broker_id UUID NOT NULL REFERENCES brokers(id) ON DELETE CASCADE,
    action VARCHAR(50) NOT NULL, -- Registered, Approved, Rejected
    performed_by_manager_id UUID REFERENCES managers(id) ON DELETE SET NULL,
    performed_by_email VARCHAR(255), -- Store email in case manager is deleted
    notes TEXT,
    previous_status VARCHAR(50),
    new_status VARCHAR(50),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_broker_approval_history_broker_id ON broker_approval_history(broker_id);
CREATE INDEX IF NOT EXISTS idx_broker_approval_history_manager_id ON broker_approval_history(performed_by_manager_id);
CREATE INDEX IF NOT EXISTS idx_broker_approval_history_created_at ON broker_approval_history(created_at);
CREATE INDEX IF NOT EXISTS idx_broker_approval_history_action ON broker_approval_history(action);

-- Add comment to table
COMMENT ON TABLE broker_approval_history IS 'Audit trail for broker registration, approval, and rejection actions';


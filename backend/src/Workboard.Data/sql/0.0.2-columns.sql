CREATE TABLE columns (
    id TEXT PRIMARY KEY NOT NULL,
    project_id TEXT NOT NULL,

    name TEXT NOT NULL,
    sort REAL NOT NULL DEFAULT 0,

    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,

    CONSTRAINT fk_columns_project
        FOREIGN KEY (project_id)
        REFERENCES projects (id)
        ON DELETE CASCADE
);

CREATE INDEX ix_columns_project_sort ON columns (project_id, sort);

-- Nullable: existing tasks have no column. Deleting a column un-places its tasks.
ALTER TABLE task_items
    ADD COLUMN column_id TEXT NULL REFERENCES columns (id) ON DELETE SET NULL;

CREATE INDEX ix_task_items_column_id ON task_items (column_id);

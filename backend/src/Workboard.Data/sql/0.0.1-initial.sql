PRAGMA foreign_keys = ON;

CREATE TABLE workspaces (
    id TEXT PRIMARY KEY NOT NULL,
    name TEXT NOT NULL,
    slug TEXT NOT NULL COLLATE NOCASE,
    description TEXT NULL,
    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,
    CONSTRAINT uq_workspaces_slug UNIQUE (slug)
);


CREATE TABLE projects (
    id TEXT PRIMARY KEY NOT NULL,
    workspace_id TEXT NOT NULL,

    name TEXT NOT NULL,
    description TEXT NULL,

    -- Useful for archiving without deleting task history.
    is_archived INTEGER NOT NULL DEFAULT 0
        CHECK (is_archived IN (0, 1)),

    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,

    CONSTRAINT uq_projects_workspace_name UNIQUE (workspace_id, name),

    CONSTRAINT fk_projects_workspace
        FOREIGN KEY (workspace_id)
        REFERENCES workspaces (id)
        ON DELETE CASCADE
);

CREATE INDEX ix_projects_workspace_id ON projects (workspace_id);


CREATE TABLE task_items (
    id TEXT PRIMARY KEY NOT NULL,
    project_id TEXT NOT NULL,

    title TEXT NOT NULL,
    description TEXT NULL,

    sort REAL NOT NULL DEFAULT 0,

    created_at TEXT NOT NULL,
    updated_at TEXT NOT NULL,

    CONSTRAINT fk_task_items_project
        FOREIGN KEY (project_id)
        REFERENCES projects (id)
        ON DELETE CASCADE
);

CREATE INDEX ix_task_items_project_sort ON task_items (project_id, sort);

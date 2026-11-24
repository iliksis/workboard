namespace Workboard.Domain
{
    public sealed record Workspace(Guid Id, string Name, string Slug, string? Description);

    public sealed record Project(Guid Id, Guid WorkspaceId, string Name, string? Description, bool IsArchived);

    public sealed record Column(Guid Id, Guid ProjectId, string Name, float Sort);

    /// <param name="ColumnId">Null while the task is not placed in a column.</param>
    public sealed record TaskItem(Guid Id, Guid ProjectId, Guid? ColumnId, string Title, string? Description, float Sort);

    // Updates: a null field means "leave unchanged" (so a description cannot be cleared yet).
    public sealed record WorkspaceUpdate(string? Name = null, string? Slug = null, string? Description = null);

    public sealed record ProjectUpdate(string? Name = null, string? Description = null, bool? IsArchived = null);

    public sealed record ColumnUpdate(string? Name = null, float? Sort = null);

    public sealed record TaskItemUpdate(string? Title = null, string? Description = null, float? Sort = null);
}

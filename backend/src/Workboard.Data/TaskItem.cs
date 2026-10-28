namespace Workboard.Data
{
    public class TaskItem(Guid id, string title, string? description, float sort = 0) : IEquatable<TaskItem>
    {
        public Guid Id { get; set; } = id;
        public string? Title { get; set; } = title;
        public string? Description { get; set; } = description;
        public float Sort { get; set; } = sort;

        public int CompareTo(TaskItem? other)
        {
            if (other == null) return 1;
            return this.Sort.CompareTo(other.Sort);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is null)
            {
                return false;
            }

            return Equals(obj);
        }

        public bool Equals(TaskItem? other)
        {
            return other?.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Id, this.Title, this.Sort);
        }
    }
}

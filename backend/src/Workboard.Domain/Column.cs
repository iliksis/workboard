namespace Workboard.Domain
{
    public class Column
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public Board? Board { get; set; }
    }
}

namespace Workboard.Domain
{
    public class BoardItem
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public Board? Board { get; set; }
        public Column? Column { get; set; }
    }
}

namespace Workboard.Domain
{
    public class Board
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public Column[]? Columns { get; set; }
    }
}

namespace TaskManager3.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; }
        public string Status { get; set; } = "To Do";

        public int BoardId { get; set; }
        public Board? Board { get; set; }
    }
}

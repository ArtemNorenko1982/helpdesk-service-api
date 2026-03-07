namespace HelpdeskService.Core.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TicketId { get; set; }
    public virtual Ticket Ticket { get; set; } = null!;

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;
}

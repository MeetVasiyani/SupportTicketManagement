using System.ComponentModel.DataAnnotations;

public class TicketStatusLog
{
    public int Id {get; set;}
    [Required]
    public int TicketId {get; set;}
    public Ticket Ticket {get; set;} = null!;

    [Required]
    public TicketsStatus OldStatus {get; set;}

    [Required]
    public TicketsStatus NewStatus {get; set;}
    
    [Required]
    public int ChangedBy {get; set;}
    public User User {get; set;} = null!;
    public DateTime ChangedAt {get; set;} = DateTime.UtcNow;
}
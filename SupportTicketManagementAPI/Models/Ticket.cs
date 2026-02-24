using System.ComponentModel.DataAnnotations;

public enum TicketsStatus
{
    OPEN,IN_PROGRESS,RESOLVED,CLOSED
}
public enum TicketsPriority
{
    LOW,MEDIUM,HIGH
}
public class Ticket
{
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    [MinLength(5)]
    public string Title {get; set;} = null!;

    [Required]
    [MinLength(10)]
    public string Description {get; set;} = null!;

    [Required]
    public TicketsStatus Status {get; set;} = TicketsStatus.OPEN;

    [Required]
    public TicketsPriority Priority {get; set;} = TicketsPriority.MEDIUM;

    [Required]
    public int CreatedBy {get; set;}
    public User CreatingUser{get; set;} = null!;
    public int? AssignedTo {get; set;}
    public User? AssignedUser{get; set;}
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}
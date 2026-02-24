using System.ComponentModel.DataAnnotations;

public class TicketComment
{
    public int Id {get; set;}
    
    [Required]
    public int TicketId {get; set;}
    public Ticket Ticket {get; set;} = null!;

    [Required]
    public int UserId {get; set;}
    public User User{get; set;} = null!;
    
    [Required]
    public string Comment{get; set;} = null!;
    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
}
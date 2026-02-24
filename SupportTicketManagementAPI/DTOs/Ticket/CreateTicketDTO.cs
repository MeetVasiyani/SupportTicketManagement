using System.ComponentModel.DataAnnotations;

public class CreateTicketDTO
{
    [Required]
    [MaxLength(255)]
    [MinLength(5)]
    public string title {get; set;} = null!;

    [Required]
    [MinLength(10)]
    public string description {get; set;} = null!;

    [Required]
    public TicketsPriority priority {get; set;}
}